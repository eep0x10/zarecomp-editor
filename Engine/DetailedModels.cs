namespace ZaTool.Engine;

public enum ShinyStyle { NonShiny, Star, Square }
public enum AlphaMode { Compatible, Custom }

/// <summary>Null preserves the field. Arrays use HP/Atk/Def/Spe/SpA/SpD and four move slots.</summary>
public sealed record PokemonPatch
{
    public int? Species { get; init; }
    public int? Form { get; init; }
    public int? Level { get; init; }
    public string? Nickname { get; init; }
    public bool? IsNicknamed { get; init; }
    public int? Gender { get; init; }
    public ShinyStyle? Shiny { get; init; }
    public int? Nature { get; init; }
    public int? StatNature { get; init; }
    public int? Ball { get; init; }
    public int? HeldItem { get; init; }
    public int? Language { get; init; }
    public int? Friendship { get; init; }
    public int[]? IVs { get; init; }
    public int[]? EVs { get; init; }
    public bool[]? HyperTrained { get; init; }
    public int[]? Moves { get; init; }
    public int[]? PP { get; init; }
    public int[]? PPUps { get; init; }
    public int[]? RelearnMoves { get; init; }
    public int[]? PlusMoves { get; init; }
    public int? MetLevel { get; init; }
    public int? MetLocation { get; init; }
    public DateOnly? MetDate { get; init; }
    public int? ObedienceLevel { get; init; }
    public string? OriginalTrainerName { get; init; }
    public int? OriginalTrainerGender { get; init; }
    public int? TID16 { get; init; }
    public int? SID16 { get; init; }
    public bool? IsAlpha { get; init; }
    /// <summary>Custom explicitly permits an Alpha encounter mismatch, never invalid size/IV data.</summary>
    public AlphaMode? AlphaMode { get; init; }
    public int? HeightScalar { get; init; }
    public int? WeightScalar { get; init; }
    public int? Scale { get; init; }
    public int? AffixedRibbon { get; init; }
    public bool? Favorite { get; init; }
    public int[]? Markings { get; init; }
}

public sealed record AlphaChange(string Field, string Before, string After);
public sealed record AlphaPlan(bool Available, string Code, string Hash, AlphaMode Mode, int? MinimumLevel,
    bool RequiresOriginChange, PokemonPatch? Patch, AddOptions? Options, PokemonDetails? Preview,
    IReadOnlyList<AlphaChange> Changes, IReadOnlyList<string> Warnings);

public sealed record PokemonReadOnly(uint PID, uint EncryptionConstant, ulong Tracker, int OriginGame, bool IsEgg,
    int EggLocation, DateOnly? EggDate, int CurrentHandler, string HandlingTrainerName, int Ability, int AbilitySlot,
    int[] Ribbons, int DisplayTID, int DisplaySID, IReadOnlyList<string> Unsupported);
public sealed record PokemonDetails(int Slot, bool Party, string Hash, PokemonPatch Values, PokemonReadOnly ReadOnly,
    bool Legal, string LegalityReport, IReadOnlyList<string> Warnings)
{
    public int[] Stats { get; init; } = [];
}
