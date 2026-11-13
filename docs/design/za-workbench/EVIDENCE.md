# Workbench implementation evidence

## First slice checkpoint — 2026-11-13

Reviewer: main, integrated browser/CUA. Fixture: synthetic copy under `C:/Demo/ZAEditorWeb20`; no personal save writes. Reference: approved desktop-proposal.png and narrow-proposal.png. Host: Debug .NET10, frontend checkpoint bundle Dr_K8yrK (focused touch correction). This is intermediate evidence, not a release verdict.

| Scenario | View | Observed | Result |
|---|---|---|---|
| Open synthetic save and select Pokémon | Desktop 1488×1058 | Party, 30 slots and editor share one workspace; actual data replaces mockup values | pass |
| Change Charizard level62→63 and ball→Ultra Ball; select Greninja/back | Desktop | Draft values retained and pending count1 | pass |
| Validate draft | Desktop | Engine preview reported valid; no save write during preview | pass |
| Save and reopen | Desktop | UI confirmed saved, draft count reset, actual full-folder backup receipt; level63 and Ultra Ball persisted | pass |
| Artwork | Desktop | Initial blob images failed because CSP omitted blob; corrected host, decoded image widths475 observed | corrected/pass |
| Mobile control targets | 390×1000 | Initial numeric36px/HT18px failed; after fix numeric min44, HT wrapper44×67, save44 | corrected/pass |
| Mobile reflow and box expansion | 390×1000 | Width375 within viewport390; 30-slot inline expansion, no horizontal page overflow | pass |
| Reduced motion changes dynamically | 390×1000 | Browser emulated reduce=true; selecting Bulbasaur updated detail without blocking; emulation restored | pass for state/access; motion execution needs final review |
| Keyboard focus | Narrow | Tab reached input with visible solid focus outline | pass for representative transition |

Visual judgment: three-column hierarchy and dark material direction match the approved composition. Font sizes, complete field groups and final screenshots remain subject to refinement. First narrow checkpoint capture: [checkpoint-narrow.jpg](../../screenshots/web/checkpoint-narrow.jpg); it predates touch correction and is not a final product image.

Independent code review found and owners are resolving: ambiguous/transport commit outcomes, edits made during commit, session cleanup and cross-session box races, strict DTO projection for Optimize. These need final consumer review and targeted checks. Opening a browser or a mockup is not evidence of those fixes.

Backend owner reports 39 HTTP checks passed: bearer/Origin/Host, cookie refresh, malformed/null DTOs, stale hash, preview/no-write, Save/Add backups and receipts, embedded assets, restricted runtime token file, heartbeat and shutdown. See [host verification](../../../WebHost/VERIFICATION.md). These do not cover all fault injections; limits are recorded there.

## Asset provenance

Built-in ImageGen produced `WebClient/public/assets/laboratory-night.png` (dark defocused Lumiose-inspired lab/city backdrop, no UI/text/characters) and `pokeball-mark.png` (single dimensional Poké Ball, transparent canvas). Originals were copied without image manipulation. Prompts specified restrained blue-black/teal with warm distant light, readable low-contrast background and emblem clarity at small size. These are illustrative generated assets, not official artwork. Pokémon entities use the existing optional official-artwork source through the local authenticated cache.

## Completed integration acceptance (before packaging)

- Root executed a fresh generated-save HostTests run: **39/39 PASS**, exit0. No external save input was required. Frontend TypeScript/Vite production build passed. Runtime npm audit reported no known production vulnerabilities at this check; that is not a malware guarantee.
- All five groups were inspected in the browser. Custom creation: shiny Star Charmander25, Modest, Ultra Ball, WebAlpha nickname, Alpha/Scale255 and IVs31/31/31/17/19/21 persisted and reopened with a valid report.
- Detailed edits persisted: Ember in move1, friendship123, encounter date2026-11-12 changed through keyboard interaction. A mixed party/box batch committed with a backup and preserved the selected box. A favourite flag produced an upstream legality warning, which the UI disclosed; clearing it restored that fixture's valid report. Such metadata edits are not a promise of online legitimacy.
- Optimize with a nickname draft succeeded; six named nature suggestions and individual reasons appeared inline without a save. Discard/reopen confirmations stayed inline. Invalid level101 retained the draft and showed a localized constraint.
- English and Spanish switching preserved the displayed save/selection, while original English nature/move names stayed unchanged. F5 reconnected using the existing session ID (observed GET session, no new POST session).
- A browser-only blocked commit request produced a transport failure. The draft remained, another write was disabled, and explicit inline reopen/discard recovered the unchanged level63. This simulates a failed request, not disconnection after a real Engine commit.
- Motion was observed with temporary no-preference emulation: preview opacity0.9722/translateY0.3092 reached opacity1/translateY0; rapid reversal ended on the correct Pokémon. The user's default reduce preference was observed as true and restored after the test. No browser/network/emulation overrides were left active.
- Final desktop1488×1058 had scrollHeight1058, retained header/save action and used inspector scrolling. Creation's primary Add action remained visible. Final390px check had width375 within390; visible enabled buttons/numeric/select controls were at least44px after two targeted corrections. Hyper Training uses a44px label hit target. The viewport override was reset.
- Independent static review found no remaining high-impact blocker after fixes to uncertain outcomes, DTO projection and response/session races. Independent image review approved composition and main field legibility; the apparently clipped status was recaptured settled and measured scrollHeight=clientHeight48 with scrollTop0.

Final captures: [desktop](../../screenshots/web/workbench-desktop.jpg), [creation](../../screenshots/web/workbench-add.jpg), [moves](../../screenshots/web/workbench-moves.jpg), [origin](../../screenshots/web/workbench-origin.jpg), [narrow](../../screenshots/web/workbench-narrow.jpg). All use synthetic data. Initial failed checkpoint images are historical evidence, not release screenshots.

Portable EXE/archive/download/CI verification is the final packaging step, recorded with the release. No actual-game or personal-save mutation was performed. Engine crash/power-loss injection, actual in-flight disconnect and uncooperative external-writer races were not fault-injected; see host limits.
