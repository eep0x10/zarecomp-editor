using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using PKHeX.Core;

namespace ZaTool.Engine;

public sealed class SaveService
{
    private static readonly object WriteGate = new();
    private static readonly HashSet<uint> MutableBlocks = [0x0D66012C, 0x3AA1A9AD, 0x2D87BE5C];
    public IReadOnlyList<SaveCandidate> DiscoverSaves()
    {
        var roots = new[] { AppContext.BaseDirectory, Directory.GetParent(AppContext.BaseDirectory)?.FullName,
            Directory.GetParent(AppContext.BaseDirectory)?.Parent?.FullName, @"F:\Gaming\Standalone",
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads") };
        var found = new List<SaveCandidate>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in roots.Where(r => r != null && Directory.Exists(r)).Distinct())
        {
            var queue = new Queue<(string Path, int Depth)>(); queue.Enqueue((root!, 0));
            int budget = 2000;
            while (queue.Count != 0 && budget-- > 0)
            {
                var (folder, depth) = queue.Dequeue();
                if (!visited.Add(folder)) continue;
                try
                {
                    if ((File.GetAttributes(folder) & FileAttributes.ReparsePoint) != 0) continue;
                    var main = Path.Combine(folder, "main");
                    if (File.Exists(main) && Path.GetFileName(folder).StartsWith("save_", StringComparison.OrdinalIgnoreCase))
                        found.Add(new(main, $"{Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(folder)))} / {Path.GetFileName(folder)}", File.GetLastWriteTime(main)));
                    if (depth >= 5) continue;
                    foreach (var child in Directory.EnumerateDirectories(folder))
                    {
                        string name = Path.GetFileName(child);
                        if (name.Equals("PORT", StringComparison.OrdinalIgnoreCase) || name.StartsWith("save", StringComparison.OrdinalIgnoreCase)
                            || name.Contains("Legends", StringComparison.OrdinalIgnoreCase) || name.Contains("recomp", StringComparison.OrdinalIgnoreCase)
                            || name.Contains("ZA", StringComparison.OrdinalIgnoreCase) || name.Contains("Z-A", StringComparison.OrdinalIgnoreCase))
                            queue.Enqueue((child, depth + 1));
                    }
                }
                catch (Exception e) when (e is IOException or UnauthorizedAccessException) { /* Inaccessible installation: manual selection remains available. */ }
            }
        }
        return found.DistinctBy(s => s.Path, StringComparer.OrdinalIgnoreCase).OrderByDescending(s => s.LastModified).ToArray();
    }

    public SaveSummary Inspect(string path)
    {
        path = Path.GetFullPath(path);
        var bytes = File.ReadAllBytes(path);
        var save = Read(bytes);
        int occupied = 0;
        for (int i = 0; i < AccessibleBoxes(save) * 30; i++) if (save.GetBoxSlotAtIndex(i).Species != 0) occupied++;
        return new(path, save.OT, save.PartyCount, AccessibleBoxes(save), AccessibleBoxes(save) * 30 - occupied,
            Enumerable.Range(0, save.PartyCount).Select(i => Summary(save.GetPartySlotAtIndex(i), i)).ToArray(), Hash(bytes));
    }

