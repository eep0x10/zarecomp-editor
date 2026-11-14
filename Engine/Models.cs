namespace ZaTool.Engine;

public sealed record Choice(int Id, string Name) { public override string ToString()=>Name; }
public sealed record SaveCandidate(string Path, string Label, DateTime LastModified);
public sealed record PokemonSummary(int Index, int Species, string Name, int Level, bool Shiny);
public sealed record SaveSummary(string Path, string Trainer, int PartyCount, int BoxCount, int EmptyBoxSlots, IReadOnlyList<PokemonSummary> Party, string Hash);
public enum Destination { Auto, Party, Box }
public sealed record AddOptions
{
    public ulong? CreationSeed { get; init; }
    public int Species { get; init; }=4;
    public int Form { get; init; }
    public int Level { get; init; }=10;
    public bool Shiny { get; init; }
    public int Nature { get; init; }=15;
    public int Gender { get; init; }=-1;
    public string Nickname { get; init; }="";
    public int Ball { get; init; }=4;
    public Destination Destination { get; init; }=Destination.Auto;
    public int PreferredBox { get; init; }=-1;
    public int[]? Moves { get; init; }
    public int[]? IVs { get; init; }
    public int[]? EVs { get; init; }
    public PokemonPatch? Details { get; init; }
}
public sealed record AddResult(string SavePath, string BackupDirectory, string Location, PokemonSummary Pokemon, bool Legal, string LegalityReport, string Hash);
public sealed record TeamPokemon(int Slot, int Species, int Form, string Name, int Level, bool Shiny, int Nature, int[] Moves, int[] BaseStats, int[] MoveCategories);
public sealed record TeamEdit(int Slot, int? Level=null, int? Nature=null, bool? Shiny=null)
{
    public PokemonPatch? Details { get; init; }
}
public sealed record TeamEditResult(string SavePath, string BackupDirectory, string Hash, IReadOnlyList<TeamPokemon> Team, IReadOnlyList<string> Warnings);
public sealed record NatureSuggestion(int Slot, int Nature, string Reason);
public sealed record TeamOptimizeOptions(bool BestNatures=false,bool Alpha=false,bool MaxIVs=false,bool Shiny=false,bool MaxSize=false,bool MaxFriendship=false,bool Level100=false);
public sealed record TeamOptimizeEdit(int Slot,IReadOnlyDictionary<string,object> Details);
public sealed record TeamOptimizeItem(int Slot,string Name,int Species,int Form,bool Available,string Code,
    PokemonDetails? Before,PokemonDetails? Preview,IReadOnlyDictionary<string,object>? Patch,
    IReadOnlyList<AlphaChange> Changes,IReadOnlyList<string> Warnings,int? Nature,string? NatureReason);
public sealed record TeamOptimizePlan(bool Available,string Code,string Hash,IReadOnlyList<TeamOptimizeEdit> TeamEdits,
    IReadOnlyList<TeamOptimizeItem> Items,IReadOnlyList<string> Warnings,IReadOnlyList<NatureSuggestion> Natures);
