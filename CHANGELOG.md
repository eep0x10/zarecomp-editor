# Changelog

## 2.2.0 — 2026-11-14

- Add an inline team optimizer with seven combinable actions: stable recommended Mint nature, Alpha, six perfect IVs, shiny, maximum size, friendship 255 and level 100. Preview the complete batch before applying drafts; saving remains an explicit backed-up transaction.
- Stabilize nature recommendations using canonical species/form data rather than the current moveset. Preserve original nature, existing square shinies, relevant draft edits and box drafts.
- Prioritize useful natural perfect IVs for Alpha without lowering existing values; retain explicit Compatible/Custom policies and truthful reports. Reject unavailable or stale batch plans instead of applying partially.
- Show nature effects once in the colored effect line, with compact name-only option labels.

## 2.1.1 — 2026-11-13

- Preserve Pokémon identity, artwork and unchanged inspector fields after applying an Alpha plan. Nullable API patch fields mean “preserve”, rather than empty UI overrides; real zero/false/empty values remain valid edits.
- Add regression coverage from a real serialized Alpha response, including draft preservation, creation and undo.

## 2.1.0 â€” 2026-11-13

- Replace the Alpha shortcut with explicit Compatible and Custom plans for existing PokÃ©mon and new additions. Inspect origin, minimum level, evolution, required moves, size, IV and Plus changes before applying a draft; only Save/Add writes with a backup.
- Support real Alpha encounters through pre-evolutions. Preserve existing identity and show the actual PKHeX report; Custom preserves the requested origin without certifying legality.
- Make planned creation reproducible with a string-encoded seed and materialized date. New PokÃ©mon start with full HP after requested stat changes; existing HP/status are preserved.
- Preserve generated ability slots and Z-A encounter ability IDs across evolution; reconcile automatic gender for single-gender targets before ancestral encounter generation.
- Enforce Alpha IV/origin constraints for detailed edits and presets. Correct automatic shiny handling, original nature/Mint synchronization, available Mint choices and gender labels/form reconciliation.
- Prevent stale box/validation responses from replacing newer state. Correct Alpha Undo and mode-only drafts; translate readable before/after values and unavailable-plan guidance.
- Reject backup junctions before traversal and retain backup/phase receipts when a write outcome or subsequent read is uncertain.
- Add generated-save Alpha regressions, client regression checks, multilingual guides and synthetic screenshots. See the detailed [audit](docs/audits/2026-11-13-alpha-review.md) for executed evidence and limits.

## 2.0.3 â€” 2026-11-13

- Order the species grid and search results by the National PokÃ©dex number already shown on each card, rather than alphabetically.
- Refresh the three language READMEs, editing/build/test guides and real screenshots for the current feature set. Portable documentation now includes its linked guides for offline use. This documentation refresh does not change the 2.0.3 executable.

## 2.0.2 â€” 2026-11-13

- Start creation with a visible inline species picker: every species in the game's supported catalog, images, search by name or number and a direct way to change the selection.
- Load picker artwork near the visible area instead of requesting the complete catalog at once; preserve detailed creation choices when changing species.

## 2.0.1 â€” 2026-11-13

- Show an individual image for every selectable capture ball in an inline, keyboard-accessible chooser.
- Display each original/Mint nature's increased and decreased stats, including neutral natures.
- Simplify Alpha into one control that adjusts dependent size/IV values and previews compatibility without writing or replacing encounter history.
- Correct form artwork resolution and prevent failed sprite requests from becoming enlarged black pixels or permanently cached failures.

## 2.0.0 â€” 2026-11-13

- Replace the primary WinForms workflow with a portable EXE launching a single local-browser workspace: party, 30-slot boxes, inline creation and detailed inspector.
- Add the approved dark acrylic PokÃ©mon presentation, generated laboratory/emblem assets, bundled fonts and responsive inline box disclosure.
- Preserve Engine validation/backups/atomic writes; add authenticated loopback API, coherent hashes, operation receipts and local interactive OpenAPI documentation.
- Add HTTP boundary regression tests and document runtime privacy, lifecycle and fault-injection limits.

## 1.1.1 â€” 2026-11-13

- Add direct PokÃ© Ball, IV, size and Alpha controls; synchronize native/HOME size presets and clear Hyper Training with IV presets.
- Select compatible Alpha creation encounters and reconcile required Alpha Plus records for evolved encounter species.
- Improve documentation and real synthetic-save screenshots.

## 1.1.0 â€” 2026-11-10

- Add a shared detailed editor for existing party/box PokÃ©mon and custom creation, with identity, attributes, moves, origin and advanced tabs.
- Separate original nature/Mint effects and IV/Hyper Training data; add custom shiny style, item, friendship, trainer/encounter fields, size, organisation markings and existing affixed titles.
- Preserve unrelated fields through nullable patches, including minted nature and square shiny status. Quick edits and detailed drafts can be combined before one backed-up write.
- Validate species/form/gender, move/Plus compatibility, EV totals, Hyper Training, Alpha/size and metadata constraints. Show unsupported PP/history and technical identifiers for inspection.
- Extend PT-BR/English/Spanish UI, documentation and regression coverage.

## 1.0.0 â€” 2026-11-09

- Initial portable Windows release with party/box browsing, quick level/nature/shiny editing, custom additions, nature suggestions and automatic backups.
