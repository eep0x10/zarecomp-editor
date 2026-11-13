using System.Buffers.Binary;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using ZaTool.Engine;
using ZaTool.WebHost;

internal static class SpriteTests
{
    private static readonly string[] Patterns = ["icy-snow", "polar", "tundra", "continental", "garden", "elegant", "meadow", "modern", "marine", "archipelago", "high-plains", "sandstorm", "river", "monsoon", "savanna", "sun", "ocean", "jungle", "fancy", "poke-ball"];
    internal static async Task Run(bool live)
    {
        string root = Path.Combine(AppContext.BaseDirectory, "data", "sprites-" + Guid.NewGuid().ToString("N"));
        int passed = 0;
        void Check(bool value, string label) { if (!value) throw new Exception(label); passed++; Console.WriteLine("PASS " + label); }
        var handler = new FixtureHandler();
        var forms = Catalog.Forms(666);
        Check(forms.Count == 20 && forms[0].Name == "Icy Snow" && forms[6].Name == "Meadow", "Vivillon Engine form order differs from API default");
        Directory.CreateDirectory(Path.Combine(root, "cache", "pokemon", "v2"));
        byte[] blank = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=");
        File.WriteAllBytes(Path.Combine(root, "cache", "pokemon", "v2", "666-0-normal.png"), blank);
        using (var cache = new SpriteCache(root, new HttpClient(handler)))
        {
            foreach (var form in forms)
            {
                var normal = await cache.GetAsync(666, form.Id, "normal", "untrusted-ignored", default);
                var shiny = await cache.GetAsync(666, form.Id, "shiny", null, default);
                Check(normal.SequenceEqual(Png((byte)(form.Id + 1))) && shiny.SequenceEqual(Png((byte)(form.Id + 101))), $"Vivillon {form.Id} {form.Name}: exact normal + shiny identity");
            }
            Check(handler.Requests.Any(p => p.EndsWith("other/home/666-continental.png")), "missing exact official artwork falls back to exact HOME form");
            Check(!handler.Requests.Any(p => p.EndsWith("/666.png")), "no implicit Meadow/base substitution for other patterns");
            int requests = handler.Requests.Count;
            await cache.GetAsync(666, 0, "normal", null, default);
            Check(handler.Requests.Count == requests, "memory cache avoids repeated requests");
        }
        Check(SpriteCache.ValidPng(File.ReadAllBytes(Path.Combine(root, "cache", "pokemon", "v2", "666-0-normal.png"))), "legacy blank cache rejected and repaired");
        var offline = new FixtureHandler { Offline = true };
        using (var disk = new SpriteCache(root, new HttpClient(offline)))
        {
            Check((await disk.GetAsync(666, 0, "normal", null, default)).SequenceEqual(Png(1)), "exact disk cache works offline across instances");
            Check(offline.Requests.Count == 0, "offline disk hit needs no metadata network");
        }
        string missingRoot = root + "-missing";
        var missing = new FixtureHandler { MissingImages = true };
        using (var cache = new SpriteCache(missingRoot, new HttpClient(missing)))
        {
            bool absent = false;
            try { await cache.GetAsync(666, 4, "shiny", null, default); } catch (ApiFailure e) { absent = e.Status == 404 && e.Code == "sprite_unavailable"; }
            Check(absent, "missing exact variant returns explicit 404, not blank success");
            Check(!missing.Requests.Any(p => p.EndsWith(".png") && !p.Contains("/shiny/")), "missing shiny never substitutes normal artwork");
            missing.MissingImages = false;
            Check((await cache.GetAsync(666, 4, "shiny", null, default)).SequenceEqual(Png(105)), "failed retrieval is not negatively cached");
        }
        Check(!SpriteCache.ValidPng(blank) && !SpriteCache.ValidPng(Png(1)[..24]), "1px placeholders and truncated PNG rejected");
        var damaged = Png(1); damaged[40] ^= 1;
        Check(!SpriteCache.ValidPng(damaged), "PNG chunk corruption rejected");
        if (live)
        {
            using var cache = new SpriteCache(root + "-live");
            foreach (var sample in new[] { (666,0), (666,3), (666,6), (666,19), (669,0), (669,3), (670,4), (671,1), (676,0), (676,7), (710,0), (710,1), (585,0), (585,2) })
            {
                if (!Catalog.Forms(sample.Item1).Any(f => f.Id == sample.Item2)) { Console.WriteLine($"SKIP form outside ZA policy: {sample}"); continue; }
                foreach (var variant in new[] { "normal", "shiny" })
                {
                    try { var image = await cache.GetAsync(sample.Item1, sample.Item2, variant, null, default); Check(SpriteCache.ValidPng(image), $"live exact artwork {sample.Item1}/{sample.Item2}/{variant}, {image.Length} bytes"); }
                    catch (ApiFailure e) { Console.WriteLine($"MISSING {sample.Item1}/{sample.Item2}/{variant}: {e.Code}"); }
                }
            }
        }
        Console.WriteLine($"SUCCESS {passed} sprite checks; generated fixture images and mock HTTP by default{(live ? ", plus live PokéAPI probes" : "")}.");
    }
    private sealed class FixtureHandler : HttpMessageHandler
    {
        internal List<string> Requests { get; } = [];
        internal bool Offline, MissingImages;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string path = request.RequestUri!.AbsolutePath; Requests.Add(path);
            if (Offline) throw new HttpRequestException("Offline test.");
            object? data = null;
            if (path == "/api/v2/pokemon-species/666/") data = new { name = "vivillon", varieties = new[] { new { is_default = true, pokemon = new { name = "vivillon", url = "https://pokeapi.co/api/v2/pokemon/666/" } } } };
            if (path == "/api/v2/pokemon/666/") data = new { forms = Patterns.Select((pattern, index) => new { name = "vivillon-" + pattern, url = $"https://pokeapi.co/api/v2/pokemon-form/{10000 + index}/" }) };
            if (path.StartsWith("/api/v2/pokemon-form/") && int.TryParse(path.Split('/')[4], out int resource))
            {
                string prefix = "https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/";
                string pattern = Patterns[resource - 10000];
                object Pair(string folder) => new { front_default = prefix + folder + "666-" + pattern + ".png", front_shiny = prefix + folder + "shiny/666-" + pattern + ".png" };
                data = new { sprites = new Dictionary<string, object> { ["other"] = new Dictionary<string, object> { ["official-artwork"] = Pair("other/official-artwork/"), ["home"] = Pair("other/home/") }, ["front_default"] = prefix + "666-" + pattern + ".png", ["front_shiny"] = prefix + "shiny/666-" + pattern + ".png" } };
            }
            if (data != null) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(data)) });
            int index = Array.FindIndex(Patterns, pattern => path.EndsWith("666-" + pattern + ".png"));
            if (index < 0 || MissingImages || path.EndsWith("666-continental.png") && path.Contains("official-artwork")) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(Png((byte)(index + (path.Contains("/shiny/") ? 101 : 1)))) });
        }
    }
    private static byte[] Png(byte color)
    {
        using var output = new MemoryStream(); output.Write(new byte[] {137,80,78,71,13,10,26,10});
        void Chunk(string name, byte[] data)
        {
            var length = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(length, data.Length); output.Write(length);
            byte[] chunk = Encoding.ASCII.GetBytes(name).Concat(data).ToArray(); output.Write(chunk);
            uint crc = uint.MaxValue; foreach (byte b in chunk) { crc ^= b; for (int bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0u : 0xEDB88320u); }
            BinaryPrimitives.WriteUInt32BigEndian(length, ~crc); output.Write(length);
        }
        byte[] header = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(header, 32); BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), 32); header[8] = 8; header[9] = 6; Chunk("IHDR", header);
        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, true)) for (int y = 0; y < 32; y++) { zlib.WriteByte(0); for (int x = 0; x < 32; x++) zlib.Write(new byte[] {color, (byte)(255-color), 80, 255}); }
        Chunk("IDAT", compressed.ToArray()); Chunk("IEND", []); return output.ToArray();
    }
}
