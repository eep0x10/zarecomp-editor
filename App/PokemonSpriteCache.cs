using System.Text.Json;

namespace ZaTool.App;

internal sealed class PokemonSpriteCache
{
    private static readonly HttpClient Client = new() { Timeout = TimeSpan.FromSeconds(12) };
    private static readonly SemaphoreSlim Network = new(4);
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, byte[]> Memory = new();
    private readonly string directory = Path.Combine(AppContext.BaseDirectory, "cache", "pokemon");

    public async Task<Image?> LoadAsync(int species, int form, string formName, bool shiny, CancellationToken cancellation)
    {
        try
        {
            int spriteId = species;
            if (form != 0 && species is not (664 or 665))
            {
                var metadata = await BytesAsync($"species-{species}.json", $"https://pokeapi.co/api/v2/pokemon-species/{species}/", cancellation);
                using var document = JsonDocument.Parse(metadata);
                string suffix = formName.ToLowerInvariant().Replace(" ", "-").Replace("alolan", "alola").Replace("galarian", "galar").Replace("hisuian", "hisui").Replace("paldean", "paldea");
                string? url = null;
                foreach (var variety in document.RootElement.GetProperty("varieties").EnumerateArray())
                {
                    var pokemon = variety.GetProperty("pokemon");
                    string name = pokemon.GetProperty("name").GetString() ?? "";
                    if (suffix.Length > 0 && name.EndsWith("-" + suffix, StringComparison.Ordinal)) { url = pokemon.GetProperty("url").GetString(); break; }
                }
                if (url is null || !int.TryParse(url.TrimEnd('/').Split('/')[^1], out spriteId)) return null;
            }
            string variant = shiny ? "shiny/" : "";
            var bytes = await BytesAsync($"{spriteId}{(shiny ? "-shiny" : "")}.png", $"https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/other/official-artwork/{variant}{spriteId}.png", cancellation);
            using var stream = new MemoryStream(bytes);
            using var source = Image.FromStream(stream);
            return new Bitmap(source);
        }
        catch (OperationCanceledException) { return null; }
        catch { return null; }
    }

    private async Task<byte[]> BytesAsync(string name, string url, CancellationToken cancellation)
    {
        string file = Path.Combine(directory, name);
        if (Memory.TryGetValue(name, out var memory)) return memory;
        if (File.Exists(file))
        {
            try { var cached = await File.ReadAllBytesAsync(file, cancellation); Validate(name, cached); Memory[name] = cached; return cached; }
            catch (OperationCanceledException) { throw; }
            catch { /* Invalid legacy/interrupted caches are fetched again and replaced atomically. */ }
        }
        await Network.WaitAsync(cancellation);
        try
        {
            if (Memory.TryGetValue(name, out memory)) return memory;
            using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            await response.Content.LoadIntoBufferAsync(5 * 1024 * 1024, cancellation);
            byte[] bytes = await response.Content.ReadAsByteArrayAsync(cancellation); Validate(name, bytes); Memory[name] = bytes;
            string temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try { Directory.CreateDirectory(directory); await File.WriteAllBytesAsync(temporary, bytes, cancellation); cancellation.ThrowIfCancellationRequested(); File.Move(temporary, file, true); }
            catch (OperationCanceledException) { throw; }
            catch { /* Read-only portable folders retain downloaded assets in memory. */ }
            finally { try { if (File.Exists(temporary)) File.Delete(temporary); } catch { } }
            return bytes;
        }
        finally { Network.Release(); }
    }
    private static void Validate(string name, byte[] bytes) { using var stream = new MemoryStream(bytes); if (name.EndsWith(".json", StringComparison.Ordinal)) { using var json = JsonDocument.Parse(stream); } else { using var image = Image.FromStream(stream); } }
}
