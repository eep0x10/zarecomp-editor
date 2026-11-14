using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using ZaTool.WebHost;
using ZaTool.Engine;
using PKHeX.Core;

if (args.Length == 1 && args[0] is "--sprites" or "--sprites-live") { await SpriteTests.Run(args[0] == "--sprites-live"); return; }
if (args.Length != 1) throw new ArgumentException("Use --generated or pass an approved synthetic fixture main path.");
string root = Path.Combine(AppContext.BaseDirectory, "data", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
string source;
if (args[0] is "--generated" or "--alpha-probe")
{
    string sourceDirectory = Path.Combine(root, "generated_source", "save_0");
    Directory.CreateDirectory(sourceDirectory);
    source = Path.Combine(sourceDirectory, "main");
    var generated = new SAV9ZA { Version = GameVersion.ZA, OT = "Demo", Language = 2 };
    // BlankBlocks intentionally omit stored types. Assign object types only in this synthetic
    // fixture helper; normal save loading and production Engine never use reflection.
    var storedType = typeof(SCBlock).GetProperty(nameof(SCBlock.Type))!;
    foreach (var block in generated.AllBlocks.Where(b => b.Type == SCTypeCode.None))
        storedType.SetValue(block, block.Data.Length switch { 0 => SCTypeCode.Bool1, 1 => SCTypeCode.Byte, 2 => SCTypeCode.UInt16, 4 => SCTypeCode.UInt32, 8 => SCTypeCode.UInt64, _ => SCTypeCode.Object });
    generated.Blocks.GetBlock(SaveBlockAccessor9ZA.KBoxesUnlocked).ChangeStoredType(SCTypeCode.Byte);
    generated.BoxesUnlocked = 8;
    byte[] encrypted = generated.Write().ToArray();
    // The detector accepts fixed retail lengths. Blank type inference omits optional
    // game blocks; pad with an explicitly synthetic inert object, without changing
    // production detection or taking a block/data sample from a game save.
    const int detectedSize = 0x309FB3;
    int paddingSize = detectedSize - encrypted.Length - 9; // object key/type/length overhead
    if (paddingSize < 0) throw new Exception("Blank fixture schema grew; review synthetic type inference.");
    var padding = (SCBlock)Activator.CreateInstance(typeof(SCBlock), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [uint.MaxValue, SCTypeCode.Object, new Memory<byte>(new byte[paddingSize])], null)!;
    var blocks = generated.AllBlocks.Append(padding).OrderBy(b => b.Key).ToArray();
    generated = (SAV9ZA)Activator.CreateInstance(typeof(SAV9ZA), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, [blocks], null)!;
    encrypted = generated.Write().ToArray();
    if (encrypted.Length != detectedSize || SaveUtil.GetSaveFile(encrypted.ToArray()) is not SAV9ZA) throw new Exception("Generated fixture must pass normal detection.");
    File.WriteAllBytes(source, encrypted);
    new SaveService().Add(source, new AddOptions { Species = 4, Level = 20, Destination = Destination.Party });
    Console.WriteLine("Generated synthetic encrypted save from blank vendor blocks; no input save required.");
}
else source = Path.GetFullPath(args[0]);
if (args[0] == "--alpha-probe") { AlphaTests.Probe(source); return; }
string Hash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
string sourceHash = Hash(source);
if(args[0]=="--generated")
{
    AlphaTests.Run(source);
    OptimizeTests.Run(source);
}
string saveDirectory = Path.Combine(root, "save_0"); Directory.CreateDirectory(saveDirectory);
string path = Path.Combine(saveDirectory, "main"); File.Copy(source, path);
File.WriteAllText(Path.Combine(saveDirectory, "metadata.txt"), "synthetic preservation probe");
int passed = 0;
void Check(bool condition, string label) { if (!condition) throw new Exception(label); Console.WriteLine("PASS " + label); passed++; }
await using var host = new WorkbenchHost(new HostOptions { DataDirectory = root, InitialPath = path });
await host.StartAsync();
string token = host.BrowserUrl.Split("#token=")[1];
using var client = new HttpClient(new HttpClientHandler { UseCookies = false }) { BaseAddress = new Uri(host.Address), Timeout = TimeSpan.FromSeconds(45) };
async Task<HttpResponseMessage> Send(string method, string endpoint, object? data = null, bool auth = true, string? origin = "valid", string? requestHost = null, string? cookie = null, string? fetchSite = null)
{
    using var request = new HttpRequestMessage(new HttpMethod(method), endpoint);
    if (auth) request.Headers.Authorization = new("Bearer", token);
    if (origin != null) request.Headers.Add("Origin", origin == "valid" ? host.Address : origin);
    if (requestHost != null) request.Headers.Host = requestHost;
    if (cookie != null) request.Headers.Add("Cookie", cookie);
    if (fetchSite != null) request.Headers.Add("Sec-Fetch-Site", fetchSite);
    if (method is not ("GET" or "DELETE")) request.Content = new StringContent(JsonSerializer.Serialize(data ?? new { }, WorkbenchHost.Json), Encoding.UTF8, "application/json");
    return await client.SendAsync(request);
}
async Task<JsonElement> Body(HttpResponseMessage response) { string text = await response.Content.ReadAsStringAsync(); if (!response.IsSuccessStatusCode) throw new Exception($"Unexpected status {(int)response.StatusCode}: {text}"); return JsonDocument.Parse(text).RootElement.Clone(); }
Check((await Send("GET", "/api/v1/bootstrap", auth: false)).StatusCode == HttpStatusCode.Unauthorized, "missing token rejected");
Check((await Send("GET", "/api/v1/bootstrap", origin: "https://example.com")).StatusCode == HttpStatusCode.Forbidden, "foreign Origin rejected");
Check((await Send("GET", "/api/v1/bootstrap", requestHost: "evil.example")).StatusCode == HttpStatusCode.Forbidden, "foreign Host rejected");
Check((await Send("POST", "/api/v1/heartbeat", origin: null)).StatusCode == HttpStatusCode.Forbidden, "missing mutation Origin rejected");
var bootstrapResponse = await Send("GET", "/api/v1/bootstrap");
var bootstrap = await Body(bootstrapResponse);
Check(bootstrap.GetProperty("version").GetString() == typeof(WorkbenchHost).Assembly.GetName().Version!.ToString(3), "bootstrap version matches the embedded host assembly");
bool formsContract = true;
foreach (var species in Catalog.Species)
{
    var expectedForms = Catalog.Forms(species.Id);
    var availableForms = await Body(await Send("GET", $"/api/v1/catalogs/species/{species.Id}/forms"));
    formsContract &= availableForms.ValueKind == JsonValueKind.Array && availableForms.GetArrayLength() == expectedForms.Count
        && availableForms.EnumerateArray().Select(f => (f.GetProperty("id").GetInt32(), f.GetProperty("name").GetString())).SequenceEqual(expectedForms.Select(f => (f.Id, (string?)f.Name)));
}
Check(formsContract, "forms list Choice array matches Engine for every catalog species");
var nonzeroFirst = Catalog.Species.FirstOrDefault(s => Catalog.Forms(s.Id)[0].Id != 0);
if (nonzeroFirst != null)
{
    var availableForms = await Body(await Send("GET", $"/api/v1/catalogs/species/{nonzeroFirst.Id}/forms"));
    int firstForm = availableForms[0].GetProperty("id").GetInt32();
    var formMetadata = await Body(await Send("GET", $"/api/v1/catalogs/species/{nonzeroFirst.Id}/forms/{firstForm}"));
    Check(firstForm != 0 && formMetadata.GetProperty("baseStats").GetArrayLength() == 6 && formMetadata.GetProperty("genders").GetArrayLength() > 0, "species without form zero lists forms and loads first valid metadata");
}
else Console.WriteLine("SKIP nonzero-first form probe: current Engine catalog has form zero for every listed species.");
Check(bootstrapResponse.Headers.GetValues("Content-Security-Policy").Single().Contains("img-src 'self' data: blob:"), "CSP permits authenticated artwork blobs");
var patchKeys = JsonSerializer.SerializeToElement(new ZaTool.Engine.PokemonPatch { IVs = [1,2,3,4,5,6], EVs = [0,0,0,0,0,0], PP = [1,1,1,1], PPUps = [0,0,0,0], TID16 = 1, SID16 = 2 }, WorkbenchHost.Json);
Check(patchKeys.TryGetProperty("iVs", out _) && patchKeys.TryGetProperty("eVs", out _) && patchKeys.TryGetProperty("pp", out _) && patchKeys.TryGetProperty("ppUps", out _) && patchKeys.TryGetProperty("tiD16", out _) && patchKeys.TryGetProperty("siD16", out _), "acronym field names match frontend contract");
Check(bootstrapResponse.Headers.GetValues("Set-Cookie").Single().Contains("httponly", StringComparison.OrdinalIgnoreCase) && bootstrapResponse.Headers.GetValues("Set-Cookie").Single().Contains("samesite=strict", StringComparison.OrdinalIgnoreCase), "session cookie HttpOnly and Strict");
string cookie = bootstrapResponse.Headers.GetValues("Set-Cookie").Single().Split(';')[0];
Check((await Send("GET", "/api/v1/bootstrap", auth: false, origin: null, cookie: cookie, fetchSite: "same-origin")).IsSuccessStatusCode, "HttpOnly session supports reload");
Check((await Send("GET", "/api/v1/bootstrap", auth: false, origin: null, cookie: cookie, fetchSite: "same-site")).StatusCode == HttpStatusCode.Unauthorized, "cookie rejected from another local origin");
var opened = await Body(await Send("POST", "/api/v1/sessions", new { path }));
string id = opened.GetProperty("id").GetString()!, hash = opened.GetProperty("hash").GetString()!;
Check(hash == Hash(path), "coherent snapshot hash");
string session = "/api/v1/sessions/" + id;
var alphaPlan=await Body(await Send("POST",session+"/alpha-plan-add",new{expectedHash=hash,options=new{species=4,level=10,destination="Box"},mode="Compatible"}));
Check(alphaPlan.GetProperty("available").GetBoolean()&&alphaPlan.GetProperty("options").GetProperty("creationSeed").ValueKind==JsonValueKind.String,"Alpha planning API returns applicable seeded options with 64-bit string");
Check((await Send("POST",session+"/alpha-plan-add",new{expectedHash="STALE",options=new{species=4,level=10},mode="Compatible"})).StatusCode==HttpStatusCode.Conflict,"stale Alpha plan rejected");
Check((await Send("POST",session+"/preview-add",new{expectedHash=hash,options=new{species=4,level=10,creationSeed="184467440737095516160"}})).StatusCode==HttpStatusCode.BadRequest,"overflowed creation seed rejected as JSON request error");
Check(Hash(path)==hash,"Alpha plan API never writes save");
Check((await Send("POST", session + "/edits", new { expectedHash = hash, operationId = Guid.NewGuid().ToString(), teamEdits = (object?)null, boxEdits = Array.Empty<object>() })).StatusCode == HttpStatusCode.BadRequest, "null team edits rejected as request error");
Check((await Send("POST", session + "/edits", new { expectedHash = hash, operationId = Guid.NewGuid().ToString(), teamEdits = new object?[] { null }, boxEdits = Array.Empty<object>() })).StatusCode == HttpStatusCode.BadRequest, "null edit entry rejected as request error");
Check((await Send("POST", session + "/suggestions", new { expectedHash = hash, team = new object?[] { null } })).StatusCode == HttpStatusCode.BadRequest, "null suggestion entry rejected as request error");
var details = await Body(await Send("GET", session + "/pokemon/party/0"));
Check(details.GetProperty("readOnly").GetProperty("tracker").ValueKind == JsonValueKind.String, "64-bit tracker serialized as string");
var preview = await Body(await Send("POST", session + "/preview", new { expectedHash = hash, party = true, slot = 0, patch = new { nickname = "HostDraft" } }));
Check(preview.GetProperty("values").GetProperty("nickname").GetString() == "HostDraft" && Hash(path) == hash, "preview never writes");
Check((await Send("POST", session + "/preview", new { expectedHash = "STALE", party = true, slot = 0, patch = new { level = 40 } })).StatusCode == HttpStatusCode.Conflict && Hash(path) == hash, "stale preview rejected without write");
var invalid = await Send("POST", session + "/preview", new { expectedHash = hash, party = true, slot = 0, patch = new { level = 101 } });
Check(invalid.StatusCode == HttpStatusCode.UnprocessableEntity && (await invalid.Content.ReadAsStringAsync()).Contains("invalid_level"), "field-coded level validation");
Check((await Send("POST", session + "/preview", new { expectedHash = hash, party = true, slot = 0, patch = new { shiny = 999 } })).StatusCode == HttpStatusCode.BadRequest, "numeric enums rejected");
Check((await Send("POST", session + "/preview", new { expectedHash = hash, party = true, slot = 0, patch = new { nickname = "ok", unknownField = true } })).StatusCode == HttpStatusCode.BadRequest, "unknown fields rejected");
using (var request = new HttpRequestMessage(HttpMethod.Post, session + "/preview"))
{
    request.Headers.Authorization = new("Bearer", token); request.Headers.Add("Origin", host.Address);
    request.Content = new StringContent("{\"expectedHash\":\"" + hash + "\",\"party\":true,\"slot\":0,\"patch\":{\"level\":NaN}}", Encoding.UTF8, "application/json");
    Check((await client.SendAsync(request)).StatusCode == HttpStatusCode.BadRequest, "NaN JSON rejected");
}
Check(Hash(path) == hash && !Directory.Exists(Path.Combine(root, "ZA-Tool-Backups")), "read and validation failures have no backups or save writes");
var edit = new { expectedHash = hash, operationId = Guid.NewGuid().ToString(), teamEdits = new[] { new { slot = 0, details = new { nickname = "HostCommit" } } }, boxEdits = Array.Empty<object>() };
var receipt = await Body(await Send("POST", session + "/edits", edit));
Check(receipt.GetProperty("committed").GetBoolean() && Hash(path) != hash, "explicit save commits");
string backup = receipt.GetProperty("backupDirectory").GetString()!;
Check(File.Exists(Path.Combine(backup, "main")) && Hash(Path.Combine(backup, "main")) == hash && File.ReadAllText(Path.Combine(backup, "metadata.txt")) == "synthetic preservation probe", "full-folder backup matches original");
int backups = Directory.GetDirectories(Path.GetDirectoryName(backup)!).Length;
var replay = await Body(await Send("POST", session + "/edits", edit));
Check(replay.GetProperty("hash").GetString() == receipt.GetProperty("hash").GetString() && Directory.GetDirectories(Path.GetDirectoryName(backup)!).Length == backups, "matching operation replay does not write twice");
Check((await Send("POST", session + "/edits", new { expectedHash = hash, operationId = edit.operationId, teamEdits = new[] { new { slot = 0, level = 20 } }, boxEdits = Array.Empty<object>() })).StatusCode == HttpStatusCode.Conflict, "operation ID mismatch rejected");
Check((await Send("POST", session + "/edits", new { expectedHash = hash, operationId = Guid.NewGuid().ToString(), teamEdits = edit.teamEdits, boxEdits = Array.Empty<object>() })).StatusCode == HttpStatusCode.Conflict, "stale commit rejected");
string nextHash = Hash(path);
var addition = await Body(await Send("POST", session + "/add", new { expectedHash = nextHash, operationId = Guid.NewGuid().ToString(), options = new { species = 4, level = 10, destination = "Box" } }));
Check(addition.GetProperty("committed").GetBoolean() && Hash(path) != nextHash, "explicit Add commits with backup");
Check((await Send("POST", "/api/v1/sessions", new { path = @"\\server\share\main" })).StatusCode == HttpStatusCode.UnprocessableEntity, "UNC path rejected");
Check((await Send("POST", "/api/v1/sessions", new { path = "main" })).StatusCode == HttpStatusCode.UnprocessableEntity, "relative path rejected");
Check((await Send("GET", "/api/v1/bootstrap", origin: "null")).StatusCode == HttpStatusCode.Forbidden, "null Origin rejected");
var docs = await client.GetAsync("/openapi/v1.json");
Check(docs.IsSuccessStatusCode && (await docs.Content.ReadAsStringAsync()).Contains("PokemonPatch"), "local OpenAPI schemas available");
var home = await client.GetAsync("/");
Check(home.IsSuccessStatusCode && (await home.Content.ReadAsStringAsync()).Contains("<html"), "embedded frontend index served");
Check((await client.GetAsync("/RecompPokemon.settings.json")).StatusCode == HttpStatusCode.NotFound, "settings never served as assets");
string runtime = Path.Combine(root, "runtime.json");
await host.WriteRuntimeFileAsync(runtime);
var acl = new FileInfo(runtime).GetAccessControl();
var rules = acl.GetAccessRules(true, true, typeof(SecurityIdentifier)).Cast<FileSystemAccessRule>().ToArray();
Check(acl.AreAccessRulesProtected && rules.Length == 1 && rules[0].IdentityReference.Equals(WindowsIdentity.GetCurrent().User), "runtime token file restricted to current user");
Check((await Body(await Send("POST", "/api/v1/heartbeat"))).GetProperty("alive").GetBoolean(), "authenticated heartbeat");
Check((await Send("POST", "/api/v1/shutdown")).IsSuccessStatusCode, "explicit shutdown acknowledged");
Check(await Task.WhenAny(host.WaitForShutdownAsync(), Task.Delay(5000)) != null && host.WaitForShutdownAsync().IsCompleted, "shutdown completes after requests drain");
Check(Hash(source) == sourceHash, "approved source fixture untouched");
Console.WriteLine($"SUCCESS {passed} HTTP integration checks; writes confined to generated synthetic copies.");
