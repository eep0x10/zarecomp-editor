using System.Security.Cryptography;
using System.Text.Json;
using ZaTool.Engine;

namespace ZaTool.WebHost;

internal sealed class WorkspaceStore
{
    internal const long MaxSaveSize = 32 * 1024 * 1024;
    internal readonly object Gate = new();
    internal readonly SaveService Engine = new();
    private readonly Dictionary<string, Session> sessions = new();
    private readonly Dictionary<string, string> candidates = new();
    private readonly Dictionary<string, (string Fingerprint, object Receipt)> receipts = new();
    private readonly string settingsPath;
    internal Preferences Preferences { get; private set; } = new();
    internal WorkspaceStore(string directory)
    {
        settingsPath = Path.Combine(directory, "RecompPokemon.settings.json");
        try { Preferences = JsonSerializer.Deserialize<Preferences>(File.ReadAllText(settingsPath), WorkbenchHost.Json) ?? new(); } catch { }
        if (Preferences.Language is not ("pt-BR" or "en" or "es")) Preferences = Preferences with { Language = "pt-BR" };
    }
    internal object Discover()
    {
        candidates.Clear();
        return Engine.DiscoverSaves().Select(c => { string id = Guid.NewGuid().ToString("N"); candidates[id] = c.Path; return new { id, c.Label, c.LastModified, c.Path }; }).ToArray();
    }
    internal object Open(OpenRequest request)
    {
        string? path = request.Path;
        if (request.CandidateId != null && !candidates.TryGetValue(request.CandidateId, out path)) throw new ApiFailure(404, "candidate_not_found", "Discover saves again.");
        if ((request.Path != null) == (request.CandidateId != null)) throw new ApiFailure(400, "selection_required", "Choose one save path or candidate.");
        path = ValidatePath(path);
        if (sessions.Count >= 32) throw new ApiFailure(409, "session_limit", "Close an existing save session first.");
        var session = new Session(Guid.NewGuid().ToString("N"), path);
        var snapshot = Snapshot(session);
        sessions.Add(session.Id, session);
        return snapshot;
    }
    internal Session Get(string id) => sessions.TryGetValue(id, out var session) ? session : throw new ApiFailure(404, "session_not_found", "Open a save first.");
    internal void Close(string id) { sessions.Remove(id); }
    internal object Snapshot(Session s)
    {
        return Coherent(s, null, () =>
        {
            var summary = Engine.Inspect(s.Path);
            var team = Engine.InspectTeam(s.Path);
            var box = summary.BoxCount > 0 ? Engine.InspectBox(s.Path, 0) : [];
            return new { id = s.Id, summary, team, box, boxIndex = 0, hash = summary.Hash };
        });
    }
    internal T Coherent<T>(Session s, string? expectedHash, Func<T> read)
    {
        ValidatePath(s.Path);
        string before = FileHash(s.Path);
        if (expectedHash != null && !string.Equals(expectedHash, before, StringComparison.OrdinalIgnoreCase)) throw Stale();
        T result = read();
        if (FileHash(s.Path) != before) throw Stale();
        return result;
    }
    internal object Commit(Session s, string expectedHash, string operationId, object request, Func<object> write)
    {
        if (string.IsNullOrWhiteSpace(expectedHash)) throw new ApiFailure(400, "hash_required", "Reload the save before writing.");
        if (!Guid.TryParse(operationId, out _)) throw new ApiFailure(400, "operation_id_required", "Use a unique UUID operationId.");
        string key = s.Id + ":" + operationId;
        string fingerprint = Convert.ToHexString(SHA256.HashData(JsonSerializer.SerializeToUtf8Bytes(request, WorkbenchHost.Json)));
        if (receipts.TryGetValue(key, out var previous))
        {
            if (previous.Fingerprint != fingerprint) throw new ApiFailure(409, "operation_mismatch", "This operation ID already belongs to another request.");
            return previous.Receipt;
        }
        // Do not evict receipts and allow an old delivery to write twice.
        if (receipts.Count >= 1024) throw new ApiFailure(409, "operation_limit", "Restart the workbench before more writes.");
        Coherent(s, expectedHash, () => true);
        object result;
        try { result = write(); }
        catch(SaveCommitException error)
        {
            string? currentHash=null;try{if(File.Exists(s.Path))currentHash=FileHash(s.Path);}catch(IOException){}catch(UnauthorizedAccessException){}
            var failed=new {committed=false,outcomeUnknown=error.MayHaveCommitted,refreshRequired=true,hash=currentHash,backupDirectory=error.BackupDirectory,phase=error.Phase};
            receipts[key]=(fingerprint,failed);return failed;
        }
        catch (IOException) when (File.Exists(s.Path) && !FileHash(s.Path).Equals(expectedHash,StringComparison.OrdinalIgnoreCase))
        {
            // A failure after replacement is ambiguous: remember it and forbid a retry.
            var uncertain = new { committed = false, outcomeUnknown = true, refreshRequired = true, hash = FileHash(s.Path), backupDirectory = (string?)null };
            receipts[key] = (fingerprint, uncertain);
            return uncertain;
        }
        string hash = result is AddResult add ? add.Hash : ((TeamEditResult)result).Hash;
        string backup = result is AddResult addition ? addition.BackupDirectory : ((TeamEditResult)result).BackupDirectory;
        var receipt = new { committed = true, outcomeUnknown = false, hash, backupDirectory = backup, result, refreshRequired = false };
        receipts[key] = (fingerprint, receipt);
        return receipt;
    }
    internal object SavePreferences(Preferences value)
    {
        if (value.Language is not ("pt-BR" or "en" or "es")) throw new ArgumentException("Unsupported language.");
        if (value.LastSavePath != null) ValidatePath(value.LastSavePath);
        string temporary = settingsPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try { File.WriteAllText(temporary, JsonSerializer.Serialize(value, WorkbenchHost.Json)); File.Move(temporary, settingsPath, true); Preferences = value; }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
        return Preferences;
    }
    internal static string ValidatePath(string? input)
    {
        if (string.IsNullOrWhiteSpace(input) || !Path.IsPathFullyQualified(input) || input.StartsWith(@"\\") || input.StartsWith("//")) throw new ApiFailure(422, "invalid_path", "Enter an absolute local path to main.");
        string path = Path.GetFullPath(input);
        if (!Path.GetFileName(path).Equals("main", StringComparison.OrdinalIgnoreCase)) throw new ApiFailure(422, "invalid_path", "Select a main save file.");
        for (string? part = path; part != null; part = Path.GetDirectoryName(part))
        {
            if ((File.GetAttributes(part) & FileAttributes.ReparsePoint) != 0) throw new ApiFailure(422, "linked_path", "Links and junctions are not supported for saves.");
        }
        long size = new FileInfo(path).Length;
        if (size < 32 || size > MaxSaveSize) throw new ApiFailure(422, "invalid_save_size", "Save size is outside the supported limit.");
        return path;
    }
    internal static string FileHash(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)));
    internal static ApiFailure Stale() => new(409, "stale_hash", "The save changed. Reload before editing.");
    internal sealed record Session(string Id, string Path);
}
