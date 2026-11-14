using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using PKHeX.Core;

namespace ZaTool.Engine;

public sealed partial class SaveService
{
    private static readonly PropertyInfo[] OptimizeFields=typeof(PokemonPatch).GetProperties();
    private static readonly JsonSerializerOptions OptimizeJson=CreateOptimizeJson();
    private static JsonSerializerOptions CreateOptimizeJson()
    {
        var result=new JsonSerializerOptions(JsonSerializerDefaults.Web);result.Converters.Add(new JsonStringEnumConverter());return result;
    }
    /// <summary>Builds one all-or-nothing draft plan. Never writes a file or creates a backup.</summary>
    public TeamOptimizePlan PlanTeamOptimize(string path,IReadOnlyList<TeamEdit> teamEdits,TeamOptimizeOptions options,AlphaMode alphaMode,string language="pt-BR")
    {
        ArgumentNullException.ThrowIfNull(teamEdits);ArgumentNullException.ThrowIfNull(options);
        if(!Enum.IsDefined(alphaMode))throw new ArgumentException("IsAlpha: invalid planning mode.");
        if(language is not("pt-BR" or "en" or "es"))throw new ArgumentException("Language: unsupported interface locale.");
        byte[] bytes=File.ReadAllBytes(path);string hash=Hash(bytes);var save=Read(bytes);
        if(teamEdits.Count>6||teamEdits.Any(e=>e==null||e.Slot<0||e.Slot>=save.PartyCount)||teamEdits.Select(e=>e.Slot).Distinct().Count()!=teamEdits.Count)
            return new(false,"invalid_team_draft",hash,[],[],["Party drafts must identify distinct occupied slots."],[]);
        var edits=teamEdits.ToDictionary(e=>e.Slot);
        var items=new List<TeamOptimizeItem>();var merged=new List<TeamOptimizeEdit>();var suggestions=new List<NatureSuggestion>();
        for(int slot=0;slot<save.PartyCount;slot++)
        {
            var original=GetDetailedPokemon(save,true,slot);var stored=DetailedSnapshot(original,true,slot,hash);
            PokemonDetails? before=null;
            try
            {
                var pending=NormalizeOptimizeDraft(original,edits.GetValueOrDefault(slot));
                var draft=original.Clone();ApplyDetails(save,draft,pending);
                before=DetailedSnapshot(draft,true,slot,hash);
                var desired=new PokemonPatch();NatureSuggestion? suggestion=null;
                if(options.BestNatures)
                {
                    suggestion=NatureAdvisor.Suggest(Describe(draft,slot),language);suggestions.Add(suggestion);
                    desired=desired with{StatNature=suggestion.Nature}; // Mint only; preserve original Nature and RNG.
                }
                if(options.Level100)desired=desired with{Level=100};
                if(options.MaxFriendship)desired=desired with{Friendship=255};
                if(options.Shiny&&!draft.IsShiny)desired=desired with{Shiny=ShinyStyle.Star};
                if(options.MaxSize)desired=desired with{Scale=255,HeightScalar=draft.Tracker==0?0:255,WeightScalar=draft.Tracker==0?0:255};
                if(options.Alpha||options.MaxIVs)
                {
                    var ivs=Enumerable.Range(0,6).Select(draft.GetIV).ToArray();
                    if(options.MaxIVs)Array.Fill(ivs,31);
                    else foreach(int index in NatureAdvisor.UsefulIVPriority(draft.Species,draft.Form,desired.StatNature??(int)draft.StatAlignment).Take(3))ivs[index]=31;
                    var hyper=new[]{draft.HT_HP,draft.HT_ATK,draft.HT_DEF,draft.HT_SPE,draft.HT_SPA,draft.HT_SPD};
                    for(int i=0;i<6;i++)if(ivs[i]==31)hyper[i]=false;
                    desired=desired with{IVs=ivs,HyperTrained=hyper};
                }
                var combined=MergeOptimizePatch(pending,desired);
                PokemonDetails after;var warnings=new List<string>();
                if(options.Alpha)
                {
                    var alpha=PlanAlpha(path,true,slot,combined,alphaMode);
                    if(!alpha.Available)
                    {
                        items.Add(new(slot,draft.Nickname,draft.Species,draft.Form,false,alpha.Code,before,null,null,[],alpha.Warnings,suggestion?.Nature,suggestion?.Reason));
                        continue;
                    }
                    combined=alpha.Patch!;after=alpha.Preview!;warnings.AddRange(alpha.Warnings);
                }
                else
                {
                    var result=original.Clone();ApplyDetails(save,result,combined);after=DetailedSnapshot(result,true,slot,hash);warnings.AddRange(after.Warnings);
                }
                var sparse=SparseOptimizeDelta(stored.Values,after.Values,combined.AlphaMode);
                // Complete replacement contract: even empty entries remove an obsolete
                // client draft that the selected optimization reverted to disk values.
                merged.Add(new(slot,sparse));
                var additions=SparseOptimizeDelta(before.Values,after.Values,combined.AlphaMode);
                items.Add(new(slot,after.Values.Nickname??draft.Nickname,after.Values.Species!.Value,after.Values.Form!.Value,true,"optimize_slot_plan",before,after,additions,
                    OptimizeChanges(before,after),warnings.Distinct().ToArray(),suggestion?.Nature,suggestion?.Reason));
            }
            catch(Exception error)when(error is ArgumentException or InvalidDataException or InvalidOperationException)
            {
                // Domain errors expose field constraints only, never filesystem exceptions or paths.
                items.Add(new(slot,original.Nickname,original.Species,original.Form,false,"invalid_team_draft",before,null,null,[],[error.Message],null,null));
            }
        }
        bool available=items.All(i=>i.Available);
        return new(available,available?"optimize_plan":"optimize_unavailable",hash,available?merged:[],items,
            available?[]:["One or more party members could not be planned. No partial batch can be applied."],suggestions);
    }

