# Alpha conversion and application review â€” 2026-11-13

Final local source verification passed **61 deterministic Alpha, 44 generated-save HTTP, 67 Engine and 39 client regression checks**. The final Alpha suite includes the two generation repairs below. All write tests used generated/copied synthetic saves; no personal save or actual game was written or launched. Remote CI and portable publication are verified separately with the release. The earlier 32 deterministic sprite and 24 sampled live-image checks are historical at this source checkpoint.

## Cause and implemented conversion contract

The earlier UI set Alpha, Scale 255 and three perfect IVs while preserving an origin that could still describe a non-Alpha starter/gift. A higher battle level does not repair that history. Low-level creation could also fall below the available Alpha template. Both paths were reproduced; unrelated legality findings were kept distinct from the Alpha failure.

The owning rules are the vendored PKHeX/PA9, encounter, RNG and Engine sources. Alpha is a stored flag with size, natural-IV, encounter and Plus dependencies, not a species or temporary Mega form. Native Z-A height/weight scalars stay zero; HOME-linked records synchronize them to Scale. Hyper Training cannot substitute for the required three natural IVs of 31.

- **Compatible encounter** preserves a suitable origin or proposes a supported Alpha origin. The explicit diff includes encounter location/level, obedience, current level/evolution requirements, necessary move corrections, scalars, IVs/Hyper Training and Plus records. Existing species, PID, encryption constant, trainer and tracker are retained. Candidate search is bounded to 24 ordered origins; failure to find a plan is not an exhaustive proof that no theoretical reconstruction exists.
- **Custom: keep encounter** is explicit. It retains requested provenance and enforces Alpha size/IV/Plus dependencies while showing the actual PKHeX report, including incompatible encounter/RNG findings. It never fabricates a legal result or certifies online acceptance.
- Planning writes nothing. Apply changes only the draft; the normal expected-hash Save/Add transaction creates a backup and performs the write. Cancel leaves the draft unchanged. Revert restores still-matching planned fields without replacing later unrelated edits. Disabling Alpha clears its policy and retains other applied values.
- Creation carries a string-encoded seed and materialized encounter date so preview and Add can reproduce the same identity. New PokÃ©mon are healed after requested stat changes; existing edits preserve HP/status within the new maximum. Full round-trip data matters, not just the Alpha flag.

## Corrected findings and evidence

| ID | Finding | Correction and executed evidence |
| --- | --- | --- |
| A1 | Starter/gift Alpha rejected by its original encounter | Compatible/custom plans; CUA Chikorita level 5 â†’ compatible level 25 with explicit origin/four-move diff â†’ Apply â†’ backup Save â†’ reload/Validate reported PKHeX valid |
| A2 | Alpha creation below an encounter minimum | Plan proposes the minimum; earlier CUA Charmander level 10 â†’ 25 â†’ Add/reopen returned Alpha true and legal true |
| A3 | IV-only edits bypassed the three-perfect-IV requirement | Engine enforces the invariant; client disables IV 0 preset and rejects a direct edit leaving fewer than three perfect IVs; existing Alpha flag retained in undo/review checks |
| A4 | Final-species-only encounter search misses evolved Alpha origins | Source now searches ancestral encounters with evolution-level requirements and necessary move corrections in the diff; the Gallade automatic-gender regression below now passes |
| U1 | Automatic shiny null could become shiny true | Null/default conversion corrected and covered by client regressions |
| U2/U3 | Late box/validation responses could overwrite newer context | Session/hash/selection/revision guards cover success and failure; client regressions exercise delayed-response context handling |
| U4 | Unavailable neutral Mint choices and effective-nature drift | Effective choices filtered, Serious neutral available, no-override nature follows original; explicit valid override retained; browser Mint-follow check passed |
| U5 | Form change could retain incompatible gender | Form metadata reconciles invalid gender; regression coverage added |
| B1 | Recursive backup could descend into junctions | Reject reparse entries before descent; persistence regression coverage uses synthetic directories |
| B2 | A post-write failure could lose the known backup/outcome | Phase/backup information retained in the uncertain receipt; API/UI require reconciliation rather than automatic retry |
| X1 | Browser did not identify an older active executable | Workspace displays host bootstrap version; 2.1.0 verified in the browser |

