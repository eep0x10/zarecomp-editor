using System.Reflection;
using ZaTool.Engine;

namespace ZaTool.App;

internal static class PokemonPatchTools
{
    internal static readonly PropertyInfo[] Fields = typeof(PokemonPatch).GetProperties().Where(x => x.CanRead && x.CanWrite).ToArray();
    internal static PokemonPatch Merge(PokemonPatch original, PokemonPatch? patch)
    {
        var copy = original with { };
        if (patch is not null) foreach (var field in Fields) if (field.GetValue(patch) is { } value) field.SetValue(copy, value is Array array ? array.Clone() : value);
        return copy;
    }
    internal static bool Empty(PokemonPatch? patch) => patch is null || Fields.All(x => x.GetValue(patch) is null);
    internal static bool Same(object? a, object? b) => a is Array aa && b is Array bb ? aa.Cast<object>().SequenceEqual(bb.Cast<object>()) : Equals(a, b);
    internal static PokemonPatch Initial(PokemonDetails details, TeamEdit? draft, TeamPokemon original)
    {
        var values = details.Values;
        if (draft is not null)
        {
            if (draft.Level is int level && level != original.Level) values = values with { Level = level };
            if (draft.Nature is int nature && nature != original.Nature) values = values with { Nature = nature, StatNature = nature };
            if (draft.Shiny is bool shiny && shiny != original.Shiny) values = values with { Shiny = shiny ? ShinyStyle.Star : ShinyStyle.NonShiny };
            values = Merge(values, draft.Details);
        }
        return values;
    }
}
