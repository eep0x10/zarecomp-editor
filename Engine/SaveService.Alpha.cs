using System.Security.Cryptography;
using System.Text.Json;
using PKHeX.Core;

namespace ZaTool.Engine;

public sealed partial class SaveService
{
    public AlphaPlan PlanAlpha(string path, bool party, int slot, PokemonPatch patch, AlphaMode mode)
    {
        ArgumentNullException.ThrowIfNull(patch);
        if (!Enum.IsDefined(mode)) throw new ArgumentException("IsAlpha: invalid planning mode.");
        byte[] bytes = File.ReadAllBytes(path); var save = Read(bytes); var original = GetDetailedPokemon(save,party,slot);
        var before = DetailedSnapshot(original,party,slot,Hash(bytes));
        var normalized = AlphaDependencies(save, original, patch, mode);
        var draft = original.Clone(); ApplyDetails(save,draft,normalized with {AlphaMode=AlphaMode.Custom});
        int? minimum = AlphaEncounters(draft).Select(e=>(int?)MinimumAlphaLevel(draft,e)).Min();
        if (mode == AlphaMode.Custom)
            return FinishAlpha(before, normalized, null, DetailedSnapshot(draft,party,slot,Hash(bytes)), mode, minimum);
        if (HasAlphaOrigin(draft))
            return FinishAlpha(before, normalized, null, DetailedSnapshot(draft,party,slot,Hash(bytes)), mode, minimum);
        foreach (var encounter in AlphaEncounters(draft)
            .OrderBy(e => e.Location == draft.MetLocation ? 0 : 1)
            .ThenBy(e => e.LevelMin <= draft.MetLevel && draft.MetLevel <= e.LevelMax ? 0 : 1)
            .ThenBy(e=>e.LevelMin).ThenBy(e=>e.Location).Take(24))
        {
            // An origin adjustment never rewrites the existing PID, EC, OT, tracker or species.
            if (encounter is IFixedGender gender && gender.Gender <= 2 && gender.Gender != draft.Gender) continue;
            if (encounter is IFixedNature nature && nature.Nature.IsFixed && nature.Nature != draft.Nature) continue;
            int level = Math.Max(draft.CurrentLevel,MinimumAlphaLevel(draft,encounter));
            int met = Math.Clamp(draft.MetLevel,encounter.LevelMin,encounter.LevelMax);
            var candidatePatch = normalized with {Level=level,MetLevel=met,MetLocation=encounter.Location,ObedienceLevel=draft.IsUntraded?met:Math.Clamp(draft.ObedienceLevel,met,level)};
            try
            {
                var candidate = original.Clone(); ApplyDetails(save,candidate,candidatePatch);
                if (!HasAlphaOrigin(candidate)) continue;
                // The new encounter can require specific initial moves, notably when
                // raising a level-5 starter to its Alpha encounter's level. Propose only
                // replacements explicitly identified by PKHeX; leave already valid slots.
                var moveAnalysis=new LegalityAnalysis(candidate);
                var repaired=new[]{(int)candidate.Move1,candidate.Move2,candidate.Move3,candidate.Move4};
                bool changedMoves=false;
                for(int i=0;i<4;i++)if(!moveAnalysis.Info.Moves[i].Valid&&moveAnalysis.Info.Moves[i].Expect!=0){repaired[i]=moveAnalysis.Info.Moves[i].Expect;changedMoves=true;}
                if(changedMoves)
                {
                    candidatePatch=candidatePatch with{Moves=repaired};
                    candidate=original.Clone();ApplyDetails(save,candidate,candidatePatch);
                }
                return FinishAlpha(before,candidatePatch,null,DetailedSnapshot(candidate,party,slot,Hash(bytes)),mode,minimum);
            }
            catch (ArgumentException) { /* A constrained template is unsuitable; try the next actual encounter. */ }
        }
        return new(false,"alpha_origin_unavailable",Hash(bytes),mode,minimum,false,null,null,null,[],
            ["No compatible Alpha origin preserves this Pokémon's identity and other constraints. Custom Alpha keeps its origin and reports incompatibility honestly."]);
    }

