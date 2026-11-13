using ZaTool.Engine;

namespace ZaTool.WebHost;

public sealed record HostOptions
{
    public int Port { get; init; }
    public string? InitialPath { get; init; }
    public string? RuntimeFile { get; init; }
    public string DataDirectory { get; init; } = AppContext.BaseDirectory;
    public TimeSpan IdleTimeout { get; init; } = TimeSpan.Zero;
}
public sealed record OpenRequest(string? Path = null, string? CandidateId = null);
public sealed record PreviewRequest(string ExpectedHash, bool Party, int Slot, PokemonPatch Patch);
public sealed record PreviewAddRequest(string ExpectedHash, AddOptions Options);
public sealed record AlphaPlanRequest(string ExpectedHash, bool Party, int Slot, PokemonPatch Patch, AlphaMode Mode);
public sealed record AlphaPlanAddRequest(string ExpectedHash, AddOptions Options, AlphaMode Mode);
public sealed record EditRequest(string ExpectedHash, string OperationId, TeamEdit[] TeamEdits, TeamEdit[] BoxEdits);
public sealed record AddRequest(string ExpectedHash, string OperationId, AddOptions Options);
public sealed record SuggestRequest(string ExpectedHash, TeamPokemon[] Team, string Language = "pt-BR");
public sealed record Preferences(string? LastSavePath = null, string Language = "pt-BR");
internal sealed class ApiFailure(int status, string code, string safeDetail) : Exception(safeDetail)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
}
