using PKHeX.Core;
using ZaTool.Engine;

internal static class AlphaTests
{
    internal static void Run(string source)
    {
        string folder=Path.Combine(Path.GetDirectoryName(source)!,"alpha-regression");Directory.CreateDirectory(folder);
        string path=Path.Combine(folder,"main");File.Copy(source,path);
        var service=new SaveService();int passed=0;
        void Check(bool value,string label){if(!value)throw new Exception(label);passed++;Console.WriteLine("PASS Alpha "+label);}
        SAV9ZA Open()=>(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(path))!;
        string hash=service.Inspect(path).Hash;
        var low=service.PlanNewAlpha(path,new AddOptions{Species=4,Level=10,Destination=Destination.Box},AlphaMode.Compatible);
        Check(low.Available&&low.MinimumLevel>10&&low.Options!.Level>=low.MinimumLevel&&low.Preview!.Values.IsAlpha==true,"low-level addition offers actual minimum and applicable preview");
        Check(service.Inspect(path).Hash==hash,"planning never writes");
        Check(low.Options!.CreationSeed!=null&&low.Options.Details!.MetDate==low.Preview!.Values.MetDate,"creation plan materializes seed and date");
        var repeated=service.PreviewNewPokemon(path,low.Options);
        Check(repeated.ReadOnly.PID==low.Preview!.ReadOnly.PID&&repeated.ReadOnly.EncryptionConstant==low.Preview.ReadOnly.EncryptionConstant,"seed repeats preview identity");
        var maker=typeof(SaveService).GetMethod("BuildNewPokemon",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
        var save=Open();var expected=(PA9)maker.Invoke(null,[save,low.Options])!;
        Check(Enumerable.Range(0,12).All(_=>expected.Data.SequenceEqual(((PA9)maker.Invoke(null,[save,low.Options])!).Data)),"seeded Alpha full bytes stable across twelve generations, including HP");
        var added=service.Add(path,low.Options,hash);var actual=(PA9)Open().GetBoxSlotAtIndex(added.Pokemon.Index);
        if(!expected.Data.SequenceEqual(actual.Data))Console.WriteLine("BYTE DIFF "+string.Join(",",Enumerable.Range(0,Math.Min(expected.Data.Length,actual.Data.Length)).Where(i=>expected.Data[i]!=actual.Data[i]).Take(25).Select(i=>$"{i:X}:{expected.Data[i]:X2}/{actual.Data[i]:X2}"))+$" lengths {expected.Data.Length}/{actual.Data.Length}");
        Check(expected.Data.SequenceEqual(actual.Data),"planned seeded creation equals persisted Pokémon byte-for-byte");
        Check(actual.Stat_HPCurrent==actual.Stat_HPMax&&actual.Status_Condition==0,"new customized Pokémon start healthy after final IV application");
        Check(File.Exists(Path.Combine(added.BackupDirectory,"main")),"planned Add keeps automatic backup");
        hash=service.Inspect(path).Hash;bool invalid=false;
        try{service.SaveEdits(path,[],[new TeamEdit(added.Pokemon.Index){Details=new(){IVs=[0,0,0,0,0,0]}}],hash);}catch(ArgumentException){invalid=true;}
        Check(invalid&&service.Inspect(path).Hash==hash,"editing only IVs cannot invalidate Alpha three-perfect-IV invariant");
        var unavailable=service.PlanNewAlpha(path,new AddOptions{Species=150,Level=100},AlphaMode.Compatible);
        Check(!unavailable.Available&&unavailable.Code=="alpha_encounter_unavailable","restricted species reports no compatible encounter without fabricated legality");
        var custom=service.PlanNewAlpha(path,new AddOptions{Species=150,Level=1,Destination=Destination.Box,Details=new(){Shiny=ShinyStyle.Square,EVs=[0,0,0,0,0,0]}},AlphaMode.Custom);
        Check(custom.Available&&custom.Preview!.Values.IsAlpha==true&&!custom.Preview.Legal&&custom.Options!.Level==1,"custom Alpha supports no-template low level with honest illegal report");
        var customResult=service.Add(path,custom.Options!,service.Inspect(path).Hash);
        var stored=service.InspectPokemon(path,false,customResult.Pokemon.Index);
        Check(stored.Values.IsAlpha==true&&stored.Values.Shiny==ShinyStyle.Square&&stored.Values.IVs!.Count(x=>x==31)>=3&&stored.Values.EVs!.All(v=>v==0)&&!stored.Legal,"custom flag, format dependencies, square shiny and trained/reset EVs roundtrip");
        var modeOnly=service.PreviewPokemon(path,false,added.Pokemon.Index,new(){AlphaMode=AlphaMode.Custom});
        Check(modeOnly.Values.IsAlpha==true,"existing Alpha accepts explicit mode switch without repeated flag");
        bool badOrigin=false;try{service.PreviewPokemon(path,false,added.Pokemon.Index,new(){MetLocation=30026,MetLevel=5});}catch(ArgumentException){badOrigin=true;}
        Check(badOrigin,"compatible Alpha cannot silently change to incompatible gift origin");
        var customOrigin=service.PreviewPokemon(path,false,added.Pokemon.Index,new(){AlphaMode=AlphaMode.Custom,MetLocation=30026,MetLevel=5});
        Check(customOrigin.Values.IsAlpha==true&&!customOrigin.Legal,"explicit custom origin change retains honest illegal report");
        foreach(int evolved in new[]{2,5,6,154})
        {
            var evolvedPlan=service.PlanNewAlpha(path,new AddOptions{Species=evolved,Level=5,Destination=Destination.Box},AlphaMode.Compatible);
            Check(evolvedPlan.Available&&evolvedPlan.Preview!.Values.Species==evolved&&evolvedPlan.Preview.Values.IsAlpha==true,$"evolved species {evolved} can use real Alpha origin/pre-evolution");
            var evolvedNative=(PA9)maker.Invoke(null,[Open(),evolvedPlan.Options!])!;
            int requiredLevel=evolved==6?36:evolved==154?32:16;
            Check(evolvedPlan.Preview!.Values.Level>=requiredLevel&&!new LegalityAnalysis(evolvedNative).Results.Any(r=>!r.Valid&&r.Result==LegalityCheckResultCode.StatAlphaInvalid),$"evolved species {evolved} meets evolution level and Alpha-origin validation");
        }
        var encountersMethod=typeof(SaveService).GetMethod("AlphaEncounters",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic)!;
        Choice? ancestorOnly=null;
        foreach(var species in Catalog.Species)
        {
            var encounters=((IEnumerable<IEncounterable>)encountersMethod.Invoke(null,[new PA9{Species=(ushort)species.Id,Form=0}])!).ToArray();
            if(encounters.Length>0&&encounters.All(e=>e.Species!=species.Id)){ancestorOnly=species;break;}
        }
        if(ancestorOnly!=null)
        {
            var ancestorPlan=service.PlanNewAlpha(path,new AddOptions{Species=ancestorOnly.Id,Level=100,Destination=Destination.Box},AlphaMode.Compatible);
            Check(ancestorPlan.Available&&ancestorPlan.Preview!.Values.Species==ancestorOnly.Id,$"ancestor-only Alpha encounter can create evolved species {ancestorOnly.Id} {ancestorOnly.Name}");
        }
        else Console.WriteLine("SKIP ancestor-only species: all current Alpha lineages have a direct final-species Alpha encounter.");
        var overrides=service.PlanNewAlpha(path,new AddOptions{Species=678,Form=1,Level=100,Nature=15,Gender=-1,Details=new(){Nature=3,Gender=1,Shiny=ShinyStyle.NonShiny}},AlphaMode.Compatible);
        Check(overrides.Available&&overrides.Preview!.Values.Gender==1&&overrides.Preview.Values.Nature==3,"detailed gender/nature overrides drive encounter selection and generation");
        for(ulong seed=0;seed<12;seed++)
        {
            var starterPreview=service.PreviewNewPokemon(path,new AddOptions{Species=152,Level=5,Nature=0,Nickname="StarterDemo",CreationSeed=seed,Destination=Destination.Box});
            Check(starterPreview.Legal,$"gift seed {seed} preserves generated ability-slot RNG correlation");
            var autoGender=service.PlanNewAlpha(path,new AddOptions{Species=475,Level=1,Gender=-1,CreationSeed=seed},AlphaMode.Compatible);
            Check(autoGender.Available&&autoGender.Preview!.Values.Gender==0,$"ancestor-derived Gallade seed {seed} automatic gender respects male-only final species");
        }
        save=Open();var blank=new PA9{Species=152};
        var chain=new[]{new EvoCriteria{Species=152,Form=0,LevelMin=1,LevelMax=100}};
        var gift=EncounterGenerator9a.Instance.GetPossible(blank,chain,GameVersion.ZA,EncounterTypeGroup.Static).OfType<EncounterGift9a>().First(e=>e.Species==152);
        var lowGift=gift.ConvertToPKM(save);lowGift.ResetPartyStats();lowGift.RefreshChecksum();save.SetPartySlotAtIndex(lowGift,0,EntityImportSettings.None);File.WriteAllBytes(path,save.Write().ToArray());
        var giftPlan=service.PlanAlpha(path,true,0,new(),AlphaMode.Compatible);
        Check(giftPlan.Available&&giftPlan.Preview!.Legal&&giftPlan.Changes.Any(c=>c.Field=="moves"),"level-5 starter proposes required encounter moves and a legal Compatible preview");
        service.SaveEdits(path,[new TeamEdit(0){Details=giftPlan.Patch}],[],service.Inspect(path).Hash);
        Check(service.InspectPokemon(path,true,0).Legal,"accepted low-level starter move/origin adjustments remain legal after save");
        var starter=gift.ConvertToPKM(save);starter.CurrentLevel=100;starter.ResetPartyStats();starter.Stat_HPCurrent=1;starter.Status_Condition=8;starter.RefreshChecksum();
        save.SetPartySlotAtIndex(starter,0,EntityImportSettings.None);File.WriteAllBytes(path,save.Write().ToArray());
        var before=service.InspectPokemon(path,true,0);
        var conversion=service.PlanAlpha(path,true,0,new(){Nickname="Planned",IVs=[0,1,2,3,4,5],HyperTrained=[true,true,true,true,true,true]},AlphaMode.Compatible);
        Check(conversion.Available&&conversion.RequiresOriginChange&&conversion.Changes.Any(c=>c.Field=="metLocation"),"starter gift conversion proposes explicit actual Alpha origin");
        Check(conversion.Preview!.ReadOnly.PID==before.ReadOnly.PID&&conversion.Preview.ReadOnly.EncryptionConstant==before.ReadOnly.EncryptionConstant,"compatible origin conversion preserves PID and EC");
        var backup=service.SaveEdits(path,[new TeamEdit(0){Details=conversion.Patch}],[],service.Inspect(path).Hash);
        var converted=service.InspectPokemon(path,true,0);actual=(PA9)Open().GetPartySlotAtIndex(0);
        Check(converted.Values.IsAlpha==true&&actual.Stat_HPCurrent==1&&actual.Status_Condition==8&&converted.Values.Nickname=="Planned","starter plan persists without healing HP/status or losing other drafts");
        Check(converted.Values.HyperTrained!.Zip(converted.Values.IVs!).All(p=>p.Second!=31||!p.First),"promoted perfect IVs clear only incompatible Hyper Training");
        var boxedBefore=service.InspectPokemon(path,false,customResult.Pokemon.Index);
        bool batchRejected=false;hash=service.Inspect(path).Hash;
        try{service.SaveEdits(path,[new TeamEdit(0){Details=new(){Nickname="NoWrite"}}],[new TeamEdit(customResult.Pokemon.Index){Details=new(){IVs=[0,0,0,0,0,0]}}],hash);}catch(ArgumentException){batchRejected=true;}
        Check(batchRejected&&service.Inspect(path).Hash==hash,"invalid mixed Alpha batch rejects atomically");
        var off=service.PreviewPokemon(path,true,0,new(){IsAlpha=false});
        Check(off.Values.IsAlpha==false&&off.Values.Scale==255,"turning Alpha off does not destroy chosen IVs/size");
        var genderPlan=service.PlanNewAlpha(path,new AddOptions{Species=678,Form=1,Gender=1,Level=100,Destination=Destination.Box},AlphaMode.Custom);
        Check(genderPlan.Available&&genderPlan.Preview!.Values.Gender==1&&genderPlan.Preview.Values.Form==1,"form-specific female gender survives custom Alpha plan");
        // Uncertain commits keep their recovery receipt and never replay the write delegate.
        var store=new ZaTool.WebHost.WorkspaceStore(folder);var opened=System.Text.Json.JsonSerializer.SerializeToElement(store.Open(new(path)),ZaTool.WebHost.WorkbenchHost.Json);
        var session=store.Get(opened.GetProperty("id").GetString()!);int writes=0;string operation=Guid.NewGuid().ToString();var request=new{probe=true};
        object Fail(){writes++;throw new SaveCommitException(backup.BackupDirectory,"final_verification",true,new IOException("injected"));}
        var receipt=System.Text.Json.JsonSerializer.SerializeToElement(store.Commit(session,service.Inspect(path).Hash,operation,request,Fail),ZaTool.WebHost.WorkbenchHost.Json);
        store.Commit(session,service.Inspect(path).Hash,operation,request,Fail);
        Check(receipt.GetProperty("backupDirectory").GetString()==backup.BackupDirectory&&receipt.GetProperty("outcomeUnknown").GetBoolean()&&writes==1,"uncertain commit exposes known backup/phase and cached no-retry receipt");
        string lockedHash=service.Inspect(path).Hash;FileStream? locked=null;
        try
        {
            object LockedFailure(){locked=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.None);throw new SaveCommitException(backup.BackupDirectory,"final_verification",true,new IOException("injected unreadable main"));}
            var unreadable=System.Text.Json.JsonSerializer.SerializeToElement(store.Commit(session,lockedHash,Guid.NewGuid().ToString(),new{lockedProbe=true},LockedFailure),ZaTool.WebHost.WorkbenchHost.Json);
            Check(unreadable.GetProperty("hash").ValueKind==System.Text.Json.JsonValueKind.Null&&unreadable.GetProperty("backupDirectory").GetString()==backup.BackupDirectory,"unreadable main cannot discard the recovery receipt");
        }
        finally{locked?.Dispose();}
        string link=Path.Combine(folder,"recursive-junction");
        var junction=new System.Diagnostics.ProcessStartInfo("cmd.exe"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        junction.ArgumentList.Add("/c");junction.ArgumentList.Add("mklink");junction.ArgumentList.Add("/J");junction.ArgumentList.Add(link);junction.ArgumentList.Add(folder);
        using(var process=System.Diagnostics.Process.Start(junction)!)
        {
            process.StandardOutput.ReadToEnd();process.StandardError.ReadToEnd();process.WaitForExit();
            if(process.ExitCode!=0)throw new Exception("Could not prepare local junction regression fixture.");
        }
        try
        {
            var timer=System.Diagnostics.Stopwatch.StartNew();bool rejected=false;hash=service.Inspect(path).Hash;
            try{service.Add(path,new AddOptions(),hash);}catch(IOException error){rejected=error.Message.Contains("links/junções");}
            Check(rejected&&timer.Elapsed<TimeSpan.FromSeconds(3)&&service.Inspect(path).Hash==hash,"backup rejects a recursive junction before descending and never writes main");
        }
        finally{if((File.GetAttributes(link)&FileAttributes.ReparsePoint)!=0)Directory.Delete(link);}
        Console.WriteLine($"SUCCESS {passed} deterministic Alpha regression checks on generated save copies.");
    }
    internal static void Probe(string source)
    {
        var service = new SaveService();
        var dependencies = new PokemonPatch { IsAlpha = true, Scale = 255, HeightScalar = 0, WeightScalar = 0, IVs = [31,31,31,0,0,0], HyperTrained = [false,false,false,false,false,false] };
        try { service.PreviewNewPokemon(source, new AddOptions {Species=4,Level=10,Details=dependencies}); Console.WriteLine("UNEXPECTED low-level Add preview succeeded."); }
        catch (ArgumentException e) { Console.WriteLine("REPRO low-level Add: " + e.Message); }
        var save = (SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(source))!;
        var blank = new PA9 {Species=152};
        var chain = new[] {new EvoCriteria {Species=152,Form=0,LevelMin=1,LevelMax=100}};
        var gift = EncounterGenerator9a.Instance.GetPossible(blank,chain,GameVersion.ZA,EncounterTypeGroup.Static).OfType<EncounterGift9a>().First(e=>e.Species==152);
        var pk = gift.ConvertToPKM(save); pk.CurrentLevel=100; pk.RefreshChecksum();
        save.SetPartySlotAtIndex(pk,0,EntityImportSettings.None);
        string path = Path.Combine(Path.GetDirectoryName(source)!,"probe","main"); Directory.CreateDirectory(Path.GetDirectoryName(path)!); File.WriteAllBytes(path,save.Write().ToArray());
        Console.WriteLine($"REPRO starter baseline legal={service.InspectPokemon(path,true,0).Legal}, met={pk.MetLocation}/{pk.MetLevel}");
        try { service.PreviewPokemon(path,true,0,dependencies); Console.WriteLine("UNEXPECTED starter toggle preview succeeded."); }
        catch (ArgumentException e) { Console.WriteLine("REPRO starter flag-only conversion: " + e.Message); }
        for(int i=0;i<10;i++)
        {
            var plan=service.PlanNewAlpha(source,new AddOptions{Species=475,Level=100,Gender=-1},AlphaMode.Compatible);
            if(!plan.Available){Console.WriteLine("SKIP Gallade auto-gender probe: "+plan.Code);break;}
            if(plan.Preview!.Values.Gender!=0)throw new Exception("Gallade auto-gender Alpha plan generated female target.");
            Console.WriteLine($"PASS Gallade auto-gender Alpha plan {i+1}/10 male");
        }
        for(ulong seed=0;seed<12;seed++)
        {
            var recipe=new AddOptions{Species=152,Level=5,Nature=0,Nickname="StarterDemo",CreationSeed=seed,Destination=Destination.Box};
            var preview=service.PreviewNewPokemon(source,recipe);
            Console.WriteLine($"STARTER seed={seed} legal={preview.Legal} abilitySlot={preview.ReadOnly.AbilitySlot} met={preview.Values.MetLocation}/{preview.Values.MetLevel}");
            var auto=service.PlanNewAlpha(source,new AddOptions{Species=475,Level=1,Gender=-1,CreationSeed=seed},AlphaMode.Compatible);
            var male=service.PlanNewAlpha(source,new AddOptions{Species=475,Level=1,Gender=0,CreationSeed=seed},AlphaMode.Compatible);
            Console.WriteLine($"GALLADE low seed={seed} auto={auto.Available}/{auto.Code}/{auto.Preview?.Values.Gender} explicitMale={male.Available}/{male.Preview?.Values.Gender} proposedLevel={auto.Options?.Level??male.Options?.Level}");
        }
    }
}
