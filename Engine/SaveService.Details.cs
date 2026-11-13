using PKHeX.Core;

namespace ZaTool.Engine;

public sealed partial class SaveService
{
    private static readonly string[] UnsupportedDetails =
    [
        "PP is stored but unused by Z-A combat; native moves use calculated base PP. PP Ups are unavailable.",
        "Relearn slots are legacy/transfer data and are preserved read-only; Z-A uses its learnset and Plus records.",
        "Ability, origin game, egg data, PID, encryption constant, HOME tracker, handler memories and awarded ribbons are read-only.",
        "Persistent forms only: this editor does not write battle-only Mega/Primal forms. Held items include Z-A Mega Stones.",
        "Native Z-A uses Scale; HeightScalar/WeightScalar must remain zero. HOME records must keep both equal to Scale.",
        "Changing trainer identity updates the handler to match save ownership. Changing TID/SID preserves the previous shiny style unless specified.",
    ];
    private static bool HasDetails(PokemonPatch? patch) => patch != null && typeof(PokemonPatch).GetProperties().Any(p => p.GetValue(patch) != null);

    public PokemonDetails InspectPokemon(string path, bool party, int slot)
    {
        var bytes = File.ReadAllBytes(path); var save = Read(bytes);
        return DetailedSnapshot(GetDetailedPokemon(save, party, slot), party, slot, Hash(bytes));
    }
    public PokemonDetails PreviewPokemon(string path, bool party, int slot, PokemonPatch patch)
    {
        ArgumentNullException.ThrowIfNull(patch);
        var bytes = File.ReadAllBytes(path); var save = Read(bytes);
        var pk = GetDetailedPokemon(save, party, slot);
        ApplyDetails(save, pk, patch);
        return DetailedSnapshot(pk, party, slot, Hash(bytes));
    }
    public PokemonDetails PreviewNewPokemon(string path, AddOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var bytes = File.ReadAllBytes(path); var save = Read(bytes); Validate(save, options);
        var pk = BuildNewPokemon(save, options);
        return DetailedSnapshot(pk, options.Destination != Destination.Box && save.PartyCount < 6, -1, Hash(bytes));
    }
    private static PA9 BuildNewPokemon(SAV9ZA save,AddOptions options)
    {
        var pk=CreatePokemon(save,options);
        if(options.Details!=null)ApplyDetails(save,pk,options.Details);
        // New Pokémon start at full health after final IV/level/customization. Existing
        // edits still preserve HP/status; don't let template pre-patch stats leak into Add.
        pk.ResetPartyStats();pk.RefreshChecksum();return pk;
    }
    private static PA9 GetDetailedPokemon(SAV9ZA save, bool party, int slot)
    {
        if (slot < 0 || slot >= (party ? save.PartyCount : AccessibleBoxes(save) * 30)) throw new ArgumentException("Posição inválida ou bloqueada.");
        var pk = (PA9)(party ? save.GetPartySlotAtIndex(slot) : save.GetBoxSlotAtIndex(slot));
        if (pk.Species == 0) throw new ArgumentException("A posição selecionada está vazia.");
        if (!pk.ChecksumValid) throw new InvalidDataException("Pokémon tem checksum inválido.");
        return pk;
    }
    private static ShinyStyle GetShinyStyle(PKM pk) => !pk.IsShiny ? ShinyStyle.NonShiny : pk.ShinyXor == 0 ? ShinyStyle.Square : ShinyStyle.Star;
    private static int[] ReadPP(PKM pk) => [pk.Move1_PP,pk.Move2_PP,pk.Move3_PP,pk.Move4_PP];
    private static int[] ReadPPUps(PKM pk) => [pk.Move1_PPUps,pk.Move2_PPUps,pk.Move3_PPUps,pk.Move4_PPUps];
    private static int[] ReadRelearn(PKM pk) => [pk.RelearnMove1,pk.RelearnMove2,pk.RelearnMove3,pk.RelearnMove4];
    private static int[] ReadPlus(PA9 pk) => pk.PersonalInfo.PlusMoveIndexes.ToArray().Select((m,i) => (Move:m, Index:i)).Where(p => pk.GetMovePlusFlag(p.Index)).Select(p => (int)p.Move).ToArray();
    private static PokemonDetails DetailedSnapshot(PA9 pk, bool party, int slot, string hash)
    {
        var legality = new LegalityAnalysis(pk);
        var warnings = new List<string>();
        if (!legality.Valid) warnings.Add("PKHeX sinaliza legalidade; este editor não garante compatibilidade online.");
        if (pk.CurrentLevel < pk.MetLevel) warnings.Add("Nível abaixo do nível original de encontro.");
        var values = new PokemonPatch
        {
            Species=pk.Species, Form=pk.Form, Level=pk.CurrentLevel, Nickname=pk.Nickname,IsNicknamed=pk.IsNicknamed,
            Gender=pk.Gender,Shiny=GetShinyStyle(pk),Nature=(int)pk.Nature,StatNature=(int)pk.StatAlignment,
            Ball=pk.Ball,HeldItem=pk.HeldItem,Language=pk.Language,Friendship=pk.CurrentFriendship,
            IVs=Enumerable.Range(0,6).Select(pk.GetIV).ToArray(),EVs=Enumerable.Range(0,6).Select(pk.GetEV).ToArray(),HyperTrained=[pk.HT_HP,pk.HT_ATK,pk.HT_DEF,pk.HT_SPE,pk.HT_SPA,pk.HT_SPD],
            Moves=[pk.Move1,pk.Move2,pk.Move3,pk.Move4],PP=ReadPP(pk),PPUps=ReadPPUps(pk),RelearnMoves=ReadRelearn(pk),PlusMoves=ReadPlus(pk),
            MetLevel=pk.MetLevel,MetLocation=pk.MetLocation,MetDate=pk.MetDate,ObedienceLevel=pk.ObedienceLevel,
            OriginalTrainerName=pk.OriginalTrainerName,OriginalTrainerGender=pk.OriginalTrainerGender,TID16=pk.TID16,SID16=pk.SID16,
            IsAlpha=pk.IsAlpha,AlphaMode=pk.IsAlpha ? (legality.Results.Any(r=>!r.Valid&&r.Result==LegalityCheckResultCode.StatAlphaInvalid) ? Engine.AlphaMode.Custom : Engine.AlphaMode.Compatible) : null,HeightScalar=pk.HeightScalar,WeightScalar=pk.WeightScalar,Scale=pk.Scale,AffixedRibbon=pk.AffixedRibbon,
            Favorite=pk.IsFavorite,Markings=Enumerable.Range(0,6).Select(i => (int)pk.GetMarking(i)).ToArray(),
        };
        var readOnly = new PokemonReadOnly(pk.PID,pk.EncryptionConstant,pk.Tracker,(int)pk.Version,pk.IsEgg,pk.EggLocation,pk.EggMetDate,
            pk.CurrentHandler,pk.HandlingTrainerName,pk.Ability,pk.AbilityNumber,
            Catalog.Ribbons.Where(r => pk.GetRibbon(r.Id)).Select(r => r.Id).ToArray(),(int)pk.GetDisplayTID(),(int)pk.GetDisplaySID(),UnsupportedDetails.ToArray());
        var calculated=pk.Clone();calculated.ResetPartyStats();
        return new(slot,party,hash,values,readOnly,legality.Valid,legality.Report(),warnings)
        { Stats=[calculated.Stat_HPMax,calculated.Stat_ATK,calculated.Stat_DEF,calculated.Stat_SPE,calculated.Stat_SPA,calculated.Stat_SPD] };
    }

