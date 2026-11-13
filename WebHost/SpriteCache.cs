using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Text.Json;
using ZaTool.Engine;

namespace ZaTool.WebHost;

internal sealed class SpriteCache : IDisposable
{
    private const int Limit = 5 * 1024 * 1024;
    private readonly HttpClient client;
    private readonly SemaphoreSlim network = new(4);
    private readonly ConcurrentDictionary<string, byte[]> memory = new();
    private readonly ConcurrentDictionary<string, byte[]> metadata = new();
    private readonly string directory;
    internal SpriteCache(string dataDirectory, HttpClient? client = null)
    {
        this.client = client ?? new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(8) };
        // v1 cached artwork using incomplete variety resolution; never reuse those identities.
        directory = Path.Combine(dataDirectory, "cache", "pokemon", "v2");
    }
    internal async Task<byte[]> GetAsync(int species, int form, string variant, string? formName, CancellationToken cancellation)
    {
        var forms = Catalog.Forms(species);
        if (variant is not ("normal" or "shiny") || !forms.Any(f => f.Id == form)) throw new ArgumentException("Invalid sprite.");
        string key = $"{species}-{form}-{variant}.png";
        if (memory.TryGetValue(key, out var bytes)) return bytes;
        await network.WaitAsync(cancellation);
        try
        {
            if (memory.TryGetValue(key, out bytes)) return bytes;
            string file = Path.Combine(directory, key);
            if (File.Exists(file) && new FileInfo(file).Length <= Limit)
            {
                bytes = await File.ReadAllBytesAsync(file, cancellation);
                if (ValidPng(bytes)) { Remember(memory, key, bytes); return bytes; }
            }
            var urls = await ResolveAsync(species, form, variant == "shiny", forms, cancellation);
            foreach (var url in urls.Distinct())
            {
                try { bytes = await Fetch(url, cancellation); }
                catch (HttpRequestException) { continue; }
                if (!ValidPng(bytes)) continue;
                Remember(memory, key, bytes);
                await StoreAsync(file, bytes, cancellation);
                return bytes;
            }
        }
        catch (Exception e) when (e is HttpRequestException or IOException or JsonException or InvalidOperationException || e is TaskCanceledException && !cancellation.IsCancellationRequested)
        { /* Never turn an unavailable upstream into a success-shaped blank PNG. */ }
        finally { network.Release(); }
        throw new ApiFailure(404, "sprite_unavailable", "Artwork for this exact species, form and shiny variant is unavailable. Retry when online.");
    }
    private async Task<IReadOnlyList<Uri>> ResolveAsync(int species, int form, bool shiny, IReadOnlyList<Choice> forms, CancellationToken cancellation)
    {
        // Scatterbug/Spewpa carry the future Vivillon pattern but have identical visuals.
        if (forms.Count == 1 && form == 0 || species is 664 or 665) return DirectSprites(species.ToString(CultureInfo.InvariantCulture), shiny);
        var speciesData = await MetadataAsync($"pokemon-species/{species}", cancellation);
        string speciesName = speciesData.GetProperty("name").GetString()!;
        string desired = FormSuffix(species, form, forms.Single(f => f.Id == form).Name);
        var varieties = speciesData.GetProperty("varieties").EnumerateArray().ToArray();
        var exactVariety = varieties.FirstOrDefault(v => v.GetProperty("pokemon").GetProperty("name").GetString() == speciesName + "-" + desired);
        bool varietyMatches = exactVariety.ValueKind != JsonValueKind.Undefined;
        var variety = varietyMatches ? exactVariety : varieties.Single(v => v.GetProperty("is_default").GetBoolean());
        var pokemon = await MetadataAsync(Resource(variety.GetProperty("pokemon").GetProperty("url").GetString()!, "pokemon"), cancellation);
        var formEntry = pokemon.GetProperty("forms").EnumerateArray().FirstOrDefault(f => f.GetProperty("name").GetString() == speciesName + "-" + desired);
        if (formEntry.ValueKind != JsonValueKind.Undefined)
        {
            // Cosmetic patterns live under pokemon-form, not species.varieties. Resolve
            // form zero too: Vivillon 0 = Icy Snow, whereas the API default is Meadow.
            var exactForm = await MetadataAsync(Resource(formEntry.GetProperty("url").GetString()!, "pokemon-form"), cancellation);
            return SpriteUrls(exactForm.GetProperty("sprites"), shiny);
        }
        if (varietyMatches || form == 0 && desired is "normal" or "default" or "standard") return SpriteUrls(pokemon.GetProperty("sprites"), shiny);
        return []; // No visually different base form or non-shiny substitution.
    }
    private static string FormSuffix(int species, int form, string name)
    {
        if (species == 678) return form == 0 ? "male" : "female";
        if (species is 710 or 711 && name is "Medium" or "Average") return "average";
        var slug = new StringBuilder();
        foreach (char c in name.Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsAsciiLetterOrDigit(c)) slug.Append(char.ToLowerInvariant(c));
            else if (slug.Length > 0 && slug[^1] != '-') slug.Append('-');
        }
        return slug.ToString().Trim('-').Replace("alolan", "alola").Replace("galarian", "galar").Replace("hisuian", "hisui").Replace("paldean", "paldea");
    }
    private static string Resource(string url, string kind)
    {
        var uri = new Uri(url);
        string prefix = $"/api/v2/{kind}/";
        if (uri.Scheme != "https" || uri.Host != "pokeapi.co" || !uri.AbsolutePath.StartsWith(prefix, StringComparison.Ordinal) || !int.TryParse(uri.AbsolutePath[prefix.Length..].Trim('/'), out int id) || id <= 0) throw new JsonException("Unexpected sprite metadata resource.");
        return kind + "/" + id;
    }
    private async Task<JsonElement> MetadataAsync(string resource, CancellationToken cancellation)
    {
        if (metadata.TryGetValue(resource, out var bytes)) { using var remembered = JsonDocument.Parse(bytes); return remembered.RootElement.Clone(); }
        string file = Path.Combine(directory, "metadata", resource.Replace('/', '-') + ".json");
        if (File.Exists(file) && new FileInfo(file).Length <= Limit && DateTime.UtcNow - File.GetLastWriteTimeUtc(file) < TimeSpan.FromDays(7))
        {
            try { bytes = await File.ReadAllBytesAsync(file, cancellation); using var cached = JsonDocument.Parse(bytes); Remember(metadata, resource, bytes); return cached.RootElement.Clone(); }
            catch (JsonException) { }
        }
        bytes = await Fetch(new Uri("https://pokeapi.co/api/v2/" + resource + "/"), cancellation);
        using var json = JsonDocument.Parse(bytes);
        Remember(metadata, resource, bytes);
        await StoreAsync(file, bytes, cancellation);
        return json.RootElement.Clone();
    }
    private static IReadOnlyList<Uri> DirectSprites(string id, bool shiny)
    {
        string variant = shiny ? "shiny/" : "";
        string root = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/";
        return [new(root + "other/official-artwork/" + variant + id + ".png"), new(root + "other/home/" + variant + id + ".png"), new(root + variant + id + ".png")];
    }
    private static IReadOnlyList<Uri> SpriteUrls(JsonElement sprites, bool shiny)
    {
        string field = shiny ? "front_shiny" : "front_default";
        var values = new List<Uri>();
        void Add(JsonElement source) { if (source.TryGetProperty(field, out var value) && value.ValueKind == JsonValueKind.String && Uri.TryCreate(value.GetString(), UriKind.Absolute, out var uri) && SafeUri(uri) && uri.AbsolutePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) values.Add(uri); }
        if (sprites.TryGetProperty("other", out var other))
        {
            if (other.TryGetProperty("official-artwork", out var artwork)) Add(artwork);
            if (other.TryGetProperty("home", out var home)) Add(home);
        }
        Add(sprites);
        return values;
    }
    private static void Remember(ConcurrentDictionary<string, byte[]> cache, string key, byte[] bytes) { if (cache.Count >= 128) cache.Clear(); cache[key] = bytes; }
    private static async Task StoreAsync(string file, byte[] bytes, CancellationToken cancellation)
    {
        string temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { Directory.CreateDirectory(Path.GetDirectoryName(file)!); await File.WriteAllBytesAsync(temporary, bytes, cancellation); File.Move(temporary, file, true); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
        finally { try { File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
    }
    private static bool SafeUri(Uri uri) => uri.Scheme == "https" && uri.IsDefaultPort && uri.UserInfo.Length == 0 && uri.Query.Length == 0 && (uri.Host switch { "pokeapi.co" => uri.AbsolutePath.StartsWith("/api/v2/", StringComparison.Ordinal), "raw.githubusercontent.com" => uri.AbsolutePath.StartsWith("/PokeAPI/sprites/master/sprites/pokemon/", StringComparison.Ordinal), _ => false });
    private async Task<byte[]> Fetch(Uri uri, CancellationToken cancellation)
    {
        if (!SafeUri(uri)) throw new HttpRequestException("Sprite host/path rejected.");
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellation);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > Limit) throw new HttpRequestException("Sprite too large.");
        await response.Content.LoadIntoBufferAsync(Limit, cancellation);
        return await response.Content.ReadAsByteArrayAsync(cancellation);
    }
    internal static bool ValidPng(byte[] bytes)
    {
        if (bytes.Length < 45 || bytes.Length > Limit || !bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10})) return false;
        if (!bytes.AsSpan(12,4).SequenceEqual("IHDR"u8) || Read(bytes,8) != 13 || Read(bytes,16) is < 16 or > 4096 || Read(bytes,20) is < 16 or > 4096) return false;
        bool pixels = false;
        for (int offset = 8; offset <= bytes.Length - 12;)
        {
            uint length = Read(bytes, offset);
            if (length > Limit || offset + 12L + length > bytes.Length) return false;
            var chunk = bytes.AsSpan(offset + 4, (int)length + 4);
            uint crc = uint.MaxValue;
            foreach (byte b in chunk) { crc ^= b; for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0u : 0xEDB88320u); }
            if (~crc != Read(bytes, offset + 8 + (int)length)) return false;
            if (chunk[..4].SequenceEqual("IDAT"u8) && length > 0) pixels = true;
            if (chunk[..4].SequenceEqual("IEND"u8)) return length == 0 && pixels && offset + 12 == bytes.Length;
            offset += (int)length + 12;
        }
        return false;
    }
    private static uint Read(byte[] bytes, int offset) => System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset,4));
    public void Dispose() { client.Dispose(); network.Dispose(); }
}