    public AlphaPlan PlanNewAlpha(string path, AddOptions options, AlphaMode mode)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (!Enum.IsDefined(mode)) throw new ArgumentException("IsAlpha: invalid planning mode.");
        byte[] bytes = File.ReadAllBytes(path); var save = Read(bytes);
        // Generate once with a stable seed; every subsequent preview/Add uses the same identity.
        ulong seed = options.CreationSeed ?? BitConverter.ToUInt64(RandomNumberGenerator.GetBytes(8));
        var user = options.Details ?? new PokemonPatch();
        var baselineOptions = options with {CreationSeed=seed,Details=user with {IsAlpha=false,AlphaMode=null,Scale=null,HeightScalar=null,WeightScalar=null,IVs=null,HyperTrained=null}};
        var baseline = BuildNewPokemon(save,baselineOptions);
        var before = DetailedSnapshot(baseline, false,-1,Hash(bytes));
        var normalized = AlphaDependencies(save,baseline,user,mode);
        var direct = new PA9 {Species=baseline.Species,Form=baseline.Form};
        var candidates = AlphaEncounters(direct).ToArray();
        int? minimum = candidates.Select(e=>(int?)MinimumAlphaLevel(direct,e)).Min();
        if (mode == AlphaMode.Compatible && minimum == null)
            return new(false,"alpha_encounter_unavailable",Hash(bytes),mode,null,false,null,null,null,[],["No Alpha encounter exists for this species/form in the current database. Custom Alpha remains available with an incompatible legality report."]);
        int level = mode == AlphaMode.Compatible ? Math.Max(baseline.CurrentLevel,minimum!.Value) : baseline.CurrentLevel;
        var proposed = options with {CreationSeed=seed,Level=level,Details=normalized with {Level=level,MetDate=user.MetDate??baseline.MetDate}};
        if(mode==AlphaMode.Compatible) proposed=proposed with {Details=proposed.Details with {MetLocation=null,MetLevel=null,ObedienceLevel=null}};
        try
        {
            var preview = PreviewNewPokemon(path,proposed);
            if(mode==AlphaMode.Compatible) proposed=proposed with {Details=proposed.Details with {MetLocation=preview.Values.MetLocation,MetLevel=preview.Values.MetLevel,ObedienceLevel=preview.Values.ObedienceLevel}};
            return FinishAlpha(before,proposed.Details!,proposed,preview,mode,minimum);
        }
        catch (ArgumentException) when (mode == AlphaMode.Compatible)
        {
            return new(false,"alpha_constraints_conflict",Hash(bytes),mode,minimum,false,null,null,null,[],["Available Alpha encounters conflict with the selected form, gender, nature or origin fields. Adjust those choices or use explicit Custom mode."]);
        }
    }

    private static PokemonPatch AlphaDependencies(SAV9ZA save, PA9 original, PokemonPatch patch, AlphaMode mode)
    {
        var draft = original.Clone();
        var neutral = patch with {IsAlpha=null,AlphaMode=null,Scale=null,HeightScalar=null,WeightScalar=null,IVs=null,HyperTrained=null};
        ApplyDetails(save,draft,neutral);
        var ivs = patch.IVs?.ToArray() ?? Enumerable.Range(0,6).Select(draft.GetIV).ToArray(); CheckStats(ivs,31,"IVs");
        foreach (int index in Enumerable.Range(0,6).OrderByDescending(i=>ivs[i])) {if(ivs.Count(v=>v==31)>=3)break;ivs[index]=31;}
        var hyper = patch.HyperTrained?.ToArray() ?? new[] {draft.HT_HP,draft.HT_ATK,draft.HT_DEF,draft.HT_SPE,draft.HT_SPA,draft.HT_SPD};
        if(hyper.Length!=6)throw new ArgumentException("HyperTrained: six flags are required.");
        for(int i=0;i<6;i++)if(ivs[i]==31)hyper[i]=false;
        return patch with {IsAlpha=true,AlphaMode=mode,Scale=255,HeightScalar=draft.Tracker==0?0:255,WeightScalar=draft.Tracker==0?0:255,IVs=ivs,HyperTrained=hyper};
    }
    private static IEnumerable<IEncounterable> AlphaEncounters(PA9 pk)
    {
        var species = EvolutionTree.Evolves9a.Reverse.GetPreEvolutions(pk.Species,pk.Form).Append((pk.Species,pk.Form));
        var chain = species.Select(s=>new EvoCriteria {Species=s.Item1,Form=s.Item2,LevelMin=1,LevelMax=100}).ToArray();
        return EncounterGenerator9a.Instance.GetPossible(pk,chain,GameVersion.ZA,EncounterTypeGroup.Static|EncounterTypeGroup.Slot)
            .Where(e=>e is IAlphaReadOnly {IsAlpha:true} && chain.Any(s=>s.Species==e.Species && (s.Form==e.Form || e.Form>=EncounterUtil.FormDynamic)));
    }
    private static int MinimumAlphaLevel(PA9 target,IEncounterable encounter)
    {
        var lineage=EvolutionTree.Evolves9a.Reverse.GetPreEvolutions(target.Species,target.Form).Append((target.Species,target.Form)).ToArray();
        int origin=Array.FindIndex(lineage,s=>s.Item1==encounter.Species&&(s.Item2==encounter.Form||encounter.Form>=EncounterUtil.FormDynamic));
        int level=encounter.LevelMin;
        for(int i=Math.Max(0,origin);i<lineage.Length-1;i++)
        {
            var next=lineage[i+1];
            var methods=EvolutionTree.Evolves9a.Forward.GetForward(lineage[i].Item1,lineage[i].Item2).ToArray().Where(e=>e.Species==next.Item1&&e.GetDestinationForm(lineage[i].Item2)==next.Item2).ToArray();
            if(methods.Length!=0)level=methods.Min(e=>Math.Max(level+e.LevelUp,e.Level));
        }
        return level;
    }
    private static bool HasAlphaOrigin(PA9 pk)
    {
        var analysis = new LegalityAnalysis(pk);
        return analysis.EncounterMatch is IAlphaReadOnly {IsAlpha:true} && !analysis.Results.Any(r=>!r.Valid&&r.Result==LegalityCheckResultCode.StatAlphaInvalid);
    }
    private static AlphaPlan FinishAlpha(PokemonDetails before, PokemonPatch patch, AddOptions? options, PokemonDetails preview, AlphaMode mode, int? minimum)
    {
        var changes = new List<AlphaChange>();
        var json = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        foreach (var field in typeof(PokemonPatch).GetProperties())
        {
            string oldValue=JsonSerializer.Serialize(field.GetValue(before.Values),json), newValue=JsonSerializer.Serialize(field.GetValue(preview.Values),json);
            if(oldValue!=newValue)changes.Add(new(JsonNamingPolicy.CamelCase.ConvertName(field.Name),oldValue,newValue));
        }
        foreach(var field in new[]{nameof(PokemonReadOnly.PID),nameof(PokemonReadOnly.EncryptionConstant)})
        {
            var property=typeof(PokemonReadOnly).GetProperty(field)!;
            string oldValue=JsonSerializer.Serialize(property.GetValue(before.ReadOnly)),newValue=JsonSerializer.Serialize(property.GetValue(preview.ReadOnly));
            if(oldValue!=newValue)changes.Add(new(JsonNamingPolicy.CamelCase.ConvertName(field),oldValue,newValue));
        }
        bool origin = before.Values.MetLocation!=preview.Values.MetLocation||before.Values.MetLevel!=preview.Values.MetLevel;
        int locationChange=changes.FindIndex(c=>c.Field=="metLocation");
        if(locationChange>=0)
        {
            string Label(int? id)=>id is int value?Catalog.MetLocations.FirstOrDefault(c=>c.Id==value)?.Name ?? $"Location ({value})":"none";
            changes[locationChange]=new("metLocation",Label(before.Values.MetLocation),Label(preview.Values.MetLocation));
        }
        var warnings=preview.Warnings.ToList();
        if(mode==AlphaMode.Custom)warnings.Insert(0,"Custom Alpha changes the flag and required size/IV dependencies while preserving the requested origin. The PKHeX report remains authoritative; incompatible encounter/RNG results are not hidden.");
        if(!preview.Legal)warnings.Add("This plan does not make the entire Pokémon legal. Review the unchanged PKHeX legality report, including any RNG correlation errors.");
        return new(true,mode==AlphaMode.Custom?"alpha_custom_plan":"alpha_compatible_plan",before.Hash,mode,minimum,origin,patch,options,preview,changes,warnings);
    }
}
