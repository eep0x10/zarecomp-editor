# ZA Recomp Editor

**A portable Pokémon Legends: Z-A save editor for ZA Recomp.** Browse boxes, edit existing Pokémon and create custom additions with automatic backups.

[Download for Windows](https://github.com/eep0x10/zarecomp-editor/releases/latest) · [Português (Brasil)](README.pt-BR.md) · [Español](README.es.md) · [Detailed editing guide](docs/EDITING.md) · [Report an issue](https://github.com/eep0x10/zarecomp-editor/issues)

The application is called **Recomp Pokémon**. Windows x64, no installer or separate .NET installation required. No game files or personal saves are included.

## Start with your save

1. Download the portable ZIP from [Releases](https://github.com/eep0x10/zarecomp-editor/releases/latest) and extract it into a writable folder.
2. Close ZA Recomp and open `RecompPokemon.exe`.
3. Find a save or select its **`main`** file. Typical paths are `<ZA Recomp folder>/save/save_0/main` and `<ZA Recomp folder>/PORT/save/save_0/main`. Check the trainer and modification date if several copies appear.

Your party stays on the left, a 30-slot box fills the centre, and the selected Pokémon appears on the right.

![Box browser with the party and selected Pokémon](docs/screenshots/boxes.png)

All screenshots below show the real application with a synthetic demonstration save.

## Edit an existing Pokémon

1. Select a Pokémon from the party or a box. Use the quick controls or open **Edit details**.
2. Review the five tabs, then choose **Apply to draft**. This does **not** write the save. **Cancel** leaves the previous draft intact.
3. Review other Pokémon or adjust your drafts, then choose **Save changes** to validate and write the whole batch with a backup. **Discard drafts** abandons pending edits.

| Tab | What it contains |
| --- | --- |
| Identity | Species/form, nickname, gender, level, original nature and Mint effect, shiny style, ball, held item and favourite |
| Attributes | IVs, EVs, Hyper Training, friendship and coloured markings |
| Moves | Four move slots and Plus records; stored PP/relearn information is read-only |
| Origin | Original trainer, raw trainer IDs, language, encounter data and obedience |
| Advanced | Alpha/size, an already-owned title, technical metadata and the original PKHeX report |

<details>
<summary>See all five editing tabs</summary>

**Identity** — original nature and the effective Mint nature are separate choices.

![Existing Pokémon: Identity tab](docs/screenshots/details.png)

**Attributes** — IVs, EVs and Hyper Training remain separate, with a visible EV total.

![Existing Pokémon: Attributes tab](docs/screenshots/attributes.png)

**Moves** — choose compatible moves and review registered Plus moves.

![Existing Pokémon: Moves tab](docs/screenshots/moves.png)

**Origin** — review trainer identity and encounter metadata before changing them.

![Existing Pokémon: Origin tab](docs/screenshots/origin.png)

**Advanced** — inspect preserved technical data and supported size/title settings.

![Existing Pokémon: Advanced tab](docs/screenshots/advanced.png)

</details>

## Add a custom Pokémon

1. Save or discard existing drafts, then click **+ Add Pokémon**.
2. Set species, form, level, original nature, shiny style and ball. The same five tabs expose optional customisation: **tick an optional field to override its automatic value**; leave it unticked to let generation choose it.
3. Choose the destination. **Automatic** fills an empty party slot first, then an empty slot in an unlocked box. **Team** requires party space; **Boxes** uses an available box slot without overwriting a Pokémon.
4. Click **Add Pokémon**. This action **writes immediately with a backup**; it does not create a main-window draft. Cancelling the dialog writes nothing.

![Add Pokémon: required identity choices and optional overrides](docs/screenshots/add-pokemon.png)

<details>
<summary>See optional attributes when adding</summary>

Enable only the IV, EV, Hyper Training or other options you want to supply. An unticked field contributes no override; its disabled controls are not a preview of the generated values.

![Add Pokémon: optional attribute overrides](docs/screenshots/add-attributes.png)

</details>

Keep your own backup too. This editor supports Z-A saves; it does not convert saves from other games. See the [detailed guide](docs/EDITING.md) for field limits, draft behaviour and examples of preserved data.

## Features

| Capability | What you can do |
| --- | --- |
| Box browser | Browse a 30-slot box grid beside your current six-slot party |
| Edit existing Pokémon | Quick level/nature/shiny controls plus a detailed editor for identity, stats, moves, origin and advanced fields |
| Add Pokémon | Customise supported species/form, nickname, gender, shiny style, nature/Mint, IVs/EVs, Hyper Training, moves/Plus, item and origin metadata |
| Optimize | Preview suggested party natures, with a reason for each suggestion |
| Languages | Switch the interface between PT-BR, English and Spanish without losing pending edits |
| Portable storage | Settings and cached artwork stay beside the executable |
| Save protection | Folder backups, SHA-256 manifests, stale-file checks, validated writes and atomic replacement |

Catalogue names and PKHeX reports currently remain in English. The dark interface requests Windows Desktop Acrylic where supported; opaque surfaces remain the fallback.

See the [detailed editing guide](docs/EDITING.md) for field groups, drafts and Z-A-specific limits. PP/history and technical identifiers are handled according to their actual format semantics; not every stored byte is an editable gameplay feature.

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

Tests require a **disposable copy** of a Z-A `main` save and write fixtures to the test executable's `data/` directory (under `Tests/bin/`). The main suite expects a full six-Pokémon party; `--preserve` requires an occupied first party slot:

```powershell
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- "C:/DisposableTestSave/main"
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- --preserve "C:/DisposableTestSave/main"
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- --plus "C:/DisposableTestSave/main"
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- --details "C:/DisposableTestSave/main"
```

No personal saves are bundled. CI compiles the application and test harness; it does not run private-save regression tests. See [contributing](https://github.com/eep0x10/zarecomp-editor/blob/main/CONTRIBUTING.md) and the historical [verification record](https://github.com/eep0x10/zarecomp-editor/blob/main/Tests/verification.txt) for scope and limits.

## Licence and acknowledgements

Code is [GPL-3.0-or-later](LICENSE). Uses [PKHeX.Core](https://github.com/kwsch/PKHeX) commit `96e33e7432d900e5345afdfd117eabfac43f5bbd`; corresponding source is included. See [third-party notices](THIRD_PARTY_NOTICES.md) for Pokémon data, PokéAPI and .NET notices.

Independent community tool, unaffiliated with Nintendo, Game Freak, The Pokémon Company, ZA Recomp or PKHeX. Pokémon assets and trademarks belong to their respective owners.
