namespace ZaTool.WebHost;

internal static class ApiDocs
{
    private static readonly (string Method, string Path, string Description)[] Routes =
    [
        ("get", "/api/v1/bootstrap", "Version, preferences and initial path"),
        ("post", "/api/v1/discovery", "Discover saves without writes"),
        ("post", "/api/v1/sessions", "Open {path} or {candidateId}"),
        ("get", "/api/v1/sessions/{id}", "Read coherent save snapshot"),
        ("delete", "/api/v1/sessions/{id}", "Release session without deleting save"),
        ("get", "/api/v1/sessions/{id}/boxes/{box}", "Read occupied slots and hash"),
        ("get", "/api/v1/sessions/{id}/pokemon/{storage}/{slot}", "Read details; storage party or box, zero-based global slot"),
        ("post", "/api/v1/sessions/{id}/preview", "Preview {expectedHash,party,slot,patch}; never writes"),
        ("post", "/api/v1/sessions/{id}/preview-add", "Preview {expectedHash,options}; never writes"),
        ("post", "/api/v1/sessions/{id}/alpha-plan", "Read-only Alpha plan {expectedHash,party,slot,patch,mode}; review changes, then explicitly apply patch to draft before Save"),
        ("post", "/api/v1/sessions/{id}/alpha-plan-add", "Read-only creation plan {expectedHash,options,mode}; retain returned options.creationSeed and apply options explicitly"),
        ("post", "/api/v1/sessions/{id}/suggestions", "Nature suggestions {expectedHash,team,language}"),
        ("post", "/api/v1/sessions/{id}/edits", "Commit {expectedHash,operationId,teamEdits,boxEdits} with backup"),
        ("post", "/api/v1/sessions/{id}/add", "Commit {expectedHash,operationId,options} with backup"),
        ("get", "/api/v1/catalogs", "Catalog choice arrays"),
        ("get", "/api/v1/catalogs/species/{species}/forms", "Available persistent forms as Choice[] {id,name}; select the first returned ID, which need not be zero. Empty array for unavailable species."),
        ("get", "/api/v1/catalogs/species/{species}/forms/{form}", "Forms, genders, stats and Plus moves"),
        ("get", "/api/v1/sprites/{species}/{form}/{variant}", "Exact-form PNG artwork; variant normal or shiny. 404 sprite_unavailable when absent/offline; no generic image substitution."),
        ("put", "/api/v1/preferences", "Save {lastSavePath,language}"),
        ("post", "/api/v1/heartbeat", "Keep launcher active"),
        ("post", "/api/v1/shutdown", "Drain writes and exit")
    ];
    internal static object Document() => new
    {
        openapi = "3.1.0", info = new { title = "Recomp Pokémon local API", version = "2.1.0" },
        security = new[] { new Dictionary<string, string[]> { ["bearerAuth"] = [] } },
        components = new { securitySchemes = new { bearerAuth = new { type = "http", scheme = "bearer" } }, schemas = Schemas() },
        paths = Routes.GroupBy(r => r.Path).ToDictionary(g => g.Key, g => g.ToDictionary(r => r.Method, r => (object)new
        {
            summary = r.Description,
            parameters = r.Path.Split('/').Where(p => p.StartsWith('{')).Select(p => new { name = p.Trim('{','}'), @in = "path", required = true, schema = new { type = "string" } }).ToArray(),
            responses = new Dictionary<string, object> { ["200"] = new { description = "Success. Mutation receipt contains committed/hash/backupDirectory; previews never persist." }, ["400"] = new { description = "Invalid request" }, ["401"] = new { description = "Runtime token required" }, ["403"] = new { description = "Origin or host rejected" }, ["409"] = new { description = "Stale hash or operation conflict" }, ["422"] = new { description = "Domain validation failed" } },
            requestBody = r.Method is "post" or "put" ? new { required = true, content = new Dictionary<string, object> { ["application/json"] = new { schema = RequestSchema(r.Path) } } } : null
        }))
    };
    private static object RequestSchema(string path) => path.EndsWith("/alpha-plan") ? Ref("AlphaPlanRequest") : path.EndsWith("/alpha-plan-add") ? Ref("AlphaPlanAddRequest") : path.EndsWith("/preview") ? Ref("PreviewRequest") : path.EndsWith("/preview-add") ? Ref("PreviewAddRequest") : path.EndsWith("/edits") ? Ref("EditRequest") : path.EndsWith("/add") ? Ref("AddRequest") : path.EndsWith("/sessions") ? Ref("OpenRequest") : path.EndsWith("/suggestions") ? Ref("SuggestRequest") : path.EndsWith("/preferences") ? Ref("Preferences") : new { type = "object" };
    private static Dictionary<string, object> Ref(string type) => new() { ["$ref"] = "#/components/schemas/" + type };
    private static Dictionary<string, object> Schemas()
    {
        Type[] types = [typeof(OpenRequest), typeof(PreviewRequest), typeof(PreviewAddRequest), typeof(AlphaPlanRequest),typeof(AlphaPlanAddRequest),typeof(EditRequest), typeof(AddRequest), typeof(SuggestRequest), typeof(Preferences), typeof(ZaTool.Engine.PokemonPatch), typeof(ZaTool.Engine.AddOptions), typeof(ZaTool.Engine.TeamEdit), typeof(ZaTool.Engine.TeamPokemon)];
        object Schema(Type t)
        {
            var underlying = Nullable.GetUnderlyingType(t); if (underlying != null) return new { anyOf = new[] { Schema(underlying), new { type = "null" } } };
            if (t == typeof(string)) return new { type = "string" };
            if (t == typeof(ulong)) return new {type="string",pattern="^[0-9]+$",description="Unsigned 64-bit decimal string; never convert to a JavaScript Number."};
            if (t == typeof(DateOnly)) return new { type = "string", format = "date" };
            if (t == typeof(bool)) return new { type = "boolean" };
            if (t.IsEnum) return new Dictionary<string, object> { ["type"] = "string", ["enum"] = Enum.GetNames(t) };
            if (t.IsArray) return new { type = "array", items = Schema(t.GetElementType()!) };
            if (types.Contains(t)) return Ref(t.Name);
            return new { type = "integer" };
        }
        return types.ToDictionary(t => t.Name, t => (object)new { type = "object", additionalProperties = false, properties = t.GetProperties().ToDictionary(p => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(p.Name), p => Schema(p.PropertyType)) });
    }
    internal const string Html = """
    <!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Local API reference</title><link rel="icon" href="data:,"><body><h1>Recomp Pokémon API 2.0</h1><p>Local interactive OpenAPI reference. Writes require explicit submission and create backups. Never share your runtime token.</p><label>Session token <input id="token" type="password" autocomplete="off"></label><button id="authorize">Authorize</button><label>Operation <select id="operation"></select></label><label>Path <input id="path" size="80"></label><label>JSON body <textarea id="body" rows="12" cols="90">{}</textarea></label><button id="execute">Execute request</button><pre id="output"></pre><script src="/docs.js"></script></body></html>
    """;
    internal const string Script = """
    const $=id=>document.getElementById(id);let token=new URLSearchParams(location.hash.slice(1)).get('token')||'';history.replaceState(null,'',location.pathname);$('token').value=token;let operations=[];fetch('/openapi/v1.json').then(r=>r.json()).then(d=>{for(const [path,items] of Object.entries(d.paths))for(const [method,op]of Object.entries(items)){operations.push({path,method});const option=document.createElement('option');option.textContent=method.toUpperCase()+' '+path+' — '+op.summary;$('operation').append(option)}$('path').value=operations[0].path});$('authorize').onclick=()=>{token=$('token').value;$('token').value='';$('output').textContent='Token kept in memory for this page only.'};$('operation').onchange=()=>{$('path').value=operations[$('operation').selectedIndex].path};$('execute').onclick=async()=>{const op=operations[$('operation').selectedIndex],path=$('path').value;if(!path.startsWith('/api/v1/')||path.includes('://')){$('output').textContent='Only local API paths are allowed.';return}try{const r=await fetch(path,{method:op.method.toUpperCase(),headers:{Authorization:'Bearer '+token,'Content-Type':'application/json'},body:op.method==='get'||op.method==='delete'?undefined:$('body').value});$('output').textContent=r.status+'\n'+await r.text()}catch(e){$('output').textContent=String(e)}};
    """;
}