    private static PokemonPatch NormalizeOptimizeDraft(PA9 original,TeamEdit? edit)
    {
        if(edit==null)return new();
        var quick=new PokemonPatch{Level=edit.Level,Nature=edit.Nature,StatNature=edit.Nature,
            Shiny=edit.Shiny is bool shiny?shiny?(original.IsShiny?GetShinyStyle(original):ShinyStyle.Star):ShinyStyle.NonShiny:null};
        return MergeOptimizePatch(quick,edit.Details??new());
    }
    private static PokemonPatch MergeOptimizePatch(PokemonPatch original,PokemonPatch patch)
    {
        var result=original with{};
        foreach(var field in OptimizeFields)if(field.GetValue(patch) is {} value)field.SetValue(result,value is Array array?array.Clone():value);
        return result;
    }
    private static Dictionary<string,object> SparseOptimizeDelta(PokemonPatch before,PokemonPatch after,AlphaMode? policy)
    {
        var result=new Dictionary<string,object>();
        foreach(var field in OptimizeFields)
        {
            // These fields are dependent/read-only; Moves alone recalculates native PP.
            if(field.Name is nameof(PokemonPatch.PP) or nameof(PokemonPatch.PPUps) or nameof(PokemonPatch.RelearnMoves) or nameof(PokemonPatch.AlphaMode))continue;
            object? value=field.GetValue(after);
            if(value!=null&&JsonSerializer.Serialize(field.GetValue(before),OptimizeJson)!=JsonSerializer.Serialize(value,OptimizeJson))
                result[JsonNamingPolicy.CamelCase.ConvertName(field.Name)]=value is Array array?array.Clone():value;
        }
        if(result.Count>0&&after.IsAlpha==true&&(policy==AlphaMode.Custom||after.AlphaMode==AlphaMode.Custom))result["alphaMode"]=AlphaMode.Custom;
        else if(result.Count>0&&after.IsAlpha==true&&policy==AlphaMode.Compatible)result["alphaMode"]=AlphaMode.Compatible;
        return result;
    }
    private static IReadOnlyList<AlphaChange> OptimizeChanges(PokemonDetails before,PokemonDetails after)
    {
        var changes=new List<AlphaChange>();
        foreach(var field in OptimizeFields)
        {
            string oldValue=JsonSerializer.Serialize(field.GetValue(before.Values),OptimizeJson),newValue=JsonSerializer.Serialize(field.GetValue(after.Values),OptimizeJson);
            if(oldValue!=newValue)changes.Add(new(JsonNamingPolicy.CamelCase.ConvertName(field.Name),oldValue,newValue));
        }
        if(before.ReadOnly.PID!=after.ReadOnly.PID)changes.Add(new("pid",before.ReadOnly.PID.ToString(),after.ReadOnly.PID.ToString()));
        return changes;
    }
}