    public AddResult Add(string path, AddOptions options, string? expectedHash = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        path = Path.GetFullPath(path);
        using var operationLock = new PathLock(path);
        lock (WriteGate)
        {
            GuardGame(path);
            path = Path.GetFullPath(path);
            var original = File.ReadAllBytes(path);
            var hash = Hash(original);
            if (expectedHash != null && !hash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase)) throw new IOException("O save mudou desde a leitura. Reabra antes de editar.");
            var save = Read(original);
            Validate(save, options);
            bool party = options.Destination == Destination.Party || options.Destination == Destination.Auto && save.PartyCount < 6;
            int index = party ? save.PartyCount : FindBoxSlot(save, options.PreferredBox);
            if (party && index >= 6) throw new InvalidOperationException("A equipe já tem seis Pokémon. Escolha Caixa ou Automático.");
            if (!party && index < 0) throw new InvalidOperationException("As caixas estão cheias. Nenhum Pokémon foi sobrescrito.");
            var before = save.Clone() as SAV9ZA ?? throw new InvalidDataException();
            var pokemon = CreatePokemon(save, options);
            if (party) save.SetPartySlotAtIndex(pokemon, index);
            else save.SetBoxSlotAtIndex(pokemon, index);
            VerifyChanges(before, save, party, index, pokemon);
            var output = save.Write().ToArray();
            var reopened = Read(output); // Read clones because SwishCrypto.Decrypt mutates its input.
            VerifyChanges(before, reopened, party, index, pokemon);
            var legality = new LegalityAnalysis(pokemon);
            string report = legality.Report();
            string backup = Commit(path, hash, output, candidate => VerifyChanges(before, candidate, party, index, pokemon));
            return new(path, backup, party ? $"Equipe — posição {index + 1}" : $"Caixa {index / 30 + 1} — posição {index % 30 + 1}",
                Summary(pokemon, index), legality.Valid, report, Hash(output));
        }
    }

    public IReadOnlyList<TeamPokemon> InspectTeam(string path) => Team(Read(File.ReadAllBytes(path)));

    public IReadOnlyList<TeamPokemon> InspectBox(string path, int box)
    {
        var save = Read(File.ReadAllBytes(path));
        if (box < 0 || box >= AccessibleBoxes(save)) throw new ArgumentException("Caixa inválida ou bloqueada.");
        return Enumerable.Range(box * 30, 30).Select(i => (Index: i, PK: save.GetBoxSlotAtIndex(i)))
            .Where(p => p.PK.Species != 0).Select(p => Describe(p.PK, p.Index)).ToArray();
    }
    public TeamEditResult EditTeam(string path, IReadOnlyList<TeamEdit> edits, string? expectedHash = null) => EditStorage(path, edits, [], expectedHash);
    public TeamEditResult EditBox(string path, IReadOnlyList<TeamEdit> edits, string? expectedHash = null) => EditStorage(path, [], edits, expectedHash, true);
    public TeamEditResult SaveEdits(string path, IReadOnlyList<TeamEdit> teamEdits, IReadOnlyList<TeamEdit> boxEdits, string? expectedHash = null) => EditStorage(path, teamEdits, boxEdits, expectedHash);
    private static TeamEditResult EditStorage(string path, IReadOnlyList<TeamEdit> teamEdits, IReadOnlyList<TeamEdit> boxEdits, string? expectedHash, bool returnBox = false)
    {
        ArgumentNullException.ThrowIfNull(teamEdits); ArgumentNullException.ThrowIfNull(boxEdits);
        path = Path.GetFullPath(path);
        using var operationLock = new PathLock(path);
        lock (WriteGate)
        {
            GuardGame(path);
            var bytes = File.ReadAllBytes(path); string hash = Hash(bytes);
            if (expectedHash != null && !hash.Equals(expectedHash, StringComparison.OrdinalIgnoreCase)) throw new IOException("O save mudou desde a leitura. Reabra antes de editar.");
            var save = Read(bytes); var before = (SAV9ZA)save.Clone();
            var edits = teamEdits.Select(e => (Edit: e, Party: true)).Concat(boxEdits.Select(e => (Edit: e, Party: false))).ToArray();
            if (edits.Length == 0 || edits.Select(e => (e.Party, e.Edit.Slot)).Distinct().Count() != edits.Length) throw new ArgumentException("Selecione posições distintas para editar.");
            foreach (var item in edits)
            {
                var edit = item.Edit;
                if (edit.Slot < 0 || edit.Slot >= (item.Party ? save.PartyCount : AccessibleBoxes(save) * 30)) throw new ArgumentException("Posição inválida ou bloqueada.");
                if (edit.Level is < 1 or > 100) throw new ArgumentException("Nível deve estar entre 1 e 100.");
                if (edit.Nature is < 0 or > 24) throw new ArgumentException("Natureza inválida.");
                if (edit.Level == null && edit.Nature == null && edit.Shiny == null) throw new ArgumentException("Informe uma alteração para cada posição selecionada.");
            }
            var expected = new Dictionary<(bool Party, int Index), PA9>(); var warnings = new List<string>();
            foreach (var item in edits)
            {
                var edit = item.Edit;
                var pk = (PA9)(item.Party ? save.GetPartySlotAtIndex(edit.Slot) : save.GetBoxSlotAtIndex(edit.Slot));
                if (pk.Species == 0) throw new ArgumentException("A posição selecionada está vazia.");
                if (!pk.ChecksumValid) throw new InvalidDataException("Pokémon tem checksum inválido.");
                if (edit.Level is int level) pk.CurrentLevel = (byte)level;
                if (edit.Nature is int nature) pk.Nature = pk.StatAlignment = (Nature)nature;
                if (edit.Shiny is true) SetStarShiny(pk);
                else if (edit.Shiny is false && pk.IsShiny) pk.PID ^= 0x10000000;
                if (edit.Level != null || edit.Nature != null)
                {
                    int hp = pk.Stat_HPCurrent, status = pk.Status_Condition;
                    pk.ResetPartyStats();
                    pk.Stat_HPCurrent = Math.Min(hp, pk.Stat_HPMax); pk.Status_Condition = status;
                }
                pk.RefreshChecksum(); expected[(item.Party, edit.Slot)] = pk;
                if (item.Party) save.SetPartySlotAtIndex(pk, edit.Slot, EntityImportSettings.None);
                else save.SetBoxSlotAtIndex(pk, edit.Slot, EntityImportSettings.None);
                if (edit.Shiny != null) save.Zukan.SetDex(pk);
                string label = item.Party ? $"Equipe {edit.Slot + 1}" : $"Caixa {edit.Slot / 30 + 1}, posição {edit.Slot % 30 + 1}";
                if (pk.CurrentLevel < pk.MetLevel) warnings.Add($"{label}: nível abaixo do nível original de encontro; legalidade não garantida.");
                if (!new LegalityAnalysis(pk).Valid) warnings.Add($"{label}: PKHeX sinaliza legalidade; edição não garante uso online.");
            }
            VerifyTeam(before, save, expected);
            var output = save.Write().ToArray(); VerifyTeam(before, Read(output), expected);
            string backup = Commit(path, hash, output, candidate => VerifyTeam(before, candidate, expected));
            return new(path, backup, Hash(output), returnBox ? expected.Where(p => !p.Key.Party).Select(p => Describe(p.Value, p.Key.Index)).ToArray() : Team(save), warnings);
        }
    }
    private static IReadOnlyList<TeamPokemon> Team(SAV9ZA save) => Enumerable.Range(0, save.PartyCount).Select(i => Describe(save.GetPartySlotAtIndex(i), i)).ToArray();
    private static TeamPokemon Describe(PKM pk, int index)
    {
        var pi = PersonalTable.ZA[pk.Species, pk.Form];
        int[] moves = [pk.Move1, pk.Move2, pk.Move3, pk.Move4];
        return new TeamPokemon(index, pk.Species, pk.Form, GameInfo.GetStrings("en").specieslist[pk.Species], pk.CurrentLevel, pk.IsShiny,
            (int)pk.StatAlignment, moves, [pi.HP, pi.ATK, pi.DEF, pi.SPE, pi.SPA, pi.SPD], moves.Select(NatureAdvisor.GetMoveCategory).ToArray());
    }
    private static void SetStarShiny(PKM pk)
    {
        uint low = pk.PID & 0xFFFF;
        pk.PID = low | ((low ^ pk.TID16 ^ pk.SID16 ^ 1u) << 16);
    }
    private static void VerifyTeam(SAV9ZA before, SAV9ZA after, IReadOnlyDictionary<(bool Party, int Index), PA9> expected)
    {
        if (before.PartyCount != after.PartyCount || before.AllBlocks.Count != after.AllBlocks.Count) throw new InvalidDataException("Estrutura da equipe alterada.");
        for (int i = 0; i < before.AllBlocks.Count; i++)
        {
            var b = before.AllBlocks[i]; var a = after.AllBlocks[i];
            if (a.Key != b.Key || a.Type != b.Type || a.SubType != b.SubType || (!MutableBlocks.Contains(b.Key) && !a.Data.SequenceEqual(b.Data)))
                throw new InvalidDataException($"Bloco alheio alterado: {b.Key:X8}.");
        }
        for (int i = 0; i < 6; i++)
        {
            var result = after.GetPartySlotAtIndex(i);
            if (expected.TryGetValue((true, i), out var edited)) { EqualPokemon(edited, result); if (!result.ChecksumValid) throw new InvalidDataException("Checksum da edição inválido."); }
            else EqualPokemon(before.GetPartySlotAtIndex(i), result);
        }
        for (int i = 0; i < before.BoxCount * 30; i++)
        {
            var result = after.GetBoxSlotAtIndex(i);
            if (expected.TryGetValue((false, i), out var edited)) { EqualPokemon(edited, result); if (!result.ChecksumValid) throw new InvalidDataException("Checksum da edição inválido."); }
            else EqualPokemon(before.GetBoxSlotAtIndex(i), result);
        }
    }
    private static string Commit(string path, string hash, byte[] output, Action<SAV9ZA> verify)
    {
        string backup = Backup(path, hash);
        string temp = Path.Combine(Path.GetDirectoryName(path)!, $".za-tool-{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None, 65536, FileOptions.WriteThrough))
            { stream.Write(output); stream.Flush(true); }
            var written = File.ReadAllBytes(temp);
            if (!Hash(written).Equals(Hash(output))) throw new IOException("Falha ao verificar arquivo temporário.");
            verify(Read(written)); GuardGame(path);
            if (!Hash(File.ReadAllBytes(path)).Equals(hash)) throw new IOException("O save mudou durante a edição. Operação cancelada.");
            File.Replace(temp, path, null);
            if (!Hash(File.ReadAllBytes(path)).Equals(Hash(output))) throw new IOException($"Verificação final falhou. Backup em {backup}");
            return backup;
        }
        finally { if (File.Exists(temp)) File.Delete(temp); }
    }

    private static SAV9ZA Read(byte[] bytes)
    {
        if (bytes.Length < 32 || !SwishCrypto.GetIsHashValid(bytes)) throw new InvalidDataException("Save corrompido ou checksum inválido.");
        if (SaveUtil.GetSaveFile(bytes.ToArray()) is not SAV9ZA save) throw new InvalidDataException("Selecione um save main de Pokémon Legends: Z-A.");
        if (save.PartyCount is < 0 or > 6) throw new InvalidDataException("Contagem da equipe inválida.");
        return save;
    }
    private static void GuardGame(string path)
    {
        foreach (var process in Process.GetProcesses())
        {
            using (process)
            {
                string name;
                try { name = process.ProcessName; } catch { continue; }
                if (name.Contains("lasrecomp", StringComparison.OrdinalIgnoreCase) || name.Contains("zarecomp", StringComparison.OrdinalIgnoreCase))
                    {
                    string? executable;
                    try { executable = process.MainModule?.FileName; } catch { throw new InvalidOperationException("Não foi possível confirmar a pasta do ZA Recomp aberto. Feche o jogo antes de editar."); }
                    if (executable == null || Path.GetFullPath(path).StartsWith(Path.GetDirectoryName(executable)! + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Feche o ZA Recomp antes de editar o save desta instalação.");
                }
            }
        }
    }
    private static void Validate(SAV9ZA save, AddOptions o)
    {
        if (o.Species > save.MaxSpeciesID || !Catalog.Forms(o.Species).Any(f => f.Id == o.Form)) throw new ArgumentException("Espécie ou forma não disponível em Z-A.");
        if (o.Level is < 1 or > 100) throw new ArgumentException("Nível deve estar entre 1 e 100.");
        if (o.Nature is < 0 or > 24) throw new ArgumentException("Natureza inválida.");
        if (o.Gender != -1 && !Catalog.Genders(o.Species, o.Form).Any(g => g.Id == o.Gender)) throw new ArgumentException("Gênero inválido para esta forma.");
        if (!Catalog.Balls.Any(b => b.Id == o.Ball)) throw new ArgumentException("Pokébola inválida para captura em Z-A.");
        if (o.Nickname == null || o.Nickname.Length > 12 || o.Nickname.Any(char.IsControl)) throw new ArgumentException("Apelido deve ter até 12 caracteres, sem controles.");
        if (!Enum.IsDefined(o.Destination)) throw new ArgumentException("Destino inválido.");
        if (o.PreferredBox < -1 || o.PreferredBox >= AccessibleBoxes(save)) throw new ArgumentException("Caixa inválida.");
        CheckStats(o.IVs, 31, "IVs"); CheckStats(o.EVs, 252, "EVs");
        if (o.EVs?.Sum() > 510) throw new ArgumentException("EVs não podem somar mais de 510.");
        if (o.Moves != null)
        {
            if (o.Moves.Length != 4 || o.Moves.All(m => m == 0) || o.Moves.Any(m => m < 0 || m > save.MaxMoveID) || o.Moves.Where(m => m != 0).Distinct().Count() != o.Moves.Count(m => m != 0))
                throw new ArgumentException("Informe quatro golpes, sem repetidos, com ao menos um válido.");
            var allowed = new bool[save.MaxMoveID + 1];
            var pk = new PA9 { Species = (ushort)o.Species, Form = (byte)o.Form };
            LearnSource9ZA.Instance.GetAllMoves(allowed, pk, new EvoCriteria { Species = pk.Species, Form = pk.Form, LevelMax = (byte)o.Level });
            if (o.Moves.Any(m => m != 0 && !allowed[m])) throw new ArgumentException("Um golpe não pode ser aprendido por esta forma no nível escolhido (nível/TM).");
        }
    }
    private static void CheckStats(int[]? stats, int max, string label)
    { if (stats != null && (stats.Length != 6 || stats.Any(s => s < 0 || s > max))) throw new ArgumentException($"{label}: informe seis valores entre 0 e {max}."); }

    private static PA9 CreatePokemon(SAV9ZA save, AddOptions o)
    {
        var species = (ushort)o.Species; var form = (byte)o.Form;
        var blank = new PA9 { Species = species, Form = form };
        var chain = new[] { new EvoCriteria { Species = species, Form = form, LevelMin = 1, LevelMax = (byte)o.Level } };
        var encounter = EncounterGenerator9a.Instance.GetPossible(blank, chain, GameVersion.ZA, EncounterTypeGroup.Static | EncounterTypeGroup.Slot)
            .FirstOrDefault(e => e.Species == species && e.Form == form && e.LevelMin <= o.Level && e is IEncounterConvertible);
        var criteria = new EncounterCriteria { Nature = (Nature)o.Nature, Gender = o.Gender < 0 ? Gender.Random : (Gender)o.Gender, Shiny = o.Shiny ? Shiny.Always : Shiny.Never };
        var pk = encounter is IEncounterConvertible converter ? (PA9)converter.ConvertToPKM(save, criteria) : new PA9
        {
            Species = species, Form = form, Version = GameVersion.ZA, Language = save.Language,
            OriginalTrainerName = save.OT, OriginalTrainerGender = save.Gender, ID32 = save.ID32,
            MetLevel = (byte)o.Level, MetDate = DateOnly.FromDateTime(DateTime.Today),
            EncryptionConstant = (uint)Random.Shared.NextInt64(0, 1L << 32), PID = (uint)Random.Shared.NextInt64(0, 1L << 32),
        };
        var info = PersonalTable.ZA[species, form];
        pk.Nature = pk.StatAlignment = (Nature)o.Nature;
        pk.Gender = (byte)(o.Gender < 0 ? Catalog.Genders(o.Species, o.Form)[0].Id : o.Gender);
        pk.CurrentLevel = (byte)o.Level; pk.ObedienceLevel = pk.MetLevel;
        pk.Ball = (byte)o.Ball;
        pk.Nickname = string.IsNullOrWhiteSpace(o.Nickname) ? SpeciesName.GetSpeciesNameGeneration(species, pk.Language, 9) : o.Nickname;
        pk.IsNicknamed = !string.IsNullOrWhiteSpace(o.Nickname);
        pk.OriginalTrainerFriendship = info.BaseFriendship;
        pk.RefreshAbility(0);
        if (encounter == null) pk.SetRandomIVs();
        if (o.IVs != null) pk.SetIVs(o.IVs);
        if (o.EVs != null) pk.SetEVs(o.EVs);
        if (o.Shiny) SetStarShiny(pk);
        if (!o.Shiny && pk.IsShiny) pk.PID ^= 0x10000000;
        Span<ushort> moves = stackalloc ushort[4];
        if (o.Moves == null) LearnSource9ZA.Instance.GetLearnset(species, form).SetEncounterMovesBackwards((byte)o.Level, moves, sameDescend: false);
        else for (int i = 0; i < 4; i++) moves[i] = (ushort)o.Moves[i];
        pk.SetMoves(moves);
        var (_, plus) = LearnSource9ZA.GetLearnsetAndPlus(species, form);
        pk.SetPlusFlagsEncounter(info, plus, (byte)o.Level);
        if (form != 0 && species is 479 or 720) // Rotom/Hoopa retain the base form's naturally available Plus moves.
            pk.SetPlusFlagsEncounter(info, LearnSource9ZA.GetLearnsetAndPlus(species, 0).Plus, (byte)o.Level);
        pk.HealPP(); pk.ResetPartyStats(); pk.RefreshChecksum();
        return pk;
    }
    private static int AccessibleBoxes(SAV9ZA save) => Math.Clamp(save.BoxesUnlocked, 0, save.BoxCount);
    private sealed class PathLock : IDisposable
    {
        private readonly Mutex mutex;
        public PathLock(string path)
        {
            string key = Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(path.ToUpperInvariant())));
            mutex = new Mutex(false, @"Local\ZA-Tool-" + key);
            try
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(15))) { mutex.Dispose(); throw new IOException("Outro editor está usando este save. Aguarde e reabra."); }
            }
            catch (AbandonedMutexException) { /* Owning process exited; hash validation still runs. */ }
        }
        public void Dispose() { mutex.ReleaseMutex(); mutex.Dispose(); }
    }
    private static int FindBoxSlot(SAV9ZA save, int preferred)
    {
        var boxes = Enumerable.Range(0, AccessibleBoxes(save));
        if (preferred >= 0) boxes = new[] { preferred }.Concat(boxes.Where(b => b != preferred));
        foreach (var box in boxes) for (int slot = 0; slot < 30; slot++)
            if (save.GetBoxSlotAtIndex(box, slot).Species == 0) return box * 30 + slot;
        return -1;
    }
    private static void VerifyChanges(SAV9ZA before, SAV9ZA after, bool party, int index, PA9 added)
    {
        if (after.PartyCount != before.PartyCount + (party ? 1 : 0)) throw new InvalidDataException("Falha na contagem da equipe.");
        if (before.AllBlocks.Count != after.AllBlocks.Count) throw new InvalidDataException("Estrutura do save alterada.");
        for (int i = 0; i < before.AllBlocks.Count; i++)
        {
            var b = before.AllBlocks[i]; var a = after.AllBlocks[i];
            if (b.Key != a.Key || b.Type != a.Type || b.SubType != a.SubType || (!MutableBlocks.Contains(b.Key) && !b.Data.SequenceEqual(a.Data)))
                throw new InvalidDataException($"Bloco alheio alterado: {b.Key:X8}.");
        }
        for (int i = 0; i < 6; i++) if (!party || i != index) EqualPokemon(before.GetPartySlotAtIndex(i), after.GetPartySlotAtIndex(i));
        for (int i = 0; i < before.BoxCount * 30; i++) if (party || i != index) EqualPokemon(before.GetBoxSlotAtIndex(i), after.GetBoxSlotAtIndex(i));
        var result = party ? after.GetPartySlotAtIndex(index) : after.GetBoxSlotAtIndex(index);
        if (result.Species != added.Species || result.Form != added.Form || result.CurrentLevel != added.CurrentLevel || result.IsShiny != added.IsShiny || !result.ChecksumValid)
            throw new InvalidDataException("Pokémon gravado não confere.");
        EqualPokemon(added, result);
    }
    private static void EqualPokemon(PKM a, PKM b)
    { if (!a.Data.SequenceEqual(b.Data)) throw new InvalidDataException("Um slot existente foi alterado."); }
    private static PokemonSummary Summary(PKM pk, int index) => new(index, pk.Species, GameInfo.GetStrings("en").specieslist[pk.Species], pk.CurrentLevel, pk.IsShiny);
    private static string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
    private static string Backup(string path, string expectedHash)
    {
        string source = Path.GetDirectoryName(path)!;
        string parent = Directory.GetParent(source)?.FullName ?? throw new IOException("Pasta do save sem diretório pai.");
        string backup = Path.Combine(parent, "ZA-Tool-Backups", $"{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-{Guid.NewGuid():N}");
        var manifest = new Dictionary<string, string>();
        // Enumerate before creating the destination; reject links to avoid following external data.
        var files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
        foreach (var entry in Directory.EnumerateFileSystemEntries(source, "*", SearchOption.AllDirectories))
            if ((File.GetAttributes(entry) & FileAttributes.ReparsePoint) != 0) throw new IOException("Backup não aceita links/junções na pasta do save.");
        Directory.CreateDirectory(backup);
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories)) Directory.CreateDirectory(Path.Combine(backup, Path.GetRelativePath(source, directory)));
        foreach (var file in files)
        {
            string relative = Path.GetRelativePath(source, file), target = Path.Combine(backup, relative);
            File.Copy(file, target, false);
            string originalHash = Hash(File.ReadAllBytes(file));
            if (!originalHash.Equals(Hash(File.ReadAllBytes(target)))) throw new IOException("Backup não conferiu: " + relative);
            manifest[relative] = originalHash;
        }
        if (!manifest.TryGetValue(Path.GetFileName(path), out var mainHash) || !mainHash.Equals(expectedHash)) throw new IOException("Save mudou durante o backup.");
        string manifestPath = Path.Combine(backup, "manifest.json");
        if (File.Exists(manifestPath)) manifestPath = Path.Combine(backup, $"manifest-{Guid.NewGuid():N}.json");
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
        return backup;
    }
}


