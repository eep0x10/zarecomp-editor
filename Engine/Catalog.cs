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

    public static IReadOnlyList<Choice> Languages { get; } = new[] {1,2,3,4,5,7,8,9,10,11}.Select(i => new Choice(i, ((LanguageID)i).ToString())).ToArray();
    public static IReadOnlyList<Choice> HeldItems { get; } = new SAV9ZA().HeldItems.ToArray().Prepend((ushort)0).Distinct().Select(i => new Choice(i, Strings.itemlist[i])).ToArray();
    public static IReadOnlyList<Choice> MetLocations { get; } = new MetDataSource(Strings).GetLocationList(GameVersion.ZA, EntityContext.Gen9a)
        .Where(c => c.Value != 0).Select(c => new Choice(c.Value, c.Text)).DistinctBy(c => c.Id).ToArray();
    public static IReadOnlyList<Choice> Ribbons { get; } = Enumerable.Range(0, Math.Min(128,(int)RibbonIndex.MAX_COUNT)).Select(i => new Choice(i, ((RibbonIndex)i).ToString())).ToArray();
    public static IReadOnlyList<Choice> PlusMoves(int species, int form)
    {
        if (!Forms(species).Any(f => f.Id == form))return [];
        var pk=new PA9 {Species=(ushort)species,Form=(byte)form};
        var allowed=new bool[Strings.movelist.Length];
        LearnSource9ZA.Instance.GetAllMoves(allowed,pk,new EvoCriteria {Species=pk.Species,Form=pk.Form,LevelMax=100});
        return PersonalTable.ZA[pk.Species,pk.Form].PlusMoveIndexes.ToArray().Where(m=>allowed[m]&&!MoveInfo.IsDummiedMove(pk,m)).Select(m=>new Choice(m,Strings.movelist[m])).ToArray();
    }

    public static int[] BaseStats(int species, int form)
    {
        if (!Forms(species).Any(f => f.Id == form)) throw new ArgumentException("Espécie/forma não disponível em Z-A.");
        var pi = PersonalTable.ZA[(ushort)species,(byte)form];
        return [pi.HP,pi.ATK,pi.DEF,pi.SPE,pi.SPA,pi.SPD];
    }

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

