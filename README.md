# ZA Recomp Editor

**A portable Pokémon Legends: Z-A save editor for ZA Recomp.** Browse boxes, edit your party and add Pokémon through a dark desktop interface with automatic backups.

[Download for Windows](https://github.com/eep0x10/zarecomp-editor/releases/latest) · [Português (Brasil)](README.pt-BR.md) · [Español](README.es.md) · [Report an issue](https://github.com/eep0x10/zarecomp-editor/issues)

The application is called **Recomp Pokémon**. Windows x64, no installer or separate .NET installation required. No game files are included.

## Quick start

1. Download the portable ZIP from [Releases](https://github.com/eep0x10/zarecomp-editor/releases/latest), then extract it into a writable folder.
2. Close ZA Recomp and open `RecompPokemon.exe`.
3. Use save discovery or select your save's **`main`** file. A common location is `<ZA Recomp folder>/save/save_0/main`; some distributions use a `PORT` subfolder. Check trainer and modification date if several copies appear.
4. Select a party or box Pokémon, change its level, nature or shiny status, then save. The star changes the draft until you save.
5. Use **+** to add a Pokémon. Automatic placement uses an empty party slot first, then an empty slot in an unlocked box.

Keep your own backup too. This editor supports Z-A saves; it does not convert saves from other games.

## Features

| Capability | What you can do |
| --- | --- |
| Box browser | Browse a 30-slot box grid beside your current six-slot party |
| Edit existing Pokémon | Change level, nature and shiny status with a preview |
| Add Pokémon | Choose supported species/form, level, nature and shiny; optionally set nickname, gender, ball, moves, IVs and EVs |
| Optimize | Preview suggested party natures, with a reason for each suggestion |
| Languages | Switch the interface between PT-BR, English and Spanish without losing pending edits |
| Portable storage | Settings and cached artwork stay beside the executable |
| Save protection | Folder backups, SHA-256 manifests, stale-file checks, validated writes and atomic replacement |

Catalogue names and PKHeX reports currently remain in English. The dark interface requests Windows Desktop Acrylic where supported; opaque surfaces remain the fallback.

### What Optimize means

Optimize recommends **natures only**, using base stats and physical/special moves. It does not change moves, IVs, EVs or levels. Suggestions are editable drafts, not a universal competitive ranking: team role, Mega Evolution, opponents and Z-A battle mechanics can change the best choice.

## Backups and recovery

Before a write, the editor copies the save folder into a sibling `ZA-Tool-Backups/<timestamp>-<id>/` directory and records a SHA-256 manifest. It validates the result before replacing `main`, and refuses a file changed since it was opened. Adding a Pokémon never overwrites an occupied slot.

To restore: close the game and editor, preserve the current file separately, then copy `main` from the desired backup over the edited `main`. Reopen the game to check it. Keep durable backups outside temporary or synchronised folders.

**Always close the game first.** Detection checks known ZA Recomp process names, not every renamed executable. Save validation does not guarantee that a customised Pokémon matches a legitimate encounter or will be accepted online; read the PKHeX analysis shown after creation.

## Privacy and artwork

Editing works offline. Optional artwork uses public PokéAPI resources and a local cache; species/form IDs are used in those requests. Saves, trainer names and nicknames are not uploaded. Image hosts receive ordinary HTTP connection information. Missing images do not block editing. A writable application folder is needed to persist settings and cache.

## Build from source

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) on Windows, then run:

```powershell
git clone https://github.com/eep0x10/zarecomp-editor.git
cd zarecomp-editor
./build.ps1
```

Portable output goes to `dist/`. `App/` contains the WinForms interface; `Engine/` contains save transactions, catalogue and nature suggestions; `Tests/` contains the regression harness; `vendor/PKHeX.Core/` is the pinned save-format dependency.

Tests require a **disposable copy** of a Z-A `main` save and write fixtures to the test executable's `data/` directory (under `Tests/bin/`):

```powershell
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- "C:/DisposableTestSave/main"
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- --preserve "C:/DisposableTestSave/main"
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- --plus "C:/DisposableTestSave/main"
```

No personal saves are bundled. CI compiles the application and test harness; it does not run private-save regression tests. See [contributing](CONTRIBUTING.md) and the historical [verification record](Tests/verification.txt) for scope and limits.

## Licence and acknowledgements

Code is [GPL-3.0-or-later](LICENSE). Uses [PKHeX.Core](https://github.com/kwsch/PKHeX) commit `96e33e7432d900e5345afdfd117eabfac43f5bbd`; corresponding source is included. See [third-party notices](THIRD_PARTY_NOTICES.md) for Pokémon data, PokéAPI and .NET notices.

Independent community tool, unaffiliated with Nintendo, Game Freak, The Pokémon Company, ZA Recomp or PKHeX. Pokémon assets and trademarks belong to their respective owners.