## Final seeded-generation repairs

Both late findings were reproduced and repaired before the final source run:

1. **Gallade ancestral Alpha with automatic gender:** seeds 5, 6, 7, 10 and 11 conflicted while an explicitly male request succeeded. Creation now derives the final species' single supported gender before encounter criteria; all twelve fixed seeds 0–11 produce an available male plan. Explicit incompatible choices remain validation errors.
2. **Gift ability-slot/RNG correlation:** creation unconditionally forced ability slot 0. It now preserves the generated AbilityNumber and uses PKHeX's canonical `AbilityChangeRules.GetExpectedAbilityZA` to retain the expected encounter ability ID across evolution. All twelve gift seeds 0–11 have a valid actual PKHeX report. The ancestral-only Annihilape case also passes; simply refreshing the evolved species' ability had broken that case.

The final run passed 61 Alpha + 44 HTTP checks; 67 detailed Engine checks and the 39 client checks also passed. The verifier reviewed the focused fixes independently. Tests are deterministic synthetic-data evidence, not game/online certification.

## Browser evidence on synthetic fixtures

Root CUA verified the local 2.1.0 host in desktop and a 390px viewport. Latest flow: Chikorita 5 â†’ Compatible 25, reviewed origin and four changed moves, Apply, Save with backup, reload and Validate returned PKHeX valid. The fresh baseline had an unrelated RNG/ability finding; this does not establish that the starting fixture was fully legal. Custom Mewtwo level 42 showed actual incompatible findings in the plan. English/Spanish switching retained the workflow and the plan labels were confirmed. At 390px, document width was 390px with no horizontal page overflow; the captured plan remained readable.

The earlier integration round additionally exercised Charmander 10 â†’ compatible 25 â†’ backed-up Add/reopen with Alpha true/legal true; Custom Mewtwo level 1 â†’ Add/reopen with Alpha true and actual legal false; existing Alpha undo, disabled IV 0 preset/direct-IV rejection and Mint following the original nature.

Real captures of synthetic data: [Compatible plan](../screenshots/web/alpha-plan-2.1.0.jpg), [Custom plan/report](../screenshots/web/alpha-custom-2.1.0.jpg), [390px layout](../screenshots/web/alpha-narrow-2.1.0.jpg). Screenshots establish the captured states, not exhaustive species/image/device coverage. General-workspace 2.0.3 captures remain separately labelled.

## Verification boundaries

Local builds and synthetic-save regressions do not establish real-game playability, online acceptance, a successful public download, remote CI, or operation on a clean no-SDK Windows machine. No personal-save write or actual-game test occurred. The 24-origin bound and existing RNG constraints can prevent a Compatible plan; Custom exposes rather than hides that incompatibility.

Power-loss/crash during replacement, an uncooperative external writer racing the final check/replace, and an actual in-flight network disconnect during a real Engine commit were not verified. Structured failure injection and a blocked browser request are narrower evidence. Backup restore remains manual. Sprite service availability and every exact species/form/shiny image were not exhaustively retested.

## Recommendations outside this correction

These are review recommendations, not implemented features:

- Add a guided restore screen that displays backup date/hash and previews the target before any replacement; retain a copy of the current main file.
- Add per-PokÃ©mon draft diff/undo with explicit dependent fields, rather than extending the Alpha-specific undo into an implicit global history.
- Before party/box moves or reorder, define full-party, occupied-destination and preservation rules and verify encrypted round trips.
- Before import/export or duplicate workflows, define source-game compatibility and identity/tracker handling; reject unsupported history rather than silently converting it.
- Extend the fixed-seed species/form matrix beyond the repaired Gallade/gift cases. Existing manual species/form edits can still produce compatibility findings; review HOME/form/ability history before adding automatic repair. Keep deterministic cases in CI and distinguish live artwork probes from required tests.
