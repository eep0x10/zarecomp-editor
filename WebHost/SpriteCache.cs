using System.Collections.Concurrent;
using System.Text.Json;
using ZaTool.Engine;

namespace ZaTool.WebHost;

internal sealed class SpriteCache : IDisposable
{
    private const int Limit = 5 * 1024 * 1024;
    private readonly HttpClient client = new(new HttpClientHandler { AllowAutoRedirect = false }) { Timeout = TimeSpan.FromSeconds(8) };
    private readonly SemaphoreSlim network = new(4);
    private readonly ConcurrentDictionary<string, byte[]> memory = new();
    private readonly string directory;
    private static readonly byte[] Fallback = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
    public SpriteCache(string dataDirectory) { directory = Path.Combine(dataDirectory, "cache", "pokemon"); }
    public async Task<byte[]> GetAsync(int species, int form, string variant, string? formName, CancellationToken cancellation)
    {
        if (variant is not ("normal" or "shiny") || !Catalog.Forms(species).Any(f => f.Id == form)) throw new ArgumentException("Invalid sprite.");
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
                if (ValidPng(bytes)) { Remember(key, bytes); return bytes; }
            }
            int spriteId = species;
            if (form != 0 && species is not (664 or 665))
            {
                var metadata = await Fetch(new Uri($"https://pokeapi.co/api/v2/pokemon-species/{species}/"), cancellation);
                using var document = JsonDocument.Parse(metadata);
                // Derive form names from trusted Engine catalog, never use a client URL.
                string suffix = Catalog.Forms(species).Single(f => f.Id == form).Name.ToLowerInvariant().Replace(" ", "-").Replace("alolan", "alola").Replace("galarian", "galar").Replace("hisuian", "hisui").Replace("paldean", "paldea");
                spriteId = 0;
                foreach (var variety in document.RootElement.GetProperty("varieties").EnumerateArray())
                {
                    var pokemon = variety.GetProperty("pokemon");
                    if (!(pokemon.GetProperty("name").GetString() ?? "").EndsWith("-" + suffix, StringComparison.Ordinal)) continue;
                    var uri = new Uri(pokemon.GetProperty("url").GetString()!);
                    if (uri.Scheme == "https" && uri.Host == "pokeapi.co" && int.TryParse(uri.Segments.Last().Trim('/'), out int id)) spriteId = id;
                }
                if (spriteId <= 0) return Fallback;
            }
            bytes = await Fetch(new Uri($"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{(variant == "shiny" ? "shiny/" : "")}{spriteId}.png"), cancellation);
            if (!ValidPng(bytes)) return Fallback;
            Remember(key, bytes);
            string temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { Directory.CreateDirectory(directory); await File.WriteAllBytesAsync(temporary, bytes, cancellation); File.Move(temporary, file, true); }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException) { }
            finally { try { File.Delete(temporary); } catch { } }
            return bytes;
        }
        catch (Exception e) when (e is HttpRequestException or IOException or JsonException or TaskCanceledException or InvalidOperationException) { return Fallback; }
        finally { network.Release(); }
    }
    private void Remember(string key, byte[] bytes) { if (memory.Count >= 128) memory.Clear(); memory[key] = bytes; }
    private async Task<byte[]> Fetch(Uri uri, CancellationToken cancellation)
    {
        if (uri.Scheme != "https" || uri.Host is not ("pokeapi.co" or "raw.githubusercontent.com")) throw new HttpRequestException("Sprite host rejected.");
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellation);
        response.EnsureSuccessStatusCode();
        if (response.Content.Headers.ContentLength > Limit) throw new HttpRequestException("Sprite too large.");
        await response.Content.LoadIntoBufferAsync(Limit, cancellation);
        return await response.Content.ReadAsByteArrayAsync(cancellation);
    }
    private static bool ValidPng(byte[] bytes) => bytes.Length is >= 24 and <= Limit && bytes.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}) && System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(16,4)) is > 0 and <= 4096 && System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(20,4)) is > 0 and <= 4096;
    public void Dispose() { client.Dispose(); network.Dispose(); }
}
