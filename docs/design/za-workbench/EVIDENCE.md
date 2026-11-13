# Workbench implementation evidence

## First slice checkpoint â€” 2026-11-13

Reviewer: main, integrated browser/CUA. Fixture: synthetic copy under `C:/Demo/ZAEditorWeb20`; no personal save writes. Reference: approved desktop-proposal.png and narrow-proposal.png. Host: Debug .NET10, frontend checkpoint bundle Dr_K8yrK (focused touch correction). This is intermediate evidence, not a release verdict.

| Scenario | View | Observed | Result |
|---|---|---|---|
| Open synthetic save and select PokÃ©mon | Desktop 1488Ã—1058 | Party, 30 slots and editor share one workspace; actual data replaces mockup values | pass |
| Change Charizard level62â†’63 and ballâ†’Ultra Ball; select Greninja/back | Desktop | Draft values retained and pending count1 | pass |
| Validate draft | Desktop | Engine preview reported valid; no save write during preview | pass |
| Save and reopen | Desktop | UI confirmed saved, draft count reset, actual full-folder backup receipt; level63 and Ultra Ball persisted | pass |
| Artwork | Desktop | Initial blob images failed because CSP omitted blob; corrected host, decoded image widths475 observed | corrected/pass |
| Mobile control targets | 390Ã—1000 | Initial numeric36px/HT18px failed; after fix numeric min44, HT wrapper44Ã—67, save44 | corrected/pass |
| Mobile reflow and box expansion | 390Ã—1000 | Width375 within viewport390; 30-slot inline expansion, no horizontal page overflow | pass |
| Reduced motion changes dynamically | 390Ã—1000 | Browser emulated reduce=true; selecting Bulbasaur updated detail without blocking; emulation restored | pass for state/access; motion execution needs final review |
| Keyboard focus | Narrow | Tab reached input with visible solid focus outline | pass for representative transition |

Visual judgment: three-column hierarchy and dark material direction match the approved composition. Font sizes, complete field groups and final screenshots remain subject to refinement. First narrow checkpoint capture: [checkpoint-narrow.jpg](../../screenshots/web/checkpoint-narrow.jpg); it predates touch correction and is not a final product image.

Independent code review found and owners are resolving: ambiguous/transport commit outcomes, edits made during commit, session cleanup and cross-session box races, strict DTO projection for Optimize. These need final consumer review and targeted checks. Opening a browser or a mockup is not evidence of those fixes.

Backend owner reports 39 HTTP checks passed: bearer/Origin/Host, cookie refresh, malformed/null DTOs, stale hash, preview/no-write, Save/Add backups and receipts, embedded assets, restricted runtime token file, heartbeat and shutdown. See [host verification](../../../WebHost/VERIFICATION.md). These do not cover all fault injections; limits are recorded there.

## Asset provenance

Built-in ImageGen produced `WebClient/public/assets/laboratory-night.png` (dark defocused Lumiose-inspired lab/city backdrop, no UI/text/characters) and `pokeball-mark.png` (single dimensional PokÃ© Ball, transparent canvas). Originals were copied without image manipulation. Prompts specified restrained blue-black/teal with warm distant light, readable low-contrast background and emblem clarity at small size. These are illustrative generated assets, not official artwork. PokÃ©mon entities use the existing optional official-artwork source through the local authenticated cache.

## Completed integration acceptance (before packaging)

- Root executed a fresh generated-save HostTests run: **39/39 PASS**, exit0. No external save input was required. Frontend TypeScript/Vite production build passed. Runtime npm audit reported no known production vulnerabilities at this check; that is not a malware guarantee.
- All five groups were inspected in the browser. Custom creation: shiny Star Charmander25, Modest, Ultra Ball, WebAlpha nickname, Alpha/Scale255 and IVs31/31/31/17/19/21 persisted and reopened with a valid report.
- Detailed edits persisted: Ember in move1, friendship123, encounter date2026-11-12 changed through keyboard interaction. A mixed party/box batch committed with a backup and preserved the selected box. A favourite flag produced an upstream legality warning, which the UI disclosed; clearing it restored that fixture's valid report. Such metadata edits are not a promise of online legitimacy.
- Optimize with a nickname draft succeeded; six named nature suggestions and individual reasons appeared inline without a save. Discard/reopen confirmations stayed inline. Invalid level101 retained the draft and showed a localized constraint.
- English and Spanish switching preserved the displayed save/selection, while original English nature/move names stayed unchanged. F5 reconnected using the existing session ID (observed GET session, no new POST session).
- A browser-only blocked commit request produced a transport failure. The draft remained, another write was disabled, and explicit inline reopen/discard recovered the unchanged level63. This simulates a failed request, not disconnection after a real Engine commit.
- Motion was observed with temporary no-preference emulation: preview opacity0.9722/translateY0.3092 reached opacity1/translateY0; rapid reversal ended on the correct PokÃ©mon. The user's default reduce preference was observed as true and restored after the test. No browser/network/emulation overrides were left active.
- Final desktop1488Ã—1058 had scrollHeight1058, retained header/save action and used inspector scrolling. Creation's primary Add action remained visible. Final390px check had width375 within390; visible enabled buttons/numeric/select controls were at least44px after two targeted corrections. Hyper Training uses a44px label hit target. The viewport override was reset.
- Independent static review found no remaining high-impact blocker after fixes to uncertain outcomes, DTO projection and response/session races. Independent image review approved composition and main field legibility; the apparently clipped status was recaptured settled and measured scrollHeight=clientHeight48 with scrollTop0.

