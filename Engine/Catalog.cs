using PKHeX.Core;

namespace ZaTool.Engine;

public static class Catalog
{
    private static readonly GameStrings Strings = GameInfo.GetStrings("en");
    public static IReadOnlyList<Choice> Species { get; } = Enumerable.Range(1, PersonalTable.ZA.MaxSpeciesID)
        .Where(i => Forms(i).Count != 0).Select(i => new Choice(i, Strings.specieslist[i])).OrderBy(c => c.Name).ToArray();
    public static IReadOnlyList<Choice> Natures { get; } = Enumerable.Range(0, 25).Select(i => new Choice(i, Strings.natures[i])).ToArray();
    public static IReadOnlyList<Choice> Balls { get; } = Enumerable.Range(1, Strings.balllist.Length - 1)
        .Where(i => i <= 26 && i != 16)
        .Select(i => new Choice(i, Strings.balllist[i])).ToArray();
    public static IReadOnlyList<Choice> Moves { get; } = Enumerable.Range(0, Strings.movelist.Length)
        .Select(i => new Choice(i, i == 0 ? "(Empty)" : Strings.movelist[i])).ToArray();

    public static IReadOnlyList<Choice> Forms(int species)
    {
        if (species < 1 || species > PersonalTable.ZA.MaxSpeciesID) return [];
        var s = (ushort)species;
        var names = FormConverter.GetFormList(s, Strings.types, Strings.forms, EntityContext.Gen9a);
        return Enumerable.Range(0, Math.Max(1, (int)PersonalTable.ZA[s].FormCount))
            .Where(f => PersonalTable.ZA.IsPresentInGame(s, (byte)f) && !FormInfo.IsBattleOnlyForm(s, (byte)f, 9))
            .Select(f => new Choice(f, f < names.Length && !string.IsNullOrWhiteSpace(names[f]) ? names[f] : f == 0 ? "Default" : $"Form {f}")).ToArray();
    }

    public static IReadOnlyList<Choice> Genders(int species, int form)
    {
        if (!Forms(species).Any(f => f.Id == form)) return [];
        return PersonalTable.ZA[(ushort)species, (byte)form].Gender switch
        {
            255 => [new(2, "Sem gênero / Genderless")],
            254 => [new(1, "Fêmea / Female")],
            0 => [new(0, "Macho / Male")],
            _ => [new(0, "Macho / Male"), new(1, "Fêmea / Female")],
        };
    }
}

