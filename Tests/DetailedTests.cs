using System.Security.Cryptography;
using PKHeX.Core;
using ZaTool.Engine;

internal static class DetailedTests
{
    public static void Run(string source)
    {
        byte[] input=File.ReadAllBytes(source);string originalHash=Hash(input);
        string root=Path.Combine(AppContext.BaseDirectory,"data","details-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
        var service=new SaveService();int passed=0;
        void Check(bool condition,string message){if(!condition)throw new Exception(message);passed++;Console.WriteLine("PASS "+message);}
        string Fixture(string name){string folder=Path.Combine(root,name);Directory.CreateDirectory(folder);string target=Path.Combine(folder,"main");File.WriteAllBytes(target,input);File.WriteAllBytes(Path.Combine(folder,"backup"),input);return target;}
        SAV9ZA Open(string p)=>(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(p).ToArray())!;
        void Reject(string target,PokemonPatch patch,string label)
        {
            string hash=Hash(File.ReadAllBytes(target));bool failed=false;
            try{service.SaveEdits(target,[new TeamEdit(0){Details=patch}],[],hash);}
            catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or IOException){failed=true;}
            Check(failed&&Hash(File.ReadAllBytes(target))==hash,label);
        }
        var path=Fixture("working");
        var before=Open(path);var baseline=service.InspectPokemon(path,true,0);
        Check(baseline.Hash==Hash(File.ReadAllBytes(path))&&baseline.Values.IVs!.Length==6&&baseline.Values.Moves!.Length==4,"Detailed snapshot uses encrypted hash and full typed data");
        Check(baseline.ReadOnly.Unsupported.Count>=5&&baseline.Stats.Length==6&&baseline.Stats[0]>0,"Snapshot includes limits and calculated stats");
        // This probe tests mint/preview behavior; do not change unrelated IV/Hyper Training
        // dependencies of an arbitrary supplied Alpha fixture. IV roundtrips are checked below.
        var preview=service.PreviewPokemon(path,true,0,new(){StatNature=15,EVs=[252,0,0,0,252,4]});
        Check(preview.Values.Nature==baseline.Values.Nature&&preview.Values.StatNature==15&&Hash(File.ReadAllBytes(path))==baseline.Hash,"Preview mint changes only effective nature and does not persist");
        var change=new PokemonPatch
        {
            Species=25,Form=0,Level=70,Gender=0,Nickname="Neon",IsNicknamed=true,Shiny=ShinyStyle.Square,Nature=3,StatNature=15,
            IVs=[0,12,21,4,5,6],EVs=[252,0,0,0,252,4],HyperTrained=[true,false,false,false,false,false],
            Moves=[84,98,86,85],PlusMoves=[84],Ball=4,HeldItem=0,Language=2,Friendship=222,
            MetLevel=10,MetDate=new DateOnly(2026,11,1),MetLocation=2,ObedienceLevel=10,
            OriginalTrainerName="Aurora",OriginalTrainerGender=1,TID16=123,SID16=456,Scale=123,HeightScalar=0,WeightScalar=0,
            IsAlpha=false,Favorite=true,Markings=[0,1,2,0,1,2],
        };
        var result=service.SaveEdits(path,[new TeamEdit(0){Details=change}],[],baseline.Hash);
        var after=Open(path);var details=service.InspectPokemon(path,true,0);var v=details.Values;
        Check(v.Species==25&&v.Form==0&&v.Level==70&&v.Nickname=="Neon"&&v.IsNicknamed==true&&v.Gender==0,"Detailed identity persists without replacing slot");
        Check(v.Nature==3&&v.StatNature==15&&v.Shiny==ShinyStyle.Square,"Original nature and mint remain separate; square shiny persists");
        Check(v.IVs!.SequenceEqual(change.IVs!)&&v.EVs!.SequenceEqual(change.EVs!)&&v.HyperTrained!.SequenceEqual(change.HyperTrained!),"IV/EV/Hyper Training order and flags persist");
        Check(v.Moves!.SequenceEqual(change.Moves!)&&v.PP!.SequenceEqual(v.Moves!.Select(m=>MoveInfo.GetPP(EntityContext.Gen9a,(ushort)m)).Select(p=>(int)p))&&v.PPUps!.All(p=>p==0),"Moves persist with calculated native PP and no PP Ups");
        Check(v.PlusMoves!.Contains(84)&&!new LegalityAnalysis(after.GetPartySlotAtIndex(0)).Results.Any(r=>!r.Valid&&r.Result.ToString().StartsWith("PlusMoveSufficientLevelMissing")),"Required Plus records reconciled after identity/level/move changes");
        Check(v.OriginalTrainerName=="Aurora"&&v.OriginalTrainerGender==1&&v.TID16==123&&v.SID16==456&&v.Friendship==222&&v.Language==2,"Trainer identity and requested friendship persist with handler update");
        Check(v.MetLevel==10&&v.MetLocation==2&&v.MetDate==new DateOnly(2026,11,1)&&v.ObedienceLevel==10,"Met data and obedience persist");
        Check(v.Scale==123&&v.Favorite==true&&v.Markings!.SequenceEqual(change.Markings!),"Scale, favorite and six colored markings persist");
        Check(after.GetPartySlotAtIndex(0).ChecksumValid&&details.ReadOnly.EncryptionConstant==baseline.ReadOnly.EncryptionConstant&&details.ReadOnly.Tracker==baseline.ReadOnly.Tracker,"Checksum valid and read-only encryption/tracker preserved");
        Check(Enumerable.Range(1,5).All(i=>before.GetPartySlotAtIndex(i).Data.SequenceEqual(after.GetPartySlotAtIndex(i).Data))&&Enumerable.Range(0,before.BoxCount*30).All(i=>before.GetBoxSlotAtIndex(i).Data.SequenceEqual(after.GetBoxSlotAtIndex(i).Data)),"All unrelated party and box slots preserved byte for byte");
        var allowed=new HashSet<uint>{0x0D66012C,0x3AA1A9AD,0x2D87BE5C};
        Check(before.AllBlocks.Zip(after.AllBlocks).All(p=>allowed.Contains(p.First.Key)||p.First.Type==p.Second.Type&&p.First.Data.SequenceEqual(p.Second.Data)),"Inventory, progress and unrelated save blocks preserved");
        Check(Hash(File.ReadAllBytes(Path.Combine(result.BackupDirectory,"main")))==baseline.Hash&&Hash(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(path)!,"backup")))==originalHash,"Original backup snapshot and game backup remain intact");
        uint mintedPid=details.ReadOnly.PID;
        service.SaveEdits(path,[new TeamEdit(0){Details=new(){Nickname="Minted"}}],[]);
        var minted=service.InspectPokemon(path,true,0);
        Check(minted.Values.Nature==3&&minted.Values.StatNature==15&&minted.Values.Shiny==ShinyStyle.Square&&minted.ReadOnly.PID==mintedPid,"Nickname-only edit preserves original nature, mint and square shiny PID");
        // Explicit zero, false and empty are edits, not missing values.
        service.SaveEdits(path,[new TeamEdit(0){Details=new(){Nickname="",IsNicknamed=false,Favorite=false,EVs=[0,0,0,0,0,0],Markings=[0,0,0,0,0,0]}}],[]);
        details=service.InspectPokemon(path,true,0);
        Check(details.Values.Nickname=="Pikachu"&&details.Values.IsNicknamed==false&&details.Values.Favorite==false&&details.Values.EVs!.All(x=>x==0)&&details.Values.Markings!.All(x=>x==0),"Empty nickname resets species name; false/zero patches apply");
        var preserve=Open(path).GetPartySlotAtIndex(0);uint priorPid=preserve.PID;
        service.SaveEdits(path,[new TeamEdit(0){Details=new(){Friendship=1}}],[]);
        Check(Open(path).GetPartySlotAtIndex(0).PID==priorPid,"Unrelated detail patch preserves PID exactly");
        service.SaveEdits(path,[new TeamEdit(0){Details=new(){TID16=321,SID16=654}}],[]);
        Check(service.InspectPokemon(path,true,0).Values.Shiny==ShinyStyle.Square,"TID/SID patch preserves shiny style when omitted");
        service.SaveEdits(path,[new TeamEdit(0){Details=new(){Shiny=ShinyStyle.NonShiny}}],[]);
        service.SaveEdits(path,[new TeamEdit(0){Details=new(){TID16=1,SID16=1}}],[]);
        Check(service.InspectPokemon(path,true,0).Values.Shiny==ShinyStyle.NonShiny,"TID/SID patch preserves non-shiny when omitted");
        var formPreview=service.PreviewPokemon(path,true,0,new(){Species=26,Form=1,Level=100,Moves=[84,85,86,98],HyperTrained=[false,false,false,false,false,false]});
        Check(formPreview.Values.Species==26&&formPreview.Values.Form==1&&formPreview.ReadOnly.Ability!=0,"Persistent regional form preview adjusts dependent ability");
        var pending=service.Inspect(path).Hash;bool batchRejected=false;
        try{service.SaveEdits(path,[new TeamEdit(0){Details=new(){Nickname="No write"}}],[new TeamEdit(0){Details=new(){IVs=[99,0,0,0,0,0]}}],pending);}catch(ArgumentException){batchRejected=true;}
        Check(batchRejected&&service.Inspect(path).Hash==pending,"Invalid detailed mixed batch persists no partial edits");
        Reject(path,new(){IVs=[32,0,0,0,0,0]},"Reject invalid IV without mutation");
        Reject(path,new(){EVs=[252,252,252,0,0,0]},"Reject EV total over 510 without mutation");
        Reject(path,new(){HyperTrained=[true]},"Reject incomplete hyper flags");
        Reject(path,new(){Level=49},"Reject level below 50 with existing hyper training");
        string quickHash=service.Inspect(path).Hash;bool quickRejected=false;
        try{service.SaveEdits(path,[new TeamEdit(0,Level:49){Details=new(){Nickname="Nope"}}],[]);}catch(ArgumentException){quickRejected=true;}
        Check(quickRejected&&service.Inspect(path).Hash==quickHash,"Quick-level plus unrelated details cannot bypass hyper training invariant");
        Reject(path,new(){IVs=[31,12,21,4,5,6]},"Reject hyper training on already perfect IV");
        Reject(path,new(){Species=6,Form=1},"Reject persistent battle-only Mega form");
        Reject(path,new(){Species=120,Gender=0},"Reject incompatible species/gender");
        Reject(path,new(){StatNature=0},"Reject neutral nature without a mint");
        Reject(path,new(){Language=6},"Reject unused language");
        Reject(path,new(){MetLocation=65535},"Reject unknown met location");
        Reject(path,new(){Nickname=new string('A',13)},"Reject overlong nickname");
        Reject(path,new(){Nickname="bad\uD800"},"Reject malformed Unicode nickname");
        Reject(path,new(){OriginalTrainerName=""},"Reject empty trainer name");
        Reject(path,new(){TID16=65536},"Reject TID truncation");
        Reject(path,new(){Markings=[3,0,0,0,0,0]},"Reject invalid marking color");
        Reject(path,new(){Moves=[84,84,0,0]},"Reject duplicate moves");
        Reject(path,new(){Moves=[105,0,0,0]},"Reject incompatible moves");
        Reject(path,new(){PlusMoves=[9999]},"Reject unsupported Plus record");
        Reject(path,new(){PP=[0,0,0,0]},"Reject editable PP gameplay claim");
        Reject(path,new(){PPUps=[1,0,0,0]},"Reject PP Ups unavailable in Z-A");
        Reject(path,new(){RelearnMoves=[84,0,0,0]},"Reject rewriting read-only relearn slots");
        Reject(path,new(){AffixedRibbon=3},"Reject affixing an unawarded ribbon");
        Reject(path,new(){AffixedRibbon=130},"Reject ribbon byte overflow");
        Reject(path,new(){HeightScalar=12},"Reject invalid native size dependency");
        Reject(path,new(){IsAlpha=true},"Reject inconsistent Alpha dependencies");
        var boxed=service.InspectBox(path,0).First();
        var boxSnapshot=service.InspectPokemon(path,false,boxed.Slot);
        service.SaveEdits(path,[],[new TeamEdit(boxed.Slot){Details=new(){Nickname="Box Star",Shiny=ShinyStyle.Star,Friendship=199}}]);
        var boxAfter=service.InspectPokemon(path,false,boxed.Slot);
        Check(boxAfter.Values.Nickname=="Box Star"&&boxAfter.Values.Shiny==ShinyStyle.Star&&boxAfter.Values.Friendship==199,"Detailed box edit persists through shared transaction");
        var newOptions=new AddOptions{Species=4,Level=60,Destination=Destination.Box,Details=new(){Nickname="Custom",StatNature=15,IVs=[0,1,2,3,4,5],EVs=[0,0,0,0,252,252],Shiny=ShinyStyle.Star,Friendship=200,Favorite=true}};
        var newPreview=service.PreviewNewPokemon(path,newOptions);
        var addition=service.Add(path,newOptions,service.Inspect(path).Hash);
        var custom=service.InspectPokemon(path,false,addition.Pokemon.Index);
        Check(custom.Values.Nickname=="Custom"&&custom.Values.IVs!.SequenceEqual(newOptions.Details.IVs!)&&custom.Values.EVs!.SequenceEqual(newOptions.Details.EVs!)&&custom.Values.Shiny==ShinyStyle.Star&&custom.Values.Friendship==200&&custom.Values.Favorite==true,"Add uses same detailed custom patch as preview and edit");
        var alphaPatch=new PokemonPatch{IsAlpha=true,Scale=255,HeightScalar=0,WeightScalar=0,IVs=[31,31,31,31,31,31],HyperTrained=[false,false,false,false,false,false]};
        foreach(int alphaSpecies in new[]{4,25,6})
        {
            var alphaPreview=service.PreviewNewPokemon(path,new AddOptions{Species=alphaSpecies,Level=100,Destination=Destination.Box,Details=alphaPatch});
            Check(alphaPreview.Values.IsAlpha==true&&alphaPreview.Values.Scale==255&&alphaPreview.Legal,$"Alpha creation selects compatible encounter for species {alphaSpecies}");
        }
        var threeIvAlpha=service.PreviewNewPokemon(path,new AddOptions{Species=4,Level=60,Destination=Destination.Box,Details=alphaPatch with{IVs=[31,31,31,0,0,0]}});
        Check(threeIvAlpha.Values.IsAlpha==true&&threeIvAlpha.Values.IVs!.Count(x=>x==31)==3&&threeIvAlpha.Legal,"Alpha creation accepts exactly three perfect IVs");
        var alphaAddition=service.Add(path,new AddOptions{Species=4,Level=100,Destination=Destination.Box,Details=alphaPatch},service.Inspect(path).Hash);
        var alphaStored=service.InspectPokemon(path,false,alphaAddition.Pokemon.Index);
        Check(alphaStored.Values.IsAlpha==true&&alphaStored.Values.Scale==255&&alphaStored.Values.IVs!.Count(x=>x==31)>=3&&alphaStored.Legal,"Alpha addition persists valid encounter and size/IV dependencies");
        var alphaEdit=service.PreviewPokemon(path,false,alphaAddition.Pokemon.Index,new(){IsAlpha=true,Nickname="Alpha"});
        Check(alphaEdit.Values.IsAlpha==true&&alphaEdit.ReadOnly.EncryptionConstant==alphaStored.ReadOnly.EncryptionConstant&&alphaEdit.Values.MetLocation==alphaStored.Values.MetLocation,"Existing Alpha edit preserves encounter and identity");
        var nonAlphaAddition=service.Add(path,new AddOptions{Species=2,Level=100,Destination=Destination.Box,Details=new(){IsAlpha=false}},service.Inspect(path).Hash);
        var nonAlphaStored=service.InspectPokemon(path,false,nonAlphaAddition.Pokemon.Index);
        Check(nonAlphaStored.Values.IsAlpha==false&&nonAlphaStored.Legal,"Conversion fixture is an existing legal non-Alpha Ivysaur");
        service.SaveEdits(path,[],[new TeamEdit(nonAlphaAddition.Pokemon.Index){Details=alphaPatch with{IVs=[31,31,31,0,0,0]}}],service.Inspect(path).Hash);
        var convertedAlpha=service.InspectPokemon(path,false,nonAlphaAddition.Pokemon.Index);
        Check(convertedAlpha.Values.IsAlpha==true&&convertedAlpha.Values.Scale==255&&convertedAlpha.Values.IVs!.Count(x=>x==31)==3&&convertedAlpha.Legal,"Existing non-Alpha converts and persists as legal Alpha with three perfect IVs");
        Check(convertedAlpha.ReadOnly.EncryptionConstant==nonAlphaStored.ReadOnly.EncryptionConstant&&convertedAlpha.Values.MetLocation==nonAlphaStored.Values.MetLocation&&convertedAlpha.Values.MetLevel==nonAlphaStored.Values.MetLevel&&convertedAlpha.Values.MetDate==nonAlphaStored.Values.MetDate,"Non-Alpha conversion preserves encryption constant and original encounter");
        foreach(var unavailable in new[]{(Species:4,Level:10),(Species:150,Level:100)})
        {
            string alphaHash=service.Inspect(path).Hash;bool alphaRejected=false;
            try{service.Add(path,new AddOptions{Species=unavailable.Species,Level=unavailable.Level,Destination=Destination.Box,Details=alphaPatch},alphaHash);}
            catch(ArgumentException ex){alphaRejected=ex.Message.Contains("não existe encontro Alpha");}
            Check(alphaRejected&&service.Inspect(path).Hash==alphaHash,$"Unavailable Alpha encounter rejects clearly without mutation: species {unavailable.Species} level {unavailable.Level}");
        }
        int stone=Catalog.HeldItems.First(c=>c.Name.Contains("Charizardite",StringComparison.OrdinalIgnoreCase)).Id;
        service.SaveEdits(path,[new TeamEdit(0){Details=new(){HeldItem=stone}}],[]);
        Check(service.InspectPokemon(path,true,0).Values.HeldItem==stone,"Actual Z-A Mega Stone can be held without persisting a battle form");
        var ribbonPath=Fixture("existing-ribbon");var ribbonSave=Open(ribbonPath);var ribbonPokemon=(PA9)ribbonSave.GetPartySlotAtIndex(0);
        ribbonPokemon.RibbonBestFriends=true;ribbonPokemon.RefreshChecksum();ribbonSave.SetPartySlotAtIndex(ribbonPokemon,0,EntityImportSettings.None);File.WriteAllBytes(ribbonPath,ribbonSave.Write().ToArray());
        service.SaveEdits(ribbonPath,[new TeamEdit(0){Details=new(){AffixedRibbon=(int)RibbonIndex.BestFriends}}],[]);
        Check(service.InspectPokemon(ribbonPath,true,0).Values.AffixedRibbon==(int)RibbonIndex.BestFriends,"Affix an already-present ribbon without granting a new ribbon");
        service.SaveEdits(ribbonPath,[new TeamEdit(0){Details=new(){AffixedRibbon=-1}}],[]);
        Check(service.InspectPokemon(ribbonPath,true,0).Values.AffixedRibbon==-1,"Explicit -1 removes the displayed ribbon only");
        Check(Catalog.HeldItems.Any(c=>c.Name.Contains("Charizardite",StringComparison.OrdinalIgnoreCase))&&Catalog.HeldItems.Any(c=>c.Id==0),"Held item catalog includes Mega Stones and empty item");
        Check(Catalog.Languages.All(c=>c.Id!=6)&&Catalog.MetLocations.Any(c=>c.Id==30027)&&Catalog.BaseStats(25,0).Length==6,"Language, met location and base-stat catalogs validated");
        Check(Hash(File.ReadAllBytes(source))==originalHash,"Synthetic source file never modified");
        Console.WriteLine($"SUCCESS {passed} detailed editor checks; copies: {root}");
    }
    private static string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes));
}