Final captures: [desktop](../../screenshots/web/workbench-desktop.jpg), [creation](../../screenshots/web/workbench-add.jpg), [moves](../../screenshots/web/workbench-moves.jpg), [origin](../../screenshots/web/workbench-origin.jpg), [narrow](../../screenshots/web/workbench-narrow.jpg). All use synthetic data. Initial failed checkpoint images are historical evidence, not release screenshots.

Portable EXE/archive/download/CI verification is the final packaging step, recorded with the release. No actual-game or personal-save mutation was performed. Engine crash/power-loss injection, actual in-flight disconnect and uncooperative external-writer races were not fault-injected; see host limits.

## 2.0.1 â€” focused controls and artwork refinement

The approved workspace composition is retained. Capture balls use one inline listbox with roving keyboard focus, 25 bundled item PNGs and separate focused/selected states. Opening uses a scoped Anime.js opacity/translation effect (160ms); reduced motion skips movement, cleanup reverts the scope. Nature and Mint labels disclose Â±10% stats or neutral 0%. Alpha prepares dependent size/IV values and immediately requests a non-writing preview; it preserves original encounter history and explains incompatible encounters inline.

Executed on a separate synthetic fixture:

- 67 Engine detailed checks passed. Backend owner reports 32 deterministic sprite checks, 24 live exact-form/variant checks and 39 generated-save HTTP checks; see WebHost/SPRITE-REVIEW.md.
- Browser at default 1280Ã—720: all 25 ball PNGs decoded at 30Ã—30; keyboard End moved focus to Beast Ball without changing PokÃ© Ball, Escape closed and restored focus. Hardy displayed neutral 0%; Modest displayed +10% Sp. Atk/âˆ’10% Attack. The increased/decreased values remain visible outside the native select.
- One-click Alpha preview on normal Ivysaur succeeded. Luxury Ball, Modest original nature, Timid Mint and Alpha were saved with a backup and reopened after F5. An independent API read confirmed legal=true, Scale255, IVs31/12/3/5/31/31, nature15, statNature10 and ball11.
- Incompatible Charmander10 Alpha displayed the specific encounter guidance and retained the draft. Turning it off restored size editing; discarding stayed inline. No personal save was used.
- At390Ã—1000, content width375 did not overflow. Ball targets were approximately68Ã—70. Touch selection of Beast Ball worked; Spanish showed localized stat effects while English nature/ball names stayed intact. The temporary viewport and media overrides were restored.
- Vivillon form0 rendered as Icy Snow at475px in both box and inspector. A blocked image request produced an explicit #0666 unavailable state, with no scaled tiny image. This test exposed a mounted-consumer recovery gap; the final correction/retest is recorded below.
- The ball-opening interaction reached its final state with normal motion and reduced-motion settings; no intermediate frame of the new 160ms effect was captured. Source integration/cleanup was independently reviewed. Earlier general workspace animation evidence remains above.

Screenshots use synthetic data: [desktop controls](../../screenshots/web/controls-2.0.1-desktop.jpg), [390px controls](../../screenshots/web/controls-2.0.1-narrow.jpg).

Final recovery retest passed on bundle index-D7aDvq8y.js: with the Vivillon route blocked after reload, both mounted views reached #0666 unavailable; after unblocking and reselecting, box and inspector both decoded475px without F5. Rapid selection ended on Vivillon, all25 ball PNGs decoded and no broken sprite images remained. Shared subscriptions are keyed by species/form/shiny and cleaned on unmount. Final screenshots above were recaptured from this build. Network/media/viewport overrides were restored and the QA tab/host closed.

## 2.0.2 â€” species picker

