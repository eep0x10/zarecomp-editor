# ZA web workbench implementation plan

> Executors use the approved visual proposal and implement each slice with concrete evidence; preserve concurrent file ownership.

**Goal:** replace the WinForms-facing workflow with the approved single-screen local-browser workbench while retaining portable distribution and Engine save protections.

**Approval:** user explicitly approved A on 2026-11-13 and requested 1:1 visual fidelity to desktop-proposal.png. This supersedes the proposal's pending status. Numeric/content details remain real Engine values rather than baked-in mockup text.

**Architecture:** React/TypeScript/Vite UI built into embedded WebHost resources. .NET10 loopback-only Kestrel launches the browser. Engine remains sole persistence owner. No Node, hosting account or .NET installation for users of the portable package.

**Spec:** README.md and ../../audits/web-migration-review.md. Original WinForms App remains a source fallback, not a second competing workflow in the new package.

## Global constraints

- One workspace, inline creation/reports/discard/recovery, no application dialogs or browser alerts.
- Exact approved layout/material hierarchy, dark acrylic, Pokémon artwork, 6×5 storage grid, party rail, details inspector, persistent save bar.
- Full prior field coverage; nullable overrides, Mint/shiny variants and all Engine validations survive.
- Keep automatic full-folder backups, expected hashes, atomic commits; never test writes on personal saves.
- PT-BR/EN/ES; move and nature identifiers/names remain original English.
- Anime.js meaningful scoped motion, cleanup, rapid reversal and reduced-motion support.
- API only loopback, bearer session in browser memory, Origin/Host validation, no arbitrary shell or filesystem browser.

## Ownership and execution slices

1. **Host/API — web_architecture:** WebHost/** except generated wwwroot, HostTests/**. Session/open/discovery/catalog/read/preview/edits/add/suggestions/preferences/lifecycle/interactive OpenAPI; auth/idempotency/coherent hashes; bounded artwork byte cache. Compile and HTTP/Engine synthetic tests.
2. **Frontend — editor_fields:** WebClient/** and generated WebHost/wwwroot. First complete open/select/edit/preview/save slice and representative desktop/390px render before expanding all detailed groups. Complete creation, Optimize, all overrides, reports and recovery. npm build/type checks and interaction evidence.
3. **Assets/integration — main:** generate laboratory background and emblem, integrate font/art licensing, update build.ps1 and package artifacts, review APIs/contracts, own integrated-browser validation and final portable smoke tests. Preserve existing user save and desktop shortcut until final build verified.
4. **Independent review — verifier:** once working artifact exists, inspect integration, auth/path/write boundaries and full capability coverage; distinguish executed tests from static review.

## Review focus and proof

- Stale file or response: preserve drafts and reject writes to mismatched hash; synthetic HTTP tests.
- Add retry/disconnect: operation receipt prevents duplicate additions; never silently retry uncertain commits.
- Defaults/zero/false and Mint/shiny: UI field changes serialize only actual intended overrides; engine reload assertions.
- Long content, empty slots and narrow view: 390px reflow, 44px touch controls and no hidden save action; integrated-browser evidence.
- Closing/relaunching browser and offline/missing artwork: lifecycle does not interrupt commit; controls remain useful without network.

## Acceptance sequence

- [x] Frontend and host agree exact DTOs before consumers expand.
- [x] First real slice: open synthetic save, edit one Pokémon, change selection/back, preview, save with backup and reload.
- [x] Desktop +390px checkpoint, visible focus, reduced motion and repeated selection interruption.
- [x] Expand all field groups/Add/Optimize/report, validate error/stale/discard recovery.
- [x] Independent review and targeted regressions after fixes.
- [ ] Build self-contained 2.0.0 EXE with embedded assets, verify archive/source, launch portable and use synthetic save without Node runtime dependency.
- [ ] Publish authorized repository/release/docs/screenshots, verify download and CI; update existing desktop shortcut to verified new portable.

Generated mockups are visual targets, not evidence of any checked acceptance item. Record actual tests and screenshots in this directory or the existing private task artifact path.
