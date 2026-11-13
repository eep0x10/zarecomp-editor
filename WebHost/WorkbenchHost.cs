using System.Globalization;
using System.Net;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.StaticFiles;
using ZaTool.Engine;

namespace ZaTool.WebHost;

public sealed class WorkbenchHost : IAsyncDisposable
{
    public static readonly JsonSerializerOptions Json = CreateJson();
    private readonly HostOptions options;
    private readonly WorkspaceStore store;
    private readonly SpriteCache sprites;
    private readonly string token = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private readonly string cookieName = "za_" + Guid.NewGuid().ToString("N");
    private readonly string cookieToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    private WebApplication? app;
    private DateTime lastActivity = DateTime.UtcNow;
    private string? runtimeFile;
    public string Address { get; private set; } = "";
    public string BrowserUrl => Address + "/#token=" + token;
    public WorkbenchHost(HostOptions options)
    {
        this.options = options;
        if (options.Port is < 0 or > 65535) throw new ArgumentOutOfRangeException(nameof(options.Port));
        store = new(options.DataDirectory);
        sprites = new(options.DataDirectory);
    }
    private static JsonSerializerOptions CreateJson()
    {
        var json = new JsonSerializerOptions(JsonSerializerDefaults.Web) { UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow };
        json.Converters.Add(new JsonStringEnumConverter(null, false));
        json.Converters.Add(new UInt64StringConverter());
        return json;
    }
    public async Task StartAsync()
    {
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], ApplicationName = typeof(WorkbenchHost).Assembly.FullName, ContentRootPath = AppContext.BaseDirectory });
        // App/environment endpoint configuration must never add non-loopback listeners.
        builder.Configuration.Sources.Clear();
        builder.Logging.ClearProviders();
        builder.WebHost.ConfigureKestrel(k => { k.Listen(IPAddress.Loopback, options.Port); k.Limits.MaxRequestBodySize = 256 * 1024; k.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(10); });
        builder.Services.ConfigureHttpJsonOptions(o => { o.SerializerOptions.PropertyNamingPolicy = Json.PropertyNamingPolicy; o.SerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow; foreach (var c in Json.Converters) o.SerializerOptions.Converters.Add(c); });
        app = builder.Build();
        app.Use(async (context, next) =>
        {
            context.Response.Headers.CacheControl = "no-store";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: blob:; connect-src 'self'; frame-ancestors 'none'; base-uri 'none'; form-action 'none'";
            try
            {
                if (!string.Equals(context.Request.Host.Value, new Uri(Address).Authority, StringComparison.Ordinal)) throw new ApiFailure(403, "host_rejected", "Invalid local host.");
                bool api = context.Request.Path.StartsWithSegments("/api/v1");
                if (api)
                {
                    var bearer = context.Request.Headers.Authorization.ToString();
                    byte[] supplied = Encoding.UTF8.GetBytes(bearer);
                    byte[] required = Encoding.UTF8.GetBytes("Bearer " + token);
                    bool bearerValid = CryptographicOperations.FixedTimeEquals(supplied, required);
                    bool cookieValid = context.Request.Headers["Sec-Fetch-Site"] == "same-origin" && context.Request.Cookies.TryGetValue(cookieName, out var cookie) && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(cookie), Encoding.UTF8.GetBytes(cookieToken));
                    if (!bearerValid && !cookieValid) throw new ApiFailure(401, "unauthorized", "Open the workbench from its launcher.");
                    var origin = context.Request.Headers.Origin.ToString();
                    bool mutation = context.Request.Method is not ("GET" or "HEAD");
                    if ((origin.Length > 0 && origin != Address) || (mutation && origin != Address)) throw new ApiFailure(403, "origin_rejected", "Invalid local origin.");
                    if (mutation && context.Request.Method != "DELETE" && !context.Request.HasJsonContentType()) throw new ApiFailure(415, "json_required", "Use application/json.");
                    if (bearerValid) context.Response.Cookies.Append(cookieName, cookieToken, new CookieOptions { HttpOnly = true, SameSite = SameSiteMode.Strict, Path = "/api/v1", IsEssential = true });
                    lastActivity = DateTime.UtcNow;
                }
                await next(context);
            }
            catch (Exception error) when (!context.Response.HasStarted)
            {
                var failure = Classify(error);
                context.Response.StatusCode = failure.Status;
                await context.Response.WriteAsJsonAsync(new { type = "about:blank", title = failure.Code, status = failure.Status, code = failure.Code, detail = failure.Message, retryable = failure.Status is 423 or 503, traceId = context.TraceIdentifier }, Json, "application/problem+json");
            }
        });
        MapApi(app);
        app.MapGet("/openapi/v1.json", () => Results.Json(ApiDocs.Document(), Json));
        app.MapGet("/api-docs", () => Results.Text(ApiDocs.Html, "text/html; charset=utf-8"));
        app.MapGet("/docs.js", () => Results.Text(ApiDocs.Script, "text/javascript; charset=utf-8"));
        var assets = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(n => n.StartsWith("wwwroot/", StringComparison.Ordinal)).ToDictionary(n => n[8..].Replace('\\', '/'), n => n, StringComparer.Ordinal);
        app.MapGet("/{**path}", (string? path) =>
        {
            path = string.IsNullOrEmpty(path) ? "index.html" : path;
            if (!assets.TryGetValue(path, out var resource)) return Results.NotFound();
            new FileExtensionContentTypeProvider().TryGetContentType(path, out var mime);
            return Results.Stream(Assembly.GetExecutingAssembly().GetManifestResourceStream(resource)!, mime ?? "application/octet-stream");
        });
        await app.StartAsync();
        Address = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.Single().TrimEnd('/');
        if (options.RuntimeFile != null) await WriteRuntimeFileAsync(options.RuntimeFile);
        if (options.IdleTimeout > TimeSpan.Zero) _ = MonitorIdle(app.Lifetime.ApplicationStopping);
    }
    private void MapApi(WebApplication host)
    {
        // All Engine calls stay synchronous within this gate. Its path mutex must be released on its owning thread.
        T Locked<T>(Func<T> action) { lock (store.Gate) return action(); }
        host.MapGet("/api/v1/bootstrap", () => Locked(() => new { version = "2.0.1", locales = new[] { "pt-BR", "en", "es" }, preferences = store.Preferences, initialPath = options.InitialPath ?? (File.Exists(store.Preferences.LastSavePath) ? store.Preferences.LastSavePath : null) }));
        host.MapPost("/api/v1/discovery", () => Locked(store.Discover));
        host.MapPost("/api/v1/sessions", (OpenRequest r) => Locked(() => store.Open(r)));
        host.MapGet("/api/v1/sessions/{id}", (string id) => Locked(() => store.Snapshot(store.Get(id))));
        host.MapDelete("/api/v1/sessions/{id}", (string id) => Locked(() => { store.Close(id); return Results.NoContent(); }));
        host.MapGet("/api/v1/sessions/{id}/boxes/{box:int}", (string id, int box) => Locked(() => { var s = store.Get(id); return store.Coherent(s, null, () => new { box, entries = store.Engine.InspectBox(s.Path, box), hash = WorkspaceStore.FileHash(s.Path) }); }));
        host.MapGet("/api/v1/sessions/{id}/pokemon/{storage}/{slot:int}", (string id, string storage, int slot) => Locked(() => { var s = store.Get(id); bool party = ParseStorage(storage); return store.Coherent(s, null, () => store.Engine.InspectPokemon(s.Path, party, slot)); }));
        host.MapPost("/api/v1/sessions/{id}/preview", (string id, PreviewRequest r) => Locked(() => { RequireHash(r.ExpectedHash); var s = store.Get(id); return store.Coherent(s, r.ExpectedHash, () => store.Engine.PreviewPokemon(s.Path, r.Party, r.Slot, r.Patch)); }));
        host.MapPost("/api/v1/sessions/{id}/preview-add", (string id, PreviewAddRequest r) => Locked(() => { RequireHash(r.ExpectedHash); var s = store.Get(id); return store.Coherent(s, r.ExpectedHash, () => store.Engine.PreviewNewPokemon(s.Path, r.Options)); }));
        host.MapPost("/api/v1/sessions/{id}/suggestions", (string id, SuggestRequest r) => Locked(() => { RequireHash(r.ExpectedHash); var s = store.Get(id); if (r.Team == null || r.Team.Length > 6 || r.Team.Any(p => p == null || p.BaseStats == null || p.BaseStats.Length != 6 || p.Moves == null || p.Moves.Length != 4 || p.MoveCategories == null)) throw new ApiFailure(400, "invalid_team", "Team entries require four moves and six base stats."); return store.Coherent(s, r.ExpectedHash, () => r.Team.Select(p => NatureAdvisor.Suggest(p, r.Language)).ToArray()); }));
        host.MapPost("/api/v1/sessions/{id}/edits", (string id, EditRequest r) => Locked(() => { if (r.TeamEdits == null || r.BoxEdits == null || r.TeamEdits.Any(e => e == null) || r.BoxEdits.Any(e => e == null)) throw new ApiFailure(400, "invalid_edits", "Provide teamEdits and boxEdits arrays with non-null entries."); var s = store.Get(id); return store.Commit(s, r.ExpectedHash, r.OperationId, r, () => store.Engine.SaveEdits(s.Path, r.TeamEdits, r.BoxEdits, r.ExpectedHash)); }));
        host.MapPost("/api/v1/sessions/{id}/add", (string id, AddRequest r) => Locked(() => { var s = store.Get(id); return store.Commit(s, r.ExpectedHash, r.OperationId, r, () => store.Engine.Add(s.Path, r.Options, r.ExpectedHash)); }));
        host.MapGet("/api/v1/catalogs", () => new { Catalog.Species, Catalog.Natures, Catalog.Balls, Catalog.Moves, Catalog.Languages, Catalog.HeldItems, Catalog.MetLocations, Catalog.Ribbons });
        host.MapGet("/api/v1/catalogs/species/{species:int}/forms/{form:int}", (int species, int form) => new { forms = Catalog.Forms(species), genders = Catalog.Genders(species, form), baseStats = Catalog.BaseStats(species, form), plusMoves = Catalog.PlusMoves(species, form) });
        host.MapGet("/api/v1/sprites/{species:int}/{form:int}/{variant}", async (int species, int form, string variant, string? formName, CancellationToken cancellation) => { var bytes = await sprites.GetAsync(species, form, variant, formName, cancellation); return Results.Bytes(bytes, "image/png"); });
        host.MapPut("/api/v1/preferences", (Preferences r) => Locked(() => store.SavePreferences(r)));
        host.MapPost("/api/v1/heartbeat", () => new { alive = true });
        host.MapPost("/api/v1/shutdown", () => { _ = Task.Run(async () => { await Task.Delay(200); lock (store.Gate) host.Lifetime.StopApplication(); }); return Results.Json(new { stopping = true }); });
    }
    private static bool ParseStorage(string storage) => storage switch { "party" => true, "box" => false, _ => throw new ApiFailure(404, "invalid_storage", "Choose party or box.") };
    private static void RequireHash(string? hash) { if (string.IsNullOrWhiteSpace(hash)) throw new ApiFailure(400, "hash_required", "Reload the save first."); }
    private static ApiFailure Classify(Exception e) => e switch
    {
        ApiFailure a => a,
        BadHttpRequestException { StatusCode: 413 } => new(413, "request_too_large", "Request exceeds the 256 KiB limit."),
        BadHttpRequestException or JsonException => new(400, "invalid_request", "Request JSON or parameters are invalid."),
        FileNotFoundException or DirectoryNotFoundException => new(404, "save_not_found", "The save is no longer available."),
        UnauthorizedAccessException => new(422, "access_denied", "This location cannot be accessed. Check folder permissions."),
        InvalidDataException => new(422, "invalid_save", "Save or Pokémon data failed validation."),
        ArgumentException a => DomainError(a),
        InvalidOperationException => new(409, "operation_conflict", "Close the game and check party/box capacity before saving."),
        IOException => new(423, "storage_unavailable", "The save is busy or backup/write verification failed. Reload and inspect backups before retrying."),
        _ => new(500, "internal_error", "The operation failed. No automatic retry was attempted.")
    };
    private static ApiFailure DomainError(ArgumentException error)
    {
        string message = error.Message;
        string field = message.Split(':')[0];
        string[] knownFields = ["Species/Form", "Species", "Form", "Level", "Nickname", "IsNicknamed", "Gender", "Shiny", "Nature", "StatNature", "Ball", "HeldItem", "Language", "Friendship", "IVs", "EVs", "HyperTrained", "Moves", "PP", "PPUps", "RelearnMoves", "PlusMoves", "MetLevel", "MetLocation", "MetDate", "ObedienceLevel", "OriginalTrainerName", "OriginalTrainerGender", "TID16", "SID16", "IsAlpha", "HeightScalar", "WeightScalar", "Scale", "AffixedRibbon", "Favorite", "Markings"];
        if (knownFields.Contains(field) && message.Length <= 250 && !message.Any(char.IsControl)) return new(422, "invalid_" + field.ToLowerInvariant().Replace('/', '_'), message);
        return message switch
        {
            "Nível deve estar entre 1 e 100." => new(422, "invalid_level", "Level must be between 1 and 100. / Nível: 1–100. / Nivel: 1–100."),
            "Natureza inválida." => new(422, "invalid_nature", "Nature must be a catalog choice (0–24)."),
            "EVs não podem somar mais de 510." => new(422, "invalid_evs", "EV total must be 510 or less. / Total de EVs: até 510. / Total de EV: hasta 510."),
            "Informe quatro golpes, sem repetidos, com ao menos um válido." => new(422, "invalid_moves", "Choose four move slots, no duplicate nonempty moves and at least one move."),
            "Um golpe não pode ser aprendido por esta forma no nível escolhido (nível/TM)." => new(422, "invalid_moves", "Choose moves available to this species/form at the selected level."),
            "Espécie ou forma não disponível em Z-A." => new(422, "invalid_species_form", "Choose a persistent species/form from the Z-A catalog."),
            "Gênero inválido para esta forma." => new(422, "invalid_gender", "Choose an available gender for this species/form."),
            "Pokébola inválida para captura em Z-A." => new(422, "invalid_ball", "Choose an available Z-A capture ball."),
            "Apelido deve ter até 12 caracteres, sem controles." => new(422, "invalid_nickname", "Nickname: at most 12 characters, no control characters."),
            _ => new(422, "validation_failed", "A field is invalid for this Pokémon. Check field ranges, slots and compatible moves.")
        };
    }
    private async Task MonitorIdle(CancellationToken stopping)
    {
        try { while (!stopping.IsCancellationRequested) { await Task.Delay(TimeSpan.FromSeconds(15), stopping); if (DateTime.UtcNow - lastActivity > options.IdleTimeout) { lock (store.Gate) app!.Lifetime.StopApplication(); break; } } } catch (OperationCanceledException) { }
    }
    public async Task WriteRuntimeFileAsync(string path)
    {
        path = Path.GetFullPath(path);
        var security = new FileSecurity();
        security.SetAccessRuleProtection(true, false);
        security.SetOwner(WindowsIdentity.GetCurrent().User!);
        security.AddAccessRule(new FileSystemAccessRule(WindowsIdentity.GetCurrent().User!, FileSystemRights.FullControl, AccessControlType.Allow));
        string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            using (var stream = new FileInfo(temporary).Create(FileMode.CreateNew, FileSystemRights.FullControl, FileShare.None, 4096, FileOptions.None, security))
            { await JsonSerializer.SerializeAsync(stream, new { address = Address, token, processId = Environment.ProcessId }, Json); }
            File.Move(temporary, path, true);
            new FileInfo(path).SetAccessControl(security);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
        runtimeFile = path;
    }
    public Task WaitForShutdownAsync() => app!.WaitForShutdownAsync();
    public async ValueTask DisposeAsync()
    {
        if (app != null) { await app.StopAsync(); await app.DisposeAsync(); }
        sprites.Dispose();
        if (runtimeFile != null) { try { File.Delete(runtimeFile); } catch { } }
    }
    private sealed class UInt64StringConverter : JsonConverter<ulong>
    {
        public override ulong Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options) => reader.TokenType == JsonTokenType.String ? ulong.Parse(reader.GetString()!, CultureInfo.InvariantCulture) : throw new JsonException("64-bit values require decimal strings.");
        public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
    }
}
