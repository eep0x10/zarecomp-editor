using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PKHeX.Core;
using ZaTool.Engine;
using ZaTool.WebHost;

internal static class OptimizeTests
{
    internal static void Run(string source)=>RunAsync(source).GetAwaiter().GetResult();
    private static async Task RunAsync(string source)
    {
        string root=Path.Combine(AppContext.BaseDirectory,"data","optimize-"+Guid.NewGuid().ToString("N"));
        string folder=Path.Combine(root,"save_0");Directory.CreateDirectory(folder);string path=Path.Combine(folder,"main");
        var blank=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(source).ToArray())!;blank.PartyInfo.PartyCount=0;File.WriteAllBytes(path,blank.Write().ToArray());
        var service=new SaveService();int passed=0;
        void Check(bool value,string label){if(!value)throw new Exception(label);passed++;Console.WriteLine("PASS Optimize "+label);}
        SAV9ZA Open()=>(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(path).ToArray())!;
        TeamEdit[] Edits(TeamOptimizePlan plan)=>JsonSerializer.SerializeToElement(plan.TeamEdits,WorkbenchHost.Json).Deserialize<TeamEdit[]>(WorkbenchHost.Json)!;
        void Apply(TeamOptimizePlan plan)
        {
            if(!plan.Available)throw new Exception("Cannot apply unavailable optimization.");
            var edits=Edits(plan).Where(e=>e.Details!=null&&typeof(PokemonPatch).GetProperties().Any(p=>p.GetValue(e.Details)!=null)).ToArray();
            if(edits.Length>0)service.SaveEdits(path,edits,[],plan.Hash);
        }
        // Use forms explicitly present in this Z-A policy. Slot 3 is a second Charizard
        // so the stored Modest Mint (15) is the canonical recommendation for reversion.
        int[] species=[6,25,150,6,4,678];
        foreach(var entry in species.Select((id,slot)=>(id,slot)))
        {
            service.Add(path,new AddOptions{Species=entry.id,Form=entry.id==678?1:0,Level=entry.slot==3?100:60,Nature=3,
                Gender=entry.id==678?1:-1,Destination=Destination.Party,CreationSeed=(ulong)(entry.slot+20),
                Details=new(){IsAlpha=false,Nickname="Opt"+entry.slot,Friendship=0,IVs=entry.slot==1?[0,31,31,0,0,31]:[0,1,2,3,4,5],
                    StatNature=entry.slot==3?15:3,Shiny=ShinyStyle.NonShiny,EVs=[1,2,3,4,5,6]}});
        }
        var prepared=Open();var home=(PA9)prepared.GetPartySlotAtIndex(1);home.Tracker=1234;home.HeightScalar=home.WeightScalar=home.Scale;
        uint shinyLow=home.PID&0xFFFF;home.PID=shinyLow|((shinyLow^home.TID16^home.SID16)<<16);
        home.CurrentHandler=1;home.OriginalTrainerFriendship=77;home.HandlingTrainerFriendship=23;
        home.HT_HP=home.HT_SPE=home.HT_SPA=true;home.RefreshChecksum();prepared.SetPartySlotAtIndex(home,1,EntityImportSettings.None);
        var first=(PA9)prepared.GetPartySlotAtIndex(0);first.Stat_HPCurrent=1;first.Status_Condition=8;first.RefreshChecksum();prepared.SetPartySlotAtIndex(first,0,EntityImportSettings.None);
        File.WriteAllBytes(path,prepared.Write().ToArray());
        var before=Enumerable.Range(0,6).Select(i=>service.InspectPokemon(path,true,i)).ToArray();string hash=service.Inspect(path).Hash;
        var sample=service.InspectTeam(path)[0];int expectedNature=NatureAdvisor.Suggest(sample).Nature;
        foreach(string locale in new[]{"pt-BR","en","es"})foreach(int nature in new[]{0,3,10,15,24})
        {
            var forged=sample with{Nature=nature,Level=nature+1,BaseStats=[999,1,999,1,1,999],Moves=nature%2==0?[33,33,33,33]:[45,45,45,45]};
            Check(NatureAdvisor.Suggest(forged,locale).Nature==expectedNature,$"canonical nature ignores current state/client stats/moves and locale {locale}/{nature}");
        }
        bool badSpecies=false;try{NatureAdvisor.Suggest(sample with{Species=int.MaxValue});}catch(ArgumentException){badSpecies=true;}
        Check(badSpecies,"invalid species recommendation is a bounded domain error");
        var mint=service.PlanTeamOptimize(path,[],new(BestNatures:true),AlphaMode.Compatible);
        Check(mint.Available&&mint.TeamEdits.Count==6&&mint.Items.All(i=>i.Before!.Values.Nature==i.Preview!.Values.Nature&&i.Before.ReadOnly.PID==i.Preview.ReadOnly.PID),"nature suggestions use Mint only and preserve original nature/PID");
        var reverted=service.PlanTeamOptimize(path,[new TeamEdit(3){Details=new(){Level=50,StatNature=10,Nickname="KeepDraft"}}],new(BestNatures:true,Level100:true),AlphaMode.Compatible);
        var canonical=reverted.TeamEdits.Single(e=>e.Slot==3).Details;
        Check(reverted.Available&&!canonical.ContainsKey("level")&&!canonical.ContainsKey("statNature")&&(string)canonical["nickname"]=="KeepDraft","canonical replacement removes reverted level/Mint drafts but retains nickname");
        Check(reverted.Items[3].Before!.Values.Level==50&&reverted.Items[3].Preview!.Values.Level==100,"diff compares effective draft before with optimized after");
        var unchanged=service.PlanTeamOptimize(path,[],new(),AlphaMode.Compatible);
        Check(unchanged.TeamEdits.Count==6&&unchanged.TeamEdits.All(e=>e.Details.Count==0),"unchanged plans include every covered slot with empty sparse dictionary");
        var role=service.PlanTeamOptimize(path,[],new(BestNatures:true,Alpha:true),AlphaMode.Custom);
        var useful=NatureAdvisor.UsefulIVPriority(6,0,expectedNature).Take(3).ToArray();var roleIVs=role.Items[0].Preview!.Values.IVs!;
        Check(role.Available&&useful.All(i=>roleIVs[i]==31)&&Enumerable.Range(0,6).Except(useful).All(i=>roleIVs[i]==before[0].Values.IVs![i]),"Alpha promotes useful role/Mint three IVs without zeroing other stats");
        var pikaRole=role.Items[1].Preview!.Values.IVs!;
        Check(pikaRole.Count(v=>v==31)>3&&new[]{1,2,5}.All(i=>pikaRole[i]==31),"useful promotions preserve existing perfect IVs even when three were already perfect");
        var incompatible=service.PlanTeamOptimize(path,[],new(Alpha:true,Level100:true),AlphaMode.Compatible);
        Check(!incompatible.Available&&incompatible.TeamEdits.Count==0&&incompatible.Items.Any(i=>!i.Available&&i.Species==150),"unsupported Compatible Alpha makes entire party batch unavailable");
        var invalid=service.PlanTeamOptimize(path,[new TeamEdit(0){Details=new(){IVs=[99,0,0,0,0,0]}}],new(MaxFriendship:true),AlphaMode.Custom);
        Check(!invalid.Available&&invalid.TeamEdits.Count==0&&invalid.Items.Any(i=>i.Available),"one invalid draft prevents partial apply despite other successful previews");
        var allOptions=new TeamOptimizeOptions(true,true,true,true,true,true,true);
        var all=service.PlanTeamOptimize(path,[new TeamEdit(0){Details=new(){Nickname="Preserved",EVs=[2,3,4,5,6,7]}}],allOptions,AlphaMode.Custom);
        Check(all.Available&&all.TeamEdits.Count==6,"all seven options produce one complete Custom party plan");
        Check(all.Items.All(i=>i.Preview!.Values.IsAlpha==true&&i.Preview.Values.Level==100&&i.Preview.Values.Friendship==255&&i.Preview.Values.Scale==255&&i.Preview.Values.IVs!.All(v=>v==31)),"Alpha/level/friendship/size/max-IV flags apply to every member");
        Check(all.Items.All(i=>i.Preview!.Values.HyperTrained!.All(v=>!v)),"perfect IVs clear incompatible Hyper Training flags");
        Check(all.Items[1].Preview!.Values.Shiny==ShinyStyle.Square&&all.Items.Where(i=>i.Slot!=1).All(i=>i.Preview!.Values.Shiny==ShinyStyle.Star),"shiny option preserves Square and only changes non-shiny to Star");
        Check(all.Items[1].Preview!.Values.HeightScalar==255&&all.Items[1].Preview!.Values.WeightScalar==255&&all.Items.Where(i=>i.Slot!=1).All(i=>i.Preview!.Values.HeightScalar==0&&i.Preview.Values.WeightScalar==0),"HOME/native size dependencies remain distinct");
        Check(all.Items[0].Preview!.Values.Nickname=="Preserved"&&all.Items[0].Preview!.Values.EVs!.SequenceEqual(new[]{2,3,4,5,6,7}),"existing nickname and EV drafts remain in optimized result");
        Check(all.Items[2].Preview!.Legal==false&&all.Items[2].Warnings.Count>0,"Custom unsupported Alpha keeps actual legality report and warning");
        var serialized=JsonSerializer.SerializeToElement(all,WorkbenchHost.Json);
        Check(serialized.GetProperty("teamEdits").EnumerateArray().All(e=>e.GetProperty("details").EnumerateObject().All(p=>p.Value.ValueKind!=JsonValueKind.Null)),"canonical team edits never serialize nullable patch placeholders");
        var repeated=service.PlanTeamOptimize(path,[new TeamEdit(0){Details=new(){Nickname="Preserved",EVs=[2,3,4,5,6,7]}}],allOptions,AlphaMode.Custom);
        Check(JsonSerializer.Serialize(all.TeamEdits,WorkbenchHost.Json)==JsonSerializer.Serialize(repeated.TeamEdits,WorkbenchHost.Json),"repeated planning returns stable canonical drafts");
        Check(service.Inspect(path).Hash==hash,"all successful and failed plans leave save hash unchanged");
        Apply(all);var after=Enumerable.Range(0,6).Select(i=>service.InspectPokemon(path,true,i)).ToArray();
        Check(after.All(p=>p.Values.IsAlpha==true&&p.Values.IVs!.All(v=>v==31))&&after[0].Values.Nickname=="Preserved","accepted canonical party drafts persist through existing SaveEdits transaction");
        var persisted=(PA9)Open().GetPartySlotAtIndex(0);
        Check(persisted.Stat_HPCurrent==1&&persisted.Status_Condition==8,"optimizing existing team does not heal HP or clear status");
        var traded=(PA9)Open().GetPartySlotAtIndex(1);
        Check(traded.CurrentHandler==1&&traded.HandlingTrainerFriendship==255&&traded.OriginalTrainerFriendship==77,"friendship maximum targets current handling trainer and preserves OT friendship");
        var noop=service.PlanTeamOptimize(path,[],allOptions,AlphaMode.Custom);
        Check(noop.Available&&noop.TeamEdits.Count==6&&noop.TeamEdits.All(e=>e.Details.Count==0)&&noop.Items.All(i=>i.Changes.Count==0),"planning again after apply is idempotent and returns empty replacement entries");
        string appliedHash=service.Inspect(path).Hash;Apply(noop);Check(service.Inspect(path).Hash==appliedHash,"applying a no-op plan does not write");
        service.SaveEdits(path,[new TeamEdit(2){Details=new(){AlphaMode=AlphaMode.Custom,IVs=[31,31,31,3,4,5]}}],[],appliedHash);
        var existingCustom=service.PlanTeamOptimize(path,[],new(BestNatures:true,MaxIVs:true),AlphaMode.Compatible);
        Check(existingCustom.Available&&existingCustom.TeamEdits[2].Details.TryGetValue("alphaMode",out var retainedMode)&&Equals(retainedMode,AlphaMode.Custom),"already Custom Alpha retains required policy even when Alpha optimization is disabled");
        Apply(existingCustom);var customAgain=service.InspectPokemon(path,true,2);
        Check(customAgain.Values.IsAlpha==true&&customAgain.Values.IVs!.All(v=>v==31)&&!customAgain.Legal,"canonical already-Custom draft roundtrips without weakening legality report");
        appliedHash=service.Inspect(path).Hash;