Creation opens the visible Choose PokÃ©mon inline grid automatically. It uses all 364 species in the current Engine catalog, with name/ID search, images loaded near the visible area, keyboard selection and an explicit return/toggle. The accepted workspace composition is retained; Anime.js scopes handle disclosure/filter feedback and reduced motion, with cleanup. Selection uses the readonly forms list and compatible metadata; it resets moves/Plus to automatic while retaining other custom choices.

Executed: frontend build and nine helper checks passed; backend40HTTP+32sprite checks passed. Independent static review found no blocker. Root CUA default1280Ã—720 and390Ã—1000 verified364 tiles, search Eevee and #025, Enter selection, touch selection and changing species while level22 remained. Only5â€“9 picker images were instantiated near the viewport rather than364 network requests. Narrow content width375 fit390; targets95Ã—108. Synthetic fixture hash stayed unchanged; no write action was clicked. Viewport reset, QA tab/host closed. Existing main-workspace motion evidence applies; no intermediate frame of the new picker effect was captured.

Captures: [desktop picker](../../screenshots/web/species-picker-2.0.2.jpg), [narrow picker](../../screenshots/web/species-picker-2.0.2-narrow.jpg). Synthetic data only.

2.0.3: species grid/search now use a copied array sorted by National PokÃ©dex ID (the number printed on cards). Four focused shuffled-input/filter/non-mutation checks and frontend build passed. Layout/controls are unchanged.

Documentation refresh: five current-*.jpg captures show the shipped2.0.3 build with synthetic data. Desktop captures use1440Ã—1000; narrow capture uses390px with375px content width. All temporary viewport overrides and QA tabs/host were cleaned up; fixture hash remained unchanged. Repository and staged offline documentation passed116 relative-link/asset checks. The linked distribution guides are now copied by scripts/Copy-DistributionDocs.ps1, which was parsed and executed. Runtime source and executable are unchanged from commit5ae7530; documentation/packaging scripts and screenshots were refreshed separately.

## 2.1.0 â€” Alpha plans and application review

This records executed local integration evidence on generated/copied synthetic fixtures. No personal save was written and no actual game was launched. Final local source verification passed **61 Alpha, 44 generated-save HTTP, 67 Engine and 39 client regression checks**, including the final generation fixes; remote CI is separate. The 32 deterministic sprite and 24 live-image results above remain historical, unchanged coverage.

- Latest root CUA: Chikorita level 5 opened a Compatible plan proposing level 25 plus explicit origin/four-move changes. After review â†’ Apply â†’ Save with backup â†’ reload â†’ Validate, PKHeX reported valid. Planning and applying were distinct from the backed-up write.
- Custom Mewtwo level 42 displayed the actual incompatibility findings in the preview. In the preceding round, Custom Mewtwo level 1 was added and reopened with Alpha true and legal false; the invalid report was shown rather than suppressed.
- Earlier compatible creation: Charmander level 10 proposed level 25; backed-up Add/reopen returned Alpha true and legal true.
- Existing Alpha undo retained unrelated values. The IV 0 preset was disabled for Alpha, and direct IV input that would leave fewer than three perfect IVs was rejected. Mint-follow behaviour was exercised.
- English/Spanish switching preserved the workflow. At a 390px viewport, document width measured 390px with no horizontal page overflow. These are representative interactions, not exhaustive keyboard/screen-reader/device coverage.
- The UI shows the running host version. Compatible plans disclose proposed origin/level/required-move changes, scalars, IVs/Hyper Training and Plus records before applying to a draft. Explicit Custom retains origin and shows the true report; neither mode promises legality or online acceptance.

Final review reproduced and fixed Gallade ancestral automatic-gender failures and gift ability-slot/RNG correlation. Twelve fixed seeds each now pass; the canonical Z-A encounter ability helper also preserves the ancestral-only Annihilape case. See the [audit](../../audits/2026-11-13-alpha-review.md) for exact scope and limits.

Real 2.1.0 captures with synthetic data: [Compatible plan](../../screenshots/web/alpha-plan-2.1.0.jpg), [Custom plan/report](../../screenshots/web/alpha-custom-2.1.0.jpg), [390px layout](../../screenshots/web/alpha-narrow-2.1.0.jpg). Existing current-*.jpg images remain labelled as general-workspace build 2.0.3 captures.

Limits: no actual-game or personal-save mutation, clean no-SDK machine, power-loss/crash injection, actual in-flight disconnect during an Engine commit, or uncooperative external-writer race. Candidate origin search is bounded to 24. Source tests, CUA states, future published CI and final portable/archive verification are separate evidence. Guided restore, generic per-record draft diff/undo, movement/reorder and validated import/export remain recommendations, not delivered functionality.
