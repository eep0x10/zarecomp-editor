# ZA workbench redesign — historical visual proposal

**Current status:** implemented and shipped through 2.0.3. Current usage and actual screenshots are in the [main README](../../../README.md); executed evidence is in [EVIDENCE.md](EVIDENCE.md). The proposal below is retained as design history, including its original unverified statements.

2026-11-13. Baseline: 1.1.1, commit 3ad3232. Status: **A explicitly approved; implementation authorized with 1:1 visual fidelity requested**. Images are generated static design concepts using synthetic data, not screenshots or functional prototypes. See [implementation plan](IMPLEMENTATION.md) for current work and proof requirements.

## Job and chosen direction

Manage a ZA save quickly: recognize party/box Pokémon, select one, edit identity/stats/moves/origin, review pending changes and explicitly commit with automatic backup. The requested replacement is one local-browser workspace launched by a portable Windows EXE, dark acrylic and Pokémon themed, without application popups.

- A: vertical party + 30-slot box + inline inspector. Recommended for repeated selection and editing.
- B: horizontal party, box above a full-width editor. Wider fields, more movement between collection and editor.
- C: editor dominates, compact storage rail. Good individual customization, weaker box overview.

[Alternatives](alternatives.png), [recommended desktop](desktop-proposal.png), [narrow-screen reference](narrow-proposal.png). Desktop target 1440×1000; narrow reference 390 logical pixels. Generated image sizes are not browser viewport evidence and exact proportions/text will need adjustment during implementation.

The original direction uses Pokémon storage/party objects and a subtle Lumiose research-lab atmosphere instead of external design references. Acrylic separates collection, selection and editable detail; artwork helps recognition, not decoration behind numeric fields. Warm amber identifies selection/shiny, muted teal marks save/backup, blue-black surfaces maintain contrast. Body 14–15px, tabular numbers, compact 20–24px headings, 8px spacing rhythm; responsive grid and readable fallback surfaces when blur is unavailable. Final Pokémon sprites should use the existing artwork pipeline, not illustrations reconstructed from these mockups.

## Replacement and invariants

- Native main window → one workspace, save discovery/path selection inline, PT-BR/EN/ES.
- Details dialog → directly editable inspector, one draft per Pokémon, inline sections for all five current field groups. No intermediate apply dialog.
- Add dialog → same inspector in creation mode, explicit destination and separate add-with-backup action. Resolve pending edit drafts inline before add.
- Message boxes → inline recovery/discard choices and persistent status/backup receipt. Advanced reports expand inline; no new window.
- Optimize → editable nature suggestions with reasons; no automatic persistence.

Existing Engine remains the persistence owner: null preserves/automatic, false/zero stay meaningful; stat arrays HP/Atk/Def/Spe/SpA/SpD; star/square shiny and Mint distinctions; Alpha/HOME/IV/EV/encounter constraints; expected hashes, full-folder backups and atomic writes. No deletion/move/reorder capability is invented. Selecting or previewing never writes a save.

At desktop, party, box and selected editor remain visible; the editor may scroll internally to secondary fields. Narrow layout uses a party strip, collapsible box grid and one-column editor on the same page, with a non-obstructing save bar. One screen means one workspace, not every advanced field compressed into one viewport.

## Interaction and motion plan

Anime.js: scoped selection/preview transition; inline creation-mode expansion; subtle highlight of fields changed by Alpha or presets; filter-result changes; persistent saving/progress/success/error feedback. Update business state immediately, cancel superseded animation, clean scopes/listeners on teardown, and remove nonessential motion for changing prefers-reduced-motion. Never replay decorative entrance on background refresh. Keyboard focus, drafts and selection survive updates.

Representative first slice after approval: open synthetic save → select box Pokémon → change level/ball/IV → switch selection and back → inspect differences → save with backup → reload. Verify desktop and 390px before expanding the rest of the UI.

## Architecture and evidence

See [read-only migration review](../../audits/web-migration-review.md) for API/session/security/file/lifecycle contracts. Recommended embedded .NET local host bound to loopback only; no external hosting or Node runtime required by users. Build-time frontend tooling is separate. Keep local OpenAPI interactive docs discreetly discoverable.

Reviewed actual existing handlers, models, save service and baseline screenshots. Backend reviewer: web_architecture; frontend inventory: editor_fields. No build/runtime/security tests were performed for the proposed web app because it does not exist yet. New desktop/390px behavior, animation, accessibility and published portability: **not verified** pending implementation. Existing 1.1.1 results do not prove the migration.

## Generated image prompt set

Built-in ImageGen was used. Prompts specified (1) three structurally distinct complete desktop workspaces with identical synthetic party/box data; (2) a high-fidelity single-workspace dark acrylic Pokémon desktop with a 6×5 box, inline inspector and backup save bar; (3) a reorganized narrow version using the desktop as style reference, with a party strip and collapsible inline box grid. Constraints: no modals, marketing hero, unrelated dashboard metrics, private saves, browser/device frames or invented persistence. The images are proposal-only assets; original generated PNGs were copied into this directory without image manipulation.
