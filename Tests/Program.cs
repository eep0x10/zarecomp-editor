using PKHeX.Core;
using ZaTool.Engine;
using System.Security.Cryptography;
using System.Text.Json;

if (args.Length > 1 && args[0] == "--details")
{
    DetailedTests.Run(args[1]);return;
}
if (args.Length > 0 && args[0] == "--race")
{
    try { new SaveService().Add(args[1],new AddOptions(),args[2]); Environment.Exit(0); }
    catch (IOException) { Environment.Exit(2); }
}
if (args.Length > 1 && args[0] == "--plus")
{
    var snapshot=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(args[1]).ToArray())!;
    foreach(int boxIndex in new[]{1,2}) {var snapshotSlot=snapshot.GetBoxSlotAtIndex(boxIndex);Console.WriteLine($"Box global slot {boxIndex}: species={snapshotSlot.Species}, form={snapshotSlot.Form}, name={SpeciesName.GetSpeciesNameGeneration(snapshotSlot.Species,2,9)}");}
    var maker=typeof(SaveService).GetMethod("CreatePokemon",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
    var pikachu=(PA9)maker.Invoke(null,[snapshot,new AddOptions {Species=25,Level=10,Nature=15,Shiny=true}])!;
    var analysis=new LegalityAnalysis(pikachu);
    var plusIssues=analysis.Results.Where(r=>!r.Valid && r.Result.ToString().StartsWith("PlusMove")).ToArray();
    Console.WriteLine($"Pikachu lv10 shiny Modest: metLevel={pikachu.MetLevel}, plusIssues={plusIssues.Length}, overallLegal={analysis.Valid}");
    foreach(var issue in plusIssues)Console.WriteLine(issue.Result+" move="+issue.Argument);
    if (plusIssues.Length != 0) Environment.Exit(3);
    var plusFolder=Path.Combine(AppContext.BaseDirectory,"data","plus-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(plusFolder);
    var plusPath=Path.Combine(plusFolder,"main");File.WriteAllBytes(plusPath,snapshot.Write().ToArray());
    var plusResult=new SaveService().Add(plusPath,new AddOptions {Species=25,Level=10,Nature=15,Shiny=true,Destination=Destination.Box});
    var plusSaved=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(plusPath).ToArray())!;
    var plusPokemon=plusSaved.GetBoxSlotAtIndex(plusResult.Pokemon.Index);
    var savedPlusIssues=new LegalityAnalysis(plusPokemon).Results.Count(r=>!r.Valid&&r.Result.ToString().StartsWith("PlusMove"));
    if(savedPlusIssues!=0 || plusPokemon.Species!=25 || plusPokemon.CurrentLevel!=10 || !plusPokemon.IsShiny || (int)plusPokemon.Nature!=15)throw new Exception("Pikachu persisted flags or fields mismatch");
    Console.WriteLine("SUCCESS Pikachu10 shinyModest: zero PlusMove issues before and after atomic write; backup created on independent copy.");
    Environment.Exit(0);
}
if (args.Length > 1 && args[0] == "--preserve")
{
    var save=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(args[1]).ToArray())!;
    var preservedPk=save.GetPartySlotAtIndex(0); preservedPk.Stat_HPCurrent=1;preservedPk.Status_Condition=8;preservedPk.Move1_PP=1;preservedPk.RefreshChecksum();save.SetPartySlotAtIndex(preservedPk,0,EntityImportSettings.None);
    var folder=Path.Combine(AppContext.BaseDirectory,"data","preserve-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
    var target=Path.Combine(folder,"main");File.WriteAllBytes(target,save.Write().ToArray());
    var editor=new SaveService();editor.EditTeam(target,[new(0,Level:73,Nature:15)]);
    var changed=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(target).ToArray())!;preservedPk=changed.GetPartySlotAtIndex(0);
    if(preservedPk.Stat_HPCurrent!=1 || preservedPk.Status_Condition!=8 || preservedPk.Move1_PP!=1)throw new Exception("Level/nature healed HP, status or PP");
    editor.EditTeam(target,[new(0,Shiny:true)]);
    changed=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(target).ToArray())!;preservedPk=changed.GetPartySlotAtIndex(0);
    if(preservedPk.Stat_HPCurrent!=1 || preservedPk.Status_Condition!=8 || preservedPk.Move1_PP!=1 || preservedPk.ShinyXor!=1)throw new Exception("Shiny-only healed or did not persist star");
    preservedPk.Stat_HPCurrent=preservedPk.Stat_HPMax;preservedPk.RefreshChecksum();changed.SetPartySlotAtIndex(preservedPk,0,EntityImportSettings.None);File.WriteAllBytes(target,changed.Write().ToArray());
    editor.EditTeam(target,[new(0,Level:1)]);changed=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(target).ToArray())!;preservedPk=changed.GetPartySlotAtIndex(0);
    if(preservedPk.Stat_HPCurrent!=preservedPk.Stat_HPMax || preservedPk.Status_Condition!=8 || preservedPk.Move1_PP!=1)throw new Exception("Lower level did not cap HP/preserve status and PP");
    Console.WriteLine("SUCCESS 3 focused preservation cases (level/nature, shiny-only, HP clamp); copies only.");Environment.Exit(0);
}
var root = Path.Combine(AppContext.BaseDirectory, "data", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
Directory.CreateDirectory(root);
if (args.Length == 0) throw new ArgumentException("Passe o caminho de uma cópia de main Z-A: dotnet run --project Tests -- <copia-main>");
var real = Path.GetFullPath(args[0]);
var initialReal = File.ReadAllBytes(real);
var service = new SaveService();
var pristine = (SAV9ZA)SaveUtil.GetSaveFile(initialReal.ToArray())!;
Console.WriteLine($"Input snapshot: party={pristine.PartyCount}, boxes={pristine.BoxCount}, unlocked={pristine.BoxesUnlocked}, revision={pristine.SaveRevision}");
int passed = 0;
void Check(bool ok, string label) { if (!ok) throw new Exception(label); passed++; Console.WriteLine("PASS " + label); }
string Hash(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));
string Fixture(string name, SAV9ZA? source = null)
{
    var folder = Path.Combine(root, name); Directory.CreateDirectory(folder);
    string path = Path.Combine(folder, "main");
    File.WriteAllBytes(path, source?.Write().ToArray() ?? initialReal);
    File.WriteAllBytes(Path.Combine(folder, "backup"), initialReal);
    Directory.CreateDirectory(Path.Combine(folder, "nested")); File.WriteAllText(Path.Combine(folder,"nested","metadata.txt"),"preserve");
    return path;
}
void Reject(string path, AddOptions options, string label, string? expected = null)
{
    string before = Hash(File.ReadAllBytes(path)); bool failed=false;
    try { service.Add(path, options, expected); } catch (Exception ex) when (ex is ArgumentException or IOException or InvalidDataException or InvalidOperationException) { failed = true; }
    Check(failed && Hash(File.ReadAllBytes(path)) == before, label);
}
var teamPath=Fixture("edit-team");
var beforeTeam=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(teamPath))!;
var old0=beforeTeam.GetPartySlotAtIndex(0);var old1=beforeTeam.GetPartySlotAtIndex(1);
var teamResult=service.EditTeam(teamPath,[new(0,Level:73,Nature:15,Shiny:true),new(1,Level:42,Nature:3,Shiny:true)],service.Inspect(teamPath).Hash);
var afterTeam=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(teamPath))!;
var new0=afterTeam.GetPartySlotAtIndex(0);var new1=afterTeam.GetPartySlotAtIndex(1);
Check(new0.CurrentLevel==73 && (int)new0.StatAlignment==15 && new0.IsShiny && new0.ShinyXor==1 && new1.CurrentLevel==42,"EditTeam multiple slots level nature star shiny");
Check(new0.Species==old0.Species && new0.Form==old0.Form && new0.ID32==old0.ID32 && new0.OriginalTrainerName==old0.OriginalTrainerName && new0.Move1==old0.Move1 && new0.IV_HP==old0.IV_HP,"EditTeam preserves identity moves IV");
Check(teamResult.Team.Count==beforeTeam.PartyCount && teamResult.Team[0].BaseStats.Length==6,"InspectTeam data contract");
var off=service.EditTeam(teamPath,[new(0,Shiny:false)]);
Check(!off.Team[0].Shiny && off.Team[1].Shiny,"EditTeam shiny off preserves other slot");
string teamHash=Hash(File.ReadAllBytes(teamPath));bool invalidTeam=false;
try{service.EditTeam(teamPath,[new(0,Level:50),new(1,Level:101)]);}catch(ArgumentException){invalidTeam=true;}
Check(invalidTeam&&Hash(File.ReadAllBytes(teamPath))==teamHash,"EditTeam invalid batch does not partially mutate");
var path = Fixture("box-auto");
var info = service.Inspect(path);
Check(info.Hash == Hash(File.ReadAllBytes(path)), "Inspect hash matches encrypted original");
Check(info.PartyCount == 6, "Input snapshot has full party");
var result = service.Add(path, new AddOptions { Species=4, Level=10, Shiny=true, Nature=15 }, info.Hash);
var edited = (SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(path).ToArray())!;
var pk = edited.GetBoxSlotAtIndex(result.Pokemon.Index);
Check(edited.PartyCount==6 && result.Location.StartsWith("Caixa"), "Auto with party six uses box");
Check(pk.Species==4 && pk.CurrentLevel==10 && pk.IsShiny && (int)pk.Nature==15 && (int)pk.StatAlignment==15, "Charmander shiny level 10 Modest persisted");
Check(pk.ChecksumValid && pk.OriginalTrainerName==pristine.OT && pk.ID32==pristine.ID32, "Checksum and trainer preserved");
Check(Hash(File.ReadAllBytes(Path.Combine(result.BackupDirectory,"main"))) == info.Hash, "Backup contains original main");
Check(Hash(File.ReadAllBytes(Path.Combine(result.BackupDirectory,"backup")))==Hash(initialReal) && File.ReadAllText(Path.Combine(result.BackupDirectory,"nested","metadata.txt"))=="preserve", "Integral folder backup preserves game backup and nested data");
var manifest=JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Path.Combine(result.BackupDirectory,"manifest.json")))!;
Check(manifest.All(kv=>Hash(File.ReadAllBytes(Path.Combine(result.BackupDirectory,kv.Key)))==kv.Value), "Backup manifest hashes verify");
Check(Hash(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(path)!,"backup")))==Hash(initialReal), "Game backup unchanged after write");
var partial=(SAV9ZA)pristine.Clone(); partial.SetPartySlotAtIndex(partial.BlankPKM,5);
var partialPath=Fixture("party-space",partial);
var partyResult=service.Add(partialPath,new AddOptions {Species=4, Level=10, Nature=15, Shiny=true});
Check(service.Inspect(partialPath).PartyCount==6 && partyResult.Location.StartsWith("Equipe"), "Auto fills party when space exists");
Reject(path,new AddOptions {Destination=Destination.Party},"Explicit full party fails without mutation");
Reject(path,new AddOptions {Species=25,Form=200},"Invalid form rejected without mutation");
Reject(path,new AddOptions {Species=6,Form=1},"Mega battle-only form rejected");
Reject(path,new AddOptions {Level=0},"Invalid level rejected");
Reject(path,new AddOptions {Gender=2},"Invalid gender rejected");
Reject(path,new AddOptions {Ball=999},"Invalid ball rejected");
Reject(path,new AddOptions {Moves=[9999,0,0,0]},"Invalid move rejected");
Reject(path,new AddOptions {Moves=[105,0,0,0]},"Incompatible move rejected");
Reject(path,new AddOptions {IVs=[32,0,0,0,0,0]},"Invalid IV rejected");
Reject(path,new AddOptions {EVs=[252,252,252,0,0,0]},"Invalid EV total rejected");
Reject(path,new AddOptions(),"Stale expected hash rejected","BADHASH");
var corrupt=Fixture("corrupt"); var bytes=File.ReadAllBytes(corrupt);bytes[25]^=0xFF;File.WriteAllBytes(corrupt,bytes);
Reject(corrupt,new AddOptions(),"Corrupt checksum rejected without mutation");
var full=(SAV9ZA)pristine.Clone(); var existing=full.GetPartySlotAtIndex(0);
for(int i=0;i<full.BoxCount*30;i++) full.SetBoxSlotAtIndex(existing,i);
Reject(Fixture("full-boxes",full),new AddOptions(),"Full storage fails without overwrite");
int boxSlot=result.Pokemon.Index;
var partyBeforeBox=Enumerable.Range(0,6).Select(i=>edited.GetPartySlotAtIndex(i).Data.ToArray()).ToArray();
var boxChange=service.EditBox(path,[new(boxSlot,Level:44,Nature:3,Shiny:false)],service.Inspect(path).Hash);
var boxEdited=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(path))!;
var boxPokemon=boxEdited.GetBoxSlotAtIndex(boxSlot);
Check(boxPokemon.CurrentLevel==44 && (int)boxPokemon.StatAlignment==3 && !boxPokemon.IsShiny,"EditBox level nature shiny persisted");
Check(Enumerable.Range(0,6).All(i=>partyBeforeBox[i].AsSpan().SequenceEqual(boxEdited.GetPartySlotAtIndex(i).Data)),"EditBox preserves party byte for byte");
Check(service.InspectBox(path,boxSlot/30).Any(p=>p.Slot==boxSlot&&p.Level==44),"InspectBox global index contract");
var mixed=service.SaveEdits(path,[new(0,Level:81)],[new(boxSlot,Level:55,Shiny:true)],service.Inspect(path).Hash);
var mixedSave=(SAV9ZA)SaveUtil.GetSaveFile(File.ReadAllBytes(path))!;
Check(mixedSave.GetPartySlotAtIndex(0).CurrentLevel==81 && mixedSave.GetBoxSlotAtIndex(boxSlot).CurrentLevel==55 && mixedSave.GetBoxSlotAtIndex(boxSlot).ShinyXor==1,"Mixed team and box transaction commits together");
string mixedHash=service.Inspect(path).Hash; bool invalidMixed=false;
try{service.SaveEdits(path,[new(0,Level:80)],[new(boxSlot,Level:101)]);}catch(ArgumentException){invalidMixed=true;}
Check(invalidMixed&&service.Inspect(path).Hash==mixedHash,"Invalid mixed batch changes nothing");
Check(NatureAdvisor.GetMoveCategory(1)==1 && NatureAdvisor.GetMoveCategory(53)==2 && NatureAdvisor.GetMoveCategory(45)==0,"Move category primary data physical special status");
var template=service.InspectTeam(path)[0];
int canonicalNature=NatureAdvisor.Suggest(template).Nature;
Check(NatureAdvisor.Suggest(template with {Moves=[53,0,0,0],MoveCategories=[2,0,0,0]}).Nature==canonicalNature,"Nature remains canonical when moves change to special");
Check(NatureAdvisor.Suggest(template with {Moves=[1,0,0,0],MoveCategories=[1,0,0,0],Nature=24,Level=1,BaseStats=[80,100,90,100,100,80]}).Nature==canonicalNature,"Forged client stats, current nature and level do not change recommendation");
bool invalidNatureSpecies=false;try{NatureAdvisor.Suggest(template with{Species=int.MaxValue});}catch(ArgumentException){invalidNatureSpecies=true;}
Check(invalidNatureSpecies,"Unavailable species recommendation rejects safely");
var make=typeof(SaveService).GetMethod("CreatePokemon",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static)!;
foreach (var sample in new[] { (4,0), (120,0), (150,0), (384,0), (26,1), (670,5), (718,0), (1007,0) })
{
    if (!Catalog.Forms(sample.Item1).Any(f=>f.Id==sample.Item2)) continue;
    var generated=(PA9)make.Invoke(null,[pristine,new AddOptions {Species=sample.Item1,Form=sample.Item2,Level=100,Shiny=true}])!;
    Check(generated.Species==sample.Item1 && generated.Form==sample.Item2 && generated.IsShiny && generated.ChecksumValid && generated.CurrentLevel==100,$"Generate species {sample.Item1} form {sample.Item2}");
}
Reject(path,new AddOptions { PreferredBox=pristine.BoxesUnlocked },"Locked box rejected");
var onlyLocked=(SAV9ZA)pristine.Clone();
for(int i=0;i<onlyLocked.BoxesUnlocked*30;i++) onlyLocked.SetBoxSlotAtIndex(existing,i);
Reject(Fixture("accessible-full",onlyLocked),new AddOptions(),"Auto cannot spill into locked boxes");
var racePath=Fixture("race"); var raceHash=service.Inspect(racePath).Hash;
var racers=new List<System.Diagnostics.Process>();
for(int n=0;n<2;n++)
{
    var start=new System.Diagnostics.ProcessStartInfo(Environment.ProcessPath!){UseShellExecute=false};
    start.ArgumentList.Add("--race");start.ArgumentList.Add(racePath);start.ArgumentList.Add(raceHash);
    racers.Add(System.Diagnostics.Process.Start(start)!);
}
foreach(var racer in racers) racer.WaitForExit();
Check(racers.Count(p=>p.ExitCode==0)==1 && racers.Count(p=>p.ExitCode==2)==1,"Cross-process concurrent edit: exactly one write, stale edit rejected");
foreach(var racer in racers) racer.Dispose();
Check(Catalog.Species.All(c=>PersonalTable.ZA.IsSpeciesInGame((ushort)c.Id)),"Catalog species all present in ZA");
Check(Catalog.Species.All(c=>Catalog.Forms(c.Id).All(f=>PersonalTable.ZA.IsPresentInGame((ushort)c.Id,(byte)f.Id)&&!FormInfo.IsBattleOnlyForm((ushort)c.Id,(byte)f.Id,9))),"Catalog forms exclude absent and battle-only");
Check(Catalog.Genders(120,0).Single().Id==2,"Genderless catalog");
Check(Catalog.Natures.Any(c=>c.Id==15 && c.Name.Contains("Modest")),"Modest nature discoverable");
Check(Hash(initialReal)==Hash(File.ReadAllBytes(real)),"Source save never modified by tests");
Console.WriteLine($"SUCCESS {passed} checks. Data: {root}");
