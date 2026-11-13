# Detailed Pokémon editing

[Main README](../README.md) · [Português](#português) · [Español](#español)

Version2 uses one local-browser workspace. The same inspector handles existing Pokémon and creation; the final write actions remain distinct. No application dialogs are required.

| Workflow | Before writing | When the save changes |
| --- | --- | --- |
| Edit existing Pokémon | Changes update an in-memory draft per party/box slot; selecting another slot preserves it | **Save with backup** validates and writes the pending batch |
| Add Pokémon | Required choices plus enabled optional overrides describe a new Pokémon in the same inspector | **Add with backup** validates and writes immediately; cancel creation writes nothing |

Close ZA Recomp before either write. Confirm the selected main file and trainer. Examples and screenshots use synthetic data; no game or personal save is bundled.

## Edit an existing Pokémon

1. Select an occupied party/box slot. Its values appear in the inline inspector.
2. Edit the quick identity controls or the Profile, Attributes, Moves, Origin and Advanced groups. Fields not changed remain untouched apart from dependent values documented below.
3. Select other Pokémon or boxes as needed. A pending count and marked slots show which records have drafts.
4. Use **Validate draft** for an Engine preview without a disk write. A field error preserves your values for correction.
5. **Save with backup** commits the complete pending batch; **Discard drafts** abandons pending edits through an inline confirmation.

There is no separate Apply dialog. Save before reloading or closing the browser: drafts are temporary editing state, not automatic writes. A stale file must be reopened after resolving the old drafts. If the transport fails during a commit or the host reports an unknown outcome, inspect/reload the result and backup before starting another write. Do not infer that an error means nothing was written.

Changing species/form edits the existing record, not its encounter history. The Engine reconciles supported dependent data, preserves existing HP up to the new maximum and retains status. Original nature and effective Mint nature are separate controls; changing one is not permission to silently replace the other.

## Add a custom Pokémon

Resolve existing edit drafts, then select **+ Add Pokémon** or a free position. Creation reuses the inspector without obscuring your collection. Choose species, persistent form, level, original nature, shiny style and ball. Enable optional fields only when overriding the generated value; **Automatic** is not zero or perfect IVs and disabled fields are not a generated preview.

Choose **Automatic**, **Party** or **Boxes**, plus a preferred unlocked box if desired. Automatic uses party space first, then a box. A full preferred box may fall back to another unlocked box; no occupied Pokémon is overwritten. **Add with backup** is an immediate backed-up write, separate from edit drafts. Cancel creation inline to abandon it.

Preview before committing to inspect the generated values and PKHeX findings. Existing-record metadata such as tracker, handler, ribbons and original report is read-only; creation does not invent raw identifiers, egg history or ribbons.

## Five field groups and limits

<details>
<summary>Real workbench examples: moves and origin</summary>

![Current moves and Plus records](screenshots/web/workbench-moves.jpg)

![Trainer and encounter metadata](screenshots/web/workbench-origin.jpg)

</details>

| Tab | Editable values | Important limits |
| --- | --- | --- |
| **Identity** | Species, persistent form, nickname/use nickname, favourite, gender, level, original nature, effective Mint nature, non-shiny/star/square style, ball and held item | Species/form and gender must be available together in Z-A; level 1–100; nickname up to 12 UTF-16 units; a different effective nature needs an available Mint; held items include Mega Stones |
| **Attributes** | Six IVs, six EVs, six Hyper Training flags, friendship and six coloured markings | IV 0–31; EV 0–252 each and 510 total; Hyper Training requires level 50 and an IV below 31 for that stat; friendship 0–255 |
| **Moves** | Four move slots and registered Plus moves | At least one non-empty move; no duplicate non-empty slots; a newly selected moveset must be compatible with the species/form and level/TM learnset; required natural Plus records are restored |
| **Origin** | Original trainer name/gender, TID16/SID16, language, encounter level/location/date and obedience level | Trainer name up to 12 UTF-16 units; IDs 0–65535; encounter level 1–100; date 2000–2099; valid native Z-A location; obedience depends on encounter/current level and ownership |
| **Advanced** | Alpha flag, supported height/weight/scale scalars and an already-owned affixed title | Scalar values 0–255; Alpha needs consistent encounter data, size, IVs and Plus records; affixing a title does not award a new ribbon |

The internal stat order is **HP, Attack, Defense, Speed, Special Attack, Special Defense**. Hyper Training is a separate flag and requires level50 with IV below31 for that stat. The IV31/0 presets explicitly remove Hyper Training. EV total is shown and capped at510.

An empty nickname with nickname use disabled restores the species name. Favourite and coloured markings are organisation metadata. Existing ribbons can be affixed as a title; selecting a title does not grant it.

## Z-A mechanics and preserved data

- **Held items matter**, including Mega Stones. This editor offers persistent forms and does not write temporary Mega/Primal battle forms into boxes. That is an editor policy, not a claim that every party battle-form record is impossible.
- **PP is not a Z-A battle resource**, and PP Ups are unavailable. Stored PP/PP Ups are shown read-only; selecting new moves calculates the corresponding native PP data. There is no unlimited-PP or cooldown-editing feature.
- **Relearn slots are legacy/transfer history.** They are shown read-only, not as the game's complete relearn menu. Current moves and Plus records have separate controls.
- **Plus records have required entries.** Replacing optional records cannot remove moves naturally required by the current level or Alpha/base-form rules. Species/form changes clear incompatible records and restore the required ones.
- **Alpha:** use the Alpha shortcut and select **Set as Alpha** in Advanced. This sets Scale 255, synchronizes native/HOME scalars, and raises only enough IVs to reach three perfect stats, preserving other IVs. Hyper Training is cleared only where an IV becomes 31. Unchecking Alpha keeps those attributes. When adding, the generator selects an Alpha encounter compatible with the species, form and level; if none exists it stops without writing. For example, Charmander Alpha needs level 25 or higher; Mewtwo has no Alpha template. Editing an existing Pokémon preserves its origin, which must already support the requested Alpha status; checking the box does not replace its encounter history.
- **Size values are scalars, not metres or kilograms.** Native Z-A records use Scale and keep HeightScalar/WeightScalar at zero under this editor's validation policy. HOME records must keep both scalars equal to Scale. Do not treat the three controls as independent size sliders.
- **Titles require an existing ribbon/mark.** Choose an already-owned title or None. The editor does not grant event ribbons or invent their history.

PID, encryption constant, HOME tracker, ability/slot, origin game, egg history and handler metadata are inspection fields, not raw editing controls. Changing species/form reconciles the dependent ability slot; changing trainer identity may update handler ownership while retaining friendship unless you set it. Changing raw trainer IDs preserves the previous shiny style unless you explicitly choose another style; PID may be adjusted as a consequence. There is no direct PID/EC editor, egg-creation workflow or conversion from another game.

Raw **TID16/SID16** are the two stored 16-bit values, not the six-digit trainer number displayed by modern games. Displayed IDs are available in Advanced for comparison. Encounter-location editing is for native Z-A records; it does not convert imported history into a Z-A encounter.

The **Advanced PKHeX report describes the original record before the current draft**. It is not a live certificate for every pending change. Field validation and a successful write do not guarantee a legitimate encounter, acceptance by the game in every case, or online compatibility.

## Backups and checking the result

Each successful write is preceded by a full save-folder backup in a sibling `ZA-Tool-Backups/<timestamp>-<id>/` directory with a SHA-256 manifest. The editor checks for stale files, reopens the output and validates it before replacing `main`. Keep a separate durable backup as well.

After saving or adding, keep the automatic backup until you have checked the result in the game. To restore, close the game and editor, preserve the current file elsewhere, then copy the desired backup's `main` over the edited `main`. Process detection recognises known ZA Recomp names, not every renamed executable; always close the game yourself first.

## Português

A versão2 mantém tudo na mesma página: selecionar um Pokémon abre o editor inline, e mudar de seleção preserva os rascunhos. **Validar rascunho** não grava. **Salvar com backup** confirma o lote de edições; **Descartar rascunhos** pede confirmação inline. Salve antes de recarregar/fechar o navegador.

**Adicionar Pokémon** reutiliza o painel e **Adicionar com backup grava imediatamente**. Resolva rascunhos anteriores. Campo automático mantém o valor gerado; ative apenas as substituições desejadas. Equipe/box precisam de espaço livre. Nenhum Pokémon ocupado é substituído.

Natureza original/Mint e IVs/Hyper Training são separados. Alpha depende de encontro compatível; marcar ajusta escala255 e pelo menos três IVs31. Altura/peso nativos ficam em0; HOME mantém ambos iguais à escala. Golpes/natures e relatórios permanecem em inglês. Campos técnicos, PP/histórico, habilidade, ovos e ribbons concedidos são de consulta. Uma falha durante a gravação exige conferir o resultado e o backup antes de tentar novamente.

## Español

La versión2 mantiene todo en la misma página. Seleccionar un Pokémon abre el editor integrado; cambiar de selección conserva los borradores. **Validar borrador** no escribe. **Guardar con copia** confirma el lote de ediciones; **Descartar borradores** pide confirmación integrada. Guarda antes de recargar/cerrar el navegador.

**Añadir Pokémon** reutiliza el panel y **Añadir con copia escribe inmediatamente**. Resuelve los borradores anteriores. Automático conserva valores generados; activa solo las opciones deseadas. Se necesita espacio libre y nunca se sobrescriben Pokémon ocupados.

Naturaleza original/menta e IVs/Hyper Training son distintos. Alfa requiere encuentro compatible y fija escala255 y al menos tres IVs31. Altura/peso nativos quedan en0; HOME iguala ambos a escala. Movimientos/naturalezas e informes permanecen en inglés. Datos técnicos, PP/historial, habilidad, huevos y cintas otorgadas son de consulta. Un fallo durante escritura exige comprobar resultado y copia antes de reintentar.
