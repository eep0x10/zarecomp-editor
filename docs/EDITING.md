# Detailed Pokémon editing

[Main README — English](../README.md) · [Português](#português) · [Español](#español)

The editor offers the same **five tabs** for an existing Pokémon and a new addition, but the final action is different:

| Workflow | What the dialog does | When the save changes |
| --- | --- | --- |
| **Edit details** | **Apply to draft** returns edits to the main window; **Cancel** preserves the previous draft | **Save changes** validates and writes the pending batch with a backup |
| **+ Add Pokémon** | Required choices plus optional overrides describe a new Pokémon | **Add Pokémon** validates and writes immediately with a backup; **Cancel** writes nothing |

Close ZA Recomp before either write. Choose the intended `main` file and confirm the trainer before working. The screenshots below use a synthetic demonstration save in the real application.

## Edit an existing Pokémon

1. Select an occupied party or box slot, then open **Edit details**. Existing drafts for that Pokémon appear in the controls.
2. Change only what you intend to edit. Review the Identity, Attributes, Moves, Origin and Advanced tabs; fields not changed remain untouched, apart from dependent values described below.
3. Choose **Apply to draft**. The dialog closes and the pending changes remain in the main window. This is not a disk write and does not create a backup yet.
4. Review other Pokémon or adjust the quick controls. Drafts can span the party and boxes. Use **Save changes** to commit the whole batch with one backup, or **Discard drafts** to abandon it.

**Cancel** in the detail dialog discards only changes made during that dialog visit. It leaves the earlier main-window draft intact. A failed save does not partially apply a mixed party/box batch. If the file changed externally, reopen it instead of overwriting the newer file.

Changing species or form edits the existing record; it does not generate a replacement encounter. Gender, moves and other dependent data must still fit the chosen species/form. The engine reconciles the ability slot, default species name, obsolete form argument and Plus records where required. Nicknames are retained unless you change/reset them. Existing HP is kept up to the new maximum, status is preserved, and recalculated attributes reflect the new level/IVs/EVs/Hyper Training/nature.

The quick **Nature** control retains its original behaviour of setting both original and effective nature. Use the detailed editor to keep those values separate.

## Add a custom Pokémon

1. Save or discard pending main-window drafts, then click **+ Add Pokémon**.
2. In **Identity**, choose species, form, level, original nature, shiny style and ball. These are direct choices.
3. To supply an optional value, tick its checkbox before editing it. **Unticked means automatic**, not zero, perfect IVs, or the number currently shown in a disabled control. Unchecked fields do not contribute an override.
4. Choose **Automatic**, **Team** or **Boxes**. Automatic uses an empty party slot first and then an unlocked box. Team fails if all six slots are occupied. A preferred box is tried first; another unlocked box can be used if it is full. No occupied Pokémon is overwritten.
5. Choose **Add Pokémon**. The engine generates a base record, applies the enabled overrides, validates the result, backs up the save folder and writes the addition immediately. Review the reported result and PKHeX findings. It is not added to the main-window draft queue.

Cancel the dialog to abandon the unsubmitted addition without writing. The same field groups are available, but inspection data such as an existing Pokémon's handler, tracker, ribbons and original PKHeX report appears only when editing an existing record.

![Add Pokémon: identity choices and optional overrides](screenshots/add-pokemon.png)

<details>
<summary>Example: optional attributes when adding</summary>

Enable IVs or EVs only when you want to provide the whole labelled stat group. For a natural/generated result, leave those options unticked. Hyper Training is a separate set of flags, not a substitute for entering IVs.

![Add Pokémon: optional IV, EV and Hyper Training controls](screenshots/add-attributes.png)

</details>

## Five tabs and their limits

| Tab | Editable values | Important limits |
| --- | --- | --- |
| **Identity** | Species, persistent form, nickname/use nickname, favourite, gender, level, original nature, effective Mint nature, non-shiny/star/square style, ball and held item | Species/form and gender must be available together in Z-A; level 1–100; nickname up to 12 UTF-16 units; a different effective nature needs an available Mint; held items include Mega Stones |
| **Attributes** | Six IVs, six EVs, six Hyper Training flags, friendship and six coloured markings | IV 0–31; EV 0–252 each and 510 total; Hyper Training requires level 50 and an IV below 31 for that stat; friendship 0–255 |
| **Moves** | Four move slots and registered Plus moves | At least one non-empty move; no duplicate non-empty slots; a newly selected moveset must be compatible with the species/form and level/TM learnset; required natural Plus records are restored |
| **Origin** | Original trainer name/gender, TID16/SID16, language, encounter level/location/date and obedience level | Trainer name up to 12 UTF-16 units; IDs 0–65535; encounter level 1–100; date 2000–2099; valid native Z-A location; obedience depends on encounter/current level and ownership |
| **Advanced** | Alpha flag, supported height/weight/scale scalars and an already-owned affixed title | Scalar values 0–255; Alpha needs consistent encounter data, size, IVs and Plus records; affixing a title does not award a new ribbon |

The stat controls are labelled individually. Internally their order is **HP, Attack, Defense, Speed, Special Attack, Special Defense**. Hyper Training preserves the actual stored IV and adds a separate flag. Disable a flag before changing its IV to 31 or lowering the Pokémon below level 50.

An empty nickname with nickname use disabled restores the species name. Favourite and coloured markings are organisation metadata; they do not improve battle attributes.

<details>
<summary>See all five tabs for an existing Pokémon</summary>

**Identity:** separate original nature from the Mint effect and choose a supported persistent form.

![Existing Pokémon: Identity](screenshots/details.png)

**Attributes:** review IVs, EVs, Hyper Training, friendship and markings independently.

![Existing Pokémon: Attributes](screenshots/attributes.png)

**Moves:** choose the four moves and review the permitted Plus records.

![Existing Pokémon: Moves](screenshots/moves.png)

**Origin:** trainer identity and encounter metadata are distinct from ordinary battle attributes.

![Existing Pokémon: Origin](screenshots/origin.png)

**Advanced:** inspect technical identifiers and the original legality report alongside supported metadata.

![Existing Pokémon: Advanced](screenshots/advanced.png)

</details>

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

[README em português](../README.pt-BR.md) · [English guide](#detailed-pokémon-editing)

**Editar existente:** selecione o Pokémon da equipe ou box, abra **Editar detalhes** e revise Identidade, Atributos, Golpes, Origem e Avançado. **Aplicar ao rascunho** não grava o save. **Cancelar** mantém o rascunho anterior; **Descartar rascunhos** abandona as mudanças pendentes na janela principal. **Salvar alterações** valida e grava o lote inteiro com backup, sem aplicar parcialmente um lote inválido.

**Adicionar personalizado:** salve ou descarte os rascunhos antes de clicar em **+ Adicionar Pokémon**. Escolha os campos básicos e marque apenas os opcionais que quiser definir. Campo desmarcado mantém o valor automático; o número desativado não é uma prévia do resultado. **Adicionar Pokémon grava imediatamente com backup**, sem criar um rascunho. Automático usa primeiro a equipe e depois uma box desbloqueada; posições ocupadas nunca são substituídas. Cancelar não grava nada.

Natureza original e efeito de Mint são separados. IVs vão de 0 a 31; EVs de 0 a 252 por atributo e até 510 no total. Hyper Training exige nível 50 e um IV abaixo de 31 no atributo marcado. Alterar atributos recalcula os valores sem curar silenciosamente: HP anterior é limitado ao novo máximo e status é preservado. Marcações e favorito são dados de organização.

PP/PP Ups e relearn são somente leitura; não são edição de recarga ou PP infinito. Itens incluem Mega Stones, mas o editor oferece formas persistentes. Alpha depende do encontro, tamanho, IVs e Plus; os valores de tamanho são escalares, não metros ou quilos. Título só pode usar uma fita/marca já existente. IDs brutos TID16/SID16 não são o número moderno de seis dígitos; dados técnicos, origem do jogo, ovos e histórico de handler são para consulta. A data de encontro validada vai de 2000 a 2099.

O relatório da aba Avançado é do Pokémon original, antes do rascunho. Validação estrutural e backup não garantem encontro legítimo, funcionamento de toda combinação no jogo ou aceitação online. Feche sempre o jogo, confira o resultado e conserve o backup.

## Español

[README en español](../README.es.md) · [English guide](#detailed-pokémon-editing)

**Editar existente:** selecciona el Pokémon del equipo o caja, abre **Editar detalles** y revisa Identidad, Atributos, Movimientos, Origen y Avanzado. **Aplicar al borrador** no escribe la partida. **Cancelar** mantiene el borrador anterior; **Descartar borradores** abandona los cambios pendientes de la ventana principal. **Guardar cambios** valida y escribe el lote completo con copia de seguridad, sin aplicar parcialmente un lote inválido.

**Añadir personalizado:** guarda o descarta los borradores antes de pulsar **+ Añadir Pokémon**. Elige los campos básicos y marca solo los opcionales que quieras definir. Sin marcar se mantiene el valor automático; un número desactivado no es una vista previa del resultado. **Añadir Pokémon escribe inmediatamente con copia de seguridad**, sin crear un borrador. Automático utiliza primero el equipo y después una caja desbloqueada; nunca sobrescribe posiciones ocupadas. Cancelar no escribe nada.

Naturaleza original y efecto de Mint son valores separados. IVs: 0–31; EVs: 0–252 por estadística y hasta 510 en total. Hyper Training requiere nivel 50 y un IV menor de 31 en la estadística marcada. Recalcular atributos conserva el estado y los PS anteriores hasta el nuevo máximo, sin curación oculta. Las marcas de colores y el favorito son datos de organización.

PP/PP Ups y relearn son de solo lectura; no hay edición de recargas o PP ilimitados. Los objetos incluyen Megapiedras, pero el editor ofrece formas persistentes. Alpha depende del encuentro, tamaño, IVs y Plus; los tamaños son escalares, no metros ni kilos. Un título solo puede usar una cinta/marca ya obtenida. TID16/SID16 no son el número moderno de seis dígitos; los identificadores técnicos, juego de origen, huevos e historial del handler son para consulta. La fecha de encuentro validada va de 2000 a 2099.

El informe de Avanzado corresponde al Pokémon original, antes del borrador. La validación y las copias de seguridad no garantizan encuentros legítimos, funcionamiento de toda combinación en el juego ni aceptación online. Cierra siempre el juego, comprueba el resultado y conserva la copia.