        await using(var host=new WorkbenchHost(new HostOptions{DataDirectory=root,InitialPath=path}))
        {
            await host.StartAsync();using var client=new HttpClient{BaseAddress=new Uri(host.Address)};
            client.DefaultRequestHeaders.Authorization=new("Bearer",host.BrowserUrl.Split("#token=")[1]);client.DefaultRequestHeaders.Add("Origin",host.Address);
            async Task<HttpResponseMessage> Post(string route,object body)=>await client.PostAsJsonAsync(route,body,WorkbenchHost.Json);
            var opened=await Post("/api/v1/sessions",new{path});opened.EnsureSuccessStatusCode();var session=(await opened.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("id").GetString();
            var response=await Post($"/api/v1/sessions/{session}/team-optimize-plan",new{expectedHash=appliedHash,teamEdits=Array.Empty<object>(),options=allOptions,alphaMode="Custom",language="es"});
            response.EnsureSuccessStatusCode();var wire=await response.Content.ReadFromJsonAsync<JsonElement>();
            Check(wire.GetProperty("available").GetBoolean()&&wire.GetProperty("teamEdits").GetArrayLength()==6&&wire.GetProperty("items").GetArrayLength()==6,"HTTP endpoint exposes complete sparse per-slot contract");
            Check((await Post($"/api/v1/sessions/{session}/team-optimize-plan",new{expectedHash="STALE",teamEdits=Array.Empty<object>(),options=allOptions,alphaMode="Custom"})).StatusCode==HttpStatusCode.Conflict,"HTTP optimization rejects stale hash");
            var forged=sample with{Species=int.MaxValue};
            Check((await Post($"/api/v1/sessions/{session}/suggestions",new{expectedHash=appliedHash,team=new[]{forged},language="en"})).StatusCode==HttpStatusCode.BadRequest,"public nature endpoint rejects unknown species instead of indexing arbitrary client data");
        }
        Check(service.Inspect(path).Hash==appliedHash,"HTTP planning and failures never write the save");
        Console.WriteLine($"SUCCESS {passed} deterministic team optimization Engine/API checks on generated copies.");
    }
}