    private static void ApplyDetails(SAV9ZA save, PA9 pk, PokemonPatch patch)
    {
        var prior = pk.Clone(); var shinyBefore = GetShinyStyle(pk);
        if(patch.AlphaMode is { } alphaMode && (!Enum.IsDefined(alphaMode) || !(patch.IsAlpha??pk.IsAlpha))) throw new ArgumentException("IsAlpha: AlphaMode requires an Alpha Pokémon.");
        Range(patch.Species,1,save.MaxSpeciesID,"Species"); Range(patch.Form,0,255,"Form");Range(patch.Level,1,100,"Level");
        Range(patch.Gender,0,2,"Gender");Range(patch.Nature,0,24,"Nature");Range(patch.StatNature,0,24,"StatNature");
        Range(patch.Language,1,11,"Language");Range(patch.Friendship,0,255,"Friendship");
        Range(patch.MetLevel,1,100,"MetLevel");Range(patch.MetLocation,0,65535,"MetLocation");Range(patch.ObedienceLevel,0,100,"ObedienceLevel");
        Range(patch.OriginalTrainerGender,0,1,"OriginalTrainerGender");Range(patch.TID16,0,65535,"TID16");Range(patch.SID16,0,65535,"SID16");
        Range(patch.HeightScalar,0,255,"HeightScalar");Range(patch.WeightScalar,0,255,"WeightScalar");Range(patch.Scale,0,255,"Scale");
        if(patch.Shiny is { } shiny && !Enum.IsDefined(shiny)) throw new ArgumentException("Shiny: estilo inválido.");
        if(patch.Language is int language && !Catalog.Languages.Any(c=>c.Id==language)) throw new ArgumentException("Language: idioma não disponível em Z-A.");
        if(patch.Ball is int ball && !Catalog.Balls.Any(c=>c.Id==ball)) throw new ArgumentException("Ball: Pokébola não disponível para captura em Z-A.");
        if(patch.HeldItem is int item && item!=0 && !save.HeldItems.Contains((ushort)item)) throw new ArgumentException("HeldItem: item não disponível em Z-A.");
        Range(patch.HeldItem,0,save.MaxItemID,"HeldItem");
        Text(patch.Nickname,12,"Nickname");Text(patch.OriginalTrainerName,12,"OriginalTrainerName");
        if(patch.OriginalTrainerName is "") throw new ArgumentException("OriginalTrainerName: nome obrigatório.");
        if(patch.MetDate is DateOnly date && (date.Year<2000||date.Year>2099)) throw new ArgumentException("MetDate: use uma data entre 2000 e 2099.");
        if(patch.MetLocation is int location && (pk.Version!=GameVersion.ZA || !Catalog.MetLocations.Any(c=>c.Id==location))) throw new ArgumentException("MetLocation: escolha um local de origem nativa de Z-A.");
        CheckStats(patch.IVs,31,"IVs");CheckStats(patch.EVs,252,"EVs");
        if(patch.EVs?.Sum()>510) throw new ArgumentException("EVs não podem somar mais de 510.");
        if(patch.HyperTrained!=null&&patch.HyperTrained.Length!=6) throw new ArgumentException("HyperTrained: seis flags são necessárias.");
        ArrayRange(patch.Markings,6,0,2,"Markings");
        // Stored legacy fields are not free combat settings in Z-A.
        PreserveOnly(patch.PP,ReadPP(pk),"PP");PreserveOnly(patch.PPUps,ReadPPUps(pk),"PPUps");PreserveOnly(patch.RelearnMoves,ReadRelearn(pk),"RelearnMoves");
        bool identity=patch.Species!=null||patch.Form!=null;
        if(patch.Species is int species) pk.Species=(ushort)species;
        if(patch.Form is int form) pk.Form=(byte)form;
        if(identity&&!Catalog.Forms(pk.Species).Any(c=>c.Id==pk.Form)) throw new ArgumentException("Species/Form: esta forma persistente não está disponível em Z-A.");
        if(identity)
        {
            pk.RefreshAbility(prior.AbilityNumber switch {2=>1,4=>2,_=>0});
            if(pk.Species!=prior.Species)pk.FormArgument=0;
        }
        if(patch.Level is int level) pk.CurrentLevel=(byte)level;
        if(patch.Gender is int gender) pk.Gender=(byte)gender;
        if((identity||patch.Gender!=null)&&!Catalog.Genders(pk.Species,pk.Form).Any(c=>c.Id==pk.Gender)) throw new ArgumentException("Gender: incompatível com a espécie/forma escolhida.");
        if(patch.Nature is int nature) pk.Nature=(Nature)nature;
        if(patch.StatNature is int mint) pk.StatAlignment=(Nature)mint;
        if((patch.Nature!=null||patch.StatNature!=null)&&pk.StatAlignment!=pk.Nature&&!pk.StatAlignment.IsMint) throw new ArgumentException("StatNature: use uma natureza com mint disponível, ou a natureza original.");
        if(patch.Ball is int newBall) pk.Ball=(byte)newBall;
        if(patch.HeldItem is int held) pk.HeldItem=held;
        if(patch.Language is int lang) pk.Language=lang;
        if(patch.IsNicknamed is bool named) pk.IsNicknamed=named;
        if(patch.Nickname!=null)
        {
            pk.IsNicknamed=!string.IsNullOrEmpty(patch.Nickname);
            pk.Nickname=pk.IsNicknamed?patch.Nickname:SpeciesName.GetSpeciesNameGeneration(pk.Species,pk.Language,9);
        }
        if(patch.IsNicknamed is bool namedFinal)pk.IsNicknamed=namedFinal;
        if(patch.IsNicknamed is true && string.IsNullOrEmpty(patch.Nickname??pk.Nickname))throw new ArgumentException("Nickname: apelido não pode ser vazio quando IsNicknamed é verdadeiro.");
        if(patch.IsNicknamed is false || ((identity||patch.Language!=null)&&!pk.IsNicknamed))
            pk.Nickname=SpeciesName.GetSpeciesNameGeneration(pk.Species,pk.Language,9);
        if(patch.IVs!=null) pk.SetIVs(patch.IVs);
        if(patch.EVs!=null) pk.SetEVs(patch.EVs);
        if(patch.HyperTrained is { } ht) {pk.HT_HP=ht[0];pk.HT_ATK=ht[1];pk.HT_DEF=ht[2];pk.HT_SPE=ht[3];pk.HT_SPA=ht[4];pk.HT_SPD=ht[5];}
        if(patch.HyperTrained!=null||patch.IVs!=null||patch.Level!=null)
        {
            if(pk.HyperTrainFlags!=0&&pk.CurrentLevel<50) throw new ArgumentException("HyperTrained: nível mínimo 50.");
            for(int i=0;i<6;i++) if(pk.IsHyperTrained(i)&&pk.GetIV(i)==31) throw new ArgumentException("HyperTrained: um IV 31 não pode receber Hyper Training.");
        }
        if(patch.Moves!=null)
        {
            ArrayRange(patch.Moves,4,0,save.MaxMoveID,"Moves");
            if(patch.Moves.All(m=>m==0)||patch.Moves.Where(m=>m!=0).Distinct().Count()!=patch.Moves.Count(m=>m!=0)) throw new ArgumentException("Moves: ao menos um golpe, sem repetidos.");
            pk.SetMoves(patch.Moves.Select(m=>(ushort)m).ToArray());pk.HealPP();
        }
        if(identity||patch.Moves!=null) ValidateDetailedMoves(save,pk,patch.IsAlpha);
        if(patch.OriginalTrainerName!=null)pk.OriginalTrainerName=patch.OriginalTrainerName;
        if(patch.OriginalTrainerGender is int otGender)pk.OriginalTrainerGender=(byte)otGender;
        if(patch.TID16 is int tid)pk.TID16=(ushort)tid;
        if(patch.SID16 is int sid)pk.SID16=(ushort)sid;
        if(patch.MetLevel is int metLevel) {pk.MetLevel=(byte)metLevel;if(patch.ObedienceLevel==null&&pk.IsUntraded)pk.ObedienceLevel=pk.MetLevel;}
        if(patch.MetLocation is int metLocation)pk.MetLocation=(ushort)metLocation;
        if(patch.MetDate is DateOnly metDate)pk.MetDate=metDate;
        if(patch.ObedienceLevel is int obedience)pk.ObedienceLevel=(byte)obedience;
        if(patch.TID16!=null||patch.SID16!=null||patch.OriginalTrainerName!=null||patch.OriginalTrainerGender!=null||patch.Language!=null)
        {
            pk.UpdateHandler(save);pk.CurrentFriendship=prior.CurrentFriendship;
        }
        if(patch.Friendship is int friendship)pk.CurrentFriendship=(byte)friendship;
        if(patch.Shiny!=null||patch.TID16!=null||patch.SID16!=null)ApplyShinyStyle(pk,patch.Shiny??shinyBefore);
        if(patch.IsAlpha is bool alpha)pk.IsAlpha=alpha;
        if(patch.HeightScalar is int height)pk.HeightScalar=(byte)height;
        if(patch.WeightScalar is int weight)pk.WeightScalar=(byte)weight;
        if(patch.Scale is int scale)pk.Scale=(byte)scale;
        if(patch.Favorite is bool favorite)pk.IsFavorite=favorite;
        if(patch.Markings is { } markings)for(int i=0;i<6;i++)pk.SetMarking(i,(MarkingColor)markings[i]);
        if(patch.AffixedRibbon is int ribbon)
        {
            if(ribbon!=-1&&(ribbon<0||ribbon>127||!Catalog.Ribbons.Any(c=>c.Id==ribbon)||!pk.GetRibbon(ribbon)))throw new ArgumentException("AffixedRibbon: selecione uma fita já existente, ou -1 para nenhuma.");
            pk.AffixedRibbon=(sbyte)ribbon;
        }
        if(identity)pk.ClearMovePlusFlags();
        if(patch.PlusMoves is { } requestedPlus)
        {
            var possible=GetDetailedLearnable(save,pk,100,patch.IsAlpha);
            if(requestedPlus.Distinct().Count()!=requestedPlus.Length||requestedPlus.Any(m=>m<=0||m>save.MaxMoveID||!possible[m]||!pk.PersonalInfo.PlusMoveIndexes.Contains((ushort)m)))throw new ArgumentException("PlusMoves: registro não permitido para esta espécie/forma.");
            pk.ClearMovePlusFlags();pk.SetPlusFlagsSpecific(pk.PersonalInfo,requestedPlus.Select(m=>(ushort)m).ToArray());
        }
        if(identity||patch.Level!=null||patch.Moves!=null||patch.PlusMoves!=null||patch.IsAlpha!=null||patch.AlphaMode!=null)ReconcileNaturalPlus(pk);
        if(patch.IsAlpha!=null||patch.AlphaMode!=null||patch.Scale!=null||patch.HeightScalar!=null||patch.WeightScalar!=null||patch.IVs!=null||patch.MetLevel!=null||patch.MetLocation!=null||identity)
        {
            if((pk.Tracker!=0) && (pk.HeightScalar!=pk.Scale||pk.WeightScalar!=pk.Scale))throw new ArgumentException("Scale: registro HOME exige HeightScalar e WeightScalar iguais a Scale.");
            if(!(pk.Tracker!=0) && (pk.HeightScalar!=0||pk.WeightScalar!=0))throw new ArgumentException("Scale: origem nativa usa Scale; HeightScalar e WeightScalar devem ser zero.");
            if(pk.IsAlpha&&(pk.Scale!=255||Enumerable.Range(0,6).Count(i=>pk.GetIV(i)==31)<3))throw new ArgumentException("IsAlpha: Alpha exige Scale 255 e ao menos três IVs 31.");
            bool originOrMode=patch.IsAlpha!=null||patch.AlphaMode!=null||pk.IsAlpha&&(identity||patch.MetLevel!=null||patch.MetLocation!=null);
            if(originOrMode && patch.AlphaMode!=Engine.AlphaMode.Custom && new LegalityAnalysis(pk).Results.Any(r=>!r.Valid&&r.Result==LegalityCheckResultCode.StatAlphaInvalid))throw new ArgumentException("IsAlpha: os dados de encontro não correspondem a um encontro Alpha válido.");
        }
        if(patch.ObedienceLevel!=null||patch.MetLevel!=null)
        {
            if(pk.IsUntraded&&pk.ObedienceLevel!=pk.MetLevel || !pk.IsUntraded&&(pk.ObedienceLevel<pk.MetLevel||pk.ObedienceLevel>pk.CurrentLevel))throw new ArgumentException("ObedienceLevel: incompatível com nível de encontro, nível atual ou treinador.");
        }
        if(identity||patch.Level!=null||patch.Nature!=null||patch.StatNature!=null||patch.IVs!=null||patch.EVs!=null||patch.HyperTrained!=null)
        {
            int hp=pk.Stat_HPCurrent,status=pk.Status_Condition;pk.ResetPartyStats();pk.Stat_HPCurrent=Math.Min(hp,pk.Stat_HPMax);pk.Status_Condition=status;
        }
        pk.RefreshChecksum();
    }
    private static void ReconcileNaturalPlus(PA9 pk)
    {
        var pi=pk.PersonalInfo;pk.SetPlusFlagsEncounter(pi,LearnSource9ZA.GetLearnsetAndPlus(pk.Species,pk.Form).Plus,pk.CurrentLevel);
        if(pk.Form!=0&&pk.Species is 479 or 720)pk.SetPlusFlagsEncounter(pi,LearnSource9ZA.GetLearnsetAndPlus(pk.Species,0).Plus,pk.CurrentLevel);
        if(pk.IsAlpha&&pi.AlphaMove!=0)pk.SetPlusFlagsSpecific(pi,pi.AlphaMove);
        // Evolved Alphas retain the required move of their original encounter species.
        if(pk.IsAlpha&&new LegalityAnalysis(pk).EncounterMatch is IEncounter9a {IsAlpha:true} encounter)
        {
            var encounterMove=PersonalTable.ZA[encounter.Species,encounter.Form].AlphaMove;
            if(encounterMove!=0)pk.SetPlusFlagsSpecific(pi,encounterMove);
        }
    }
    private static bool[] GetDetailedLearnable(SAV9ZA save,PA9 pk,byte level,bool? alpha=null)
    {
        var moves=new bool[save.MaxMoveID+1];
        LearnSource9ZA.Instance.GetAllMoves(moves,pk,new EvoCriteria {Species=pk.Species,Form=pk.Form,LevelMax=level});
        if((alpha??pk.IsAlpha)&&pk.PersonalInfo.AlphaMove<moves.Length)moves[pk.PersonalInfo.AlphaMove]=true;
        return moves;
    }
    private static void ValidateDetailedMoves(SAV9ZA save,PA9 pk,bool? alpha=null)
    {
        var moves=GetDetailedLearnable(save,pk,pk.CurrentLevel,alpha);
        for(int i=0;i<4;i++) {var move=pk.GetMove(i);if(move!=0&&(move>save.MaxMoveID||!moves[move]||MoveInfo.IsDummiedMove(pk,move)))throw new ArgumentException("Moves: golpe indisponível para a espécie/forma e nível escolhidos em Z-A.");}
    }
    private static void ApplyShinyStyle(PA9 pk,ShinyStyle style)
    {
        if(GetShinyStyle(pk)==style)return;
        if(style==ShinyStyle.NonShiny){if(pk.IsShiny)pk.PID^=0x10000000;return;}
        uint low=pk.PID&0xFFFF;pk.PID=low|((low^pk.TID16^pk.SID16^(style==ShinyStyle.Square?0u:1u))<<16);
    }
    private static void Range(int? value,int min,int max,string field)
    {if(value is int v&&(v<min||v>max))throw new ArgumentException($"{field}: use {min}..{max}.");}
    private static void ArrayRange(int[]? values,int count,int min,int max,string field)
    {if(values!=null&&(values.Length!=count||values.Any(v=>v<min||v>max)))throw new ArgumentException($"{field}: informe {count} valores entre {min} e {max}.");}
    private static void PreserveOnly(int[]? values,int[] current,string field)
    {if(values!=null&&!values.SequenceEqual(current))throw new NotSupportedException($"{field}: dados somente para leitura em Z-A; PP é calculado ao selecionar golpes.");}
    private static void Text(string? value,int max,string field)
    {
        if(value==null)return;
        if(value.Length>max||value.Any(char.IsControl))throw new ArgumentException($"{field}: até {max} caracteres válidos, sem controles.");
        for(int i=0;i<value.Length;i++)
        {
            if(!char.IsSurrogate(value[i]))continue;
            if(!char.IsHighSurrogate(value[i])||i+1>=value.Length||!char.IsLowSurrogate(value[++i]))throw new ArgumentException($"{field}: texto Unicode inválido.");
        }
    }
}
