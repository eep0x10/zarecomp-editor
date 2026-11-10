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
        if(p.BaseStats.Length!=6) return new(p.Slot,p.Nature,T(language,"Dados insuficientes; mantenha a natureza atual.","Not enough data; keep the current nature.","Datos insuficientes; conserva la naturaleza actual."));
        var categories=p.Moves.Where(m=>m!=0).Select(GetMoveCategory).ToArray();
        int physical=categories.Count(c=>c==1),special=categories.Count(c=>c==2),status=categories.Count(c=>c==0);
        int atk=p.BaseStats[1],def=p.BaseStats[2],spa=p.BaseStats[4],spd=p.BaseStats[5];
        bool prefersPhysical=physical>special || physical==special&&atk>=spa;
        bool mixed=physical>0&&special>0&&Math.Abs(atk-spa)<20&&Math.Abs(physical-special)<=1;
        int nature; string reason;
        if(status>=3 && Math.Max(def,spd)>=Math.Max(atk,spa))
        {
            if(prefersPhysical) nature=def>=spd?8:23; // Impish / Careful; retain Attack.
            else nature=def>=spd?5:20; // Bold / Calm; retain Special Attack.
            reason=T(language,"Predomínio de suporte e defesa: reforça a defesa mais alta e reduz o ataque menos usado.","Support and defensive profile: boosts the stronger defense and lowers the less-used attack.","Perfil de apoyo y defensa: refuerza la defensa mayor y reduce el ataque menos usado.");
        }
        else if(mixed)
        {
            nature=def>=spd?14:11; // Naive / Hasty: avoid reducing either attack.
            reason=T(language,"Golpes físicos e especiais equilibrados: preserva ambos os ataques, ganha Velocidade e reduz a defesa menor.","Balanced physical and special moves: retains both attacks, gains Speed and lowers the weaker defense.","Movimientos físicos y especiales equilibrados: conserva ambos ataques, gana Velocidad y reduce la defensa menor.");
        }
        else if(prefersPhysical)
        {
            nature=3;
            reason=T(language,"Perfil físico pelos golpes e atributos: Adamant aumenta Ataque e reduz Ataque Especial.","Physical profile from moves and base stats: Adamant boosts Attack and lowers Special Attack.","Perfil físico según movimientos y atributos: Firme aumenta Ataque y reduce Ataque Especial.");
        }
        else
        {
            nature=15;
            reason=T(language,"Perfil especial pelos golpes e atributos: Modest aumenta Ataque Especial e reduz Ataque.","Special profile from moves and base stats: Modest boosts Special Attack and lowers Attack.","Perfil especial según movimientos y atributos: Modesta aumenta Ataque Especial y reduce Ataque.");
        }
        if(physical+special==0)reason=T(language,"Sem golpes ofensivos reconhecidos: sugestão baseada nos atributos-base. ","No recognized damaging moves: suggestion uses base stats. ","Sin movimientos ofensivos reconocidos: sugerencia basada en atributos base. ")+reason;
        reason+=" "+T(language,"Sugestão inicial: função, Mega Evolução, tempos de recarga e adversários podem mudar a escolha.","Starting point only: role, Mega Evolution, cooldowns and opponents may change the choice.","Solo un punto de partida: el rol, la Megaevolución, las recargas y los rivales pueden cambiar la elección.");
        return new(p.Slot,nature,reason);
    }
    private static string T(string l,string pt,string en,string es)=>l.StartsWith("es",StringComparison.OrdinalIgnoreCase)?es:l.StartsWith("en",StringComparison.OrdinalIgnoreCase)?en:pt;
}
