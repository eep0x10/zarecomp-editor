using System.Reflection;

namespace ZaTool.Engine;

/// <summary>A transparent starting point, not a claim about a universal competitive set.</summary>
public static class NatureAdvisor
{
    private static readonly byte[] Categories = LoadCategories();
    private static byte[] LoadCategories()
    {
        using var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("ZaTool.Engine.move-categories.bin");
        if(stream is null)return [];
        using var memory=new MemoryStream(); stream.CopyTo(memory); return memory.ToArray();
    }
    public static int GetMoveCategory(int id)=>id>=0&&id<Categories.Length&&Categories[id]!=255?Categories[id]:-1;

    public static NatureSuggestion Suggest(TeamPokemon p,string language="pt-BR")
    {
        // Canonical species/form data is the entire decision basis. In particular,
        // don't consume caller-derived stats or a moveset changed by a prior plan.
        int[] basis=Catalog.BaseStats(p.Species,p.Form);
        int atk=basis[1],def=basis[2],speed=basis[3],spa=basis[4],spd=basis[5];
        bool prefersPhysical=atk>=spa;
        bool mixed=Math.Min(atk,spa)>=80&&Math.Abs(atk-spa)<20;
        int nature; string reason;
        if(speed<70 && Math.Max(def,spd)>=Math.Max(atk,spa)+20)
        {
            if(prefersPhysical) nature=def>=spd?8:23; // Impish / Careful; retain Attack.
            else nature=def>=spd?5:20; // Bold / Calm; retain Special Attack.
            reason=T(language,"Perfil-base lento e defensivo: reforça a maior defesa e reduz o ataque-base menor.","Slow defensive base profile: boosts the stronger defense and lowers the smaller base attack.","Perfil base lento y defensivo: refuerza la mayor defensa y reduce el menor ataque base.");
        }
        else if(mixed)
        {
            nature=def>=spd?14:11; // Naive / Hasty: avoid reducing either attack.
            reason=T(language,"Ataques-base altos e próximos: preserva ambos, aumenta Velocidade e reduz a defesa-base menor.","High, similar base attacks: retains both, boosts Speed and lowers the smaller base defense.","Ataques base altos y próximos: conserva ambos, aumenta Velocidad y reduce la menor defensa base.");
        }
        else if(prefersPhysical)
        {
            nature=3;
            reason=T(language,"Ataque-base físico predominante: Adamant aumenta Ataque e reduz Ataque Especial.","Predominantly physical base attack: Adamant boosts Attack and lowers Special Attack.","Predomina el Ataque base físico: Adamant aumenta Ataque y reduce Ataque Especial.");
        }
        else
        {
            nature=15;
            reason=T(language,"Ataque-base especial predominante: Modest aumenta Ataque Especial e reduz Ataque.","Predominantly special base attack: Modest boosts Special Attack and lowers Attack.","Predomina el Ataque Especial base: Modest aumenta Ataque Especial y reduce Ataque.");
        }
        reason+=" "+T(language,"Heurística estável da espécie/forma, não uma escolha universal; golpes, função, Mega Evolução e adversários podem mudar a melhor opção.","Stable species/form heuristic, not a universal best choice; moves, role, Mega Evolution and opponents can change the best option.","Heurística estable de especie/forma, no una elección universal; movimientos, rol, Megaevolución y rivales pueden cambiar la mejor opción.");
        return new(p.Slot,nature,reason);
    }
    public static IReadOnlyList<int> UsefulIVPriority(int species,int form,int effectiveNature)
    {
        if(effectiveNature is <0 or >24)throw new ArgumentException("StatNature: use 0..24.");
        var basis=Catalog.BaseStats(species,form);
        int boost=effectiveNature/5+1,reduced=effectiveNature%5+1;
        if(boost==reduced){boost=-1;reduced=-1;}
        int offense=boost is 1 or 4?boost:reduced==1?4:reduced==4?1:basis[1]>=basis[4]?1:4;
        int defense=basis[2]>=basis[5]?2:5;
        int third=boost is 2 or 3 or 5?boost:basis[3]>=70&&reduced!=3?3:defense;
        return new[]{boost,0,offense,third,defense,3,1,4,2,5}.Where(i=>i>=0).Distinct().ToArray();
    }
    private static string T(string l,string pt,string en,string es)=>l.StartsWith("es",StringComparison.OrdinalIgnoreCase)?es:l.StartsWith("en",StringComparison.OrdinalIgnoreCase)?en:pt;
}
