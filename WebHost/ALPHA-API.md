# Alpha planning and backend audit — 2.1.0

2026-11-13. Technical contract and regression evidence for the 2.1.0 Alpha workflow. All executed save writes used generated or approved synthetic copies. No personal save or game process was modified.

## Confirmed failures

The previous toggle set Alpha/size/IV dependencies but did not adapt an incompatible gift/starter origin. The Engine rejected it with `StatAlphaInvalid` because the matched encounter remained non-Alpha. Native PKHeX `MiscVerifierPA9` checks both the Alpha flag against the matched encounter and the HOME Alpha scalar requirements. The old Add path searched only direct-species encounters below the requested level, so low-level Alpha creation failed rather than proposing the minimum level, and could miss an Alpha available through pre-evolution.

The original direct Add behavior is deliberately retained when no plan is accepted: a low-level request with `IsAlpha=true` and no explicit custom policy still rejects rather than secretly changing level/origin. The new flow is plan → inspect changes/report → explicitly apply to draft → explicit Save/Add.

## API contract

All paths below are relative to `/api/v1/sessions/{id}` and retain the existing runtime authentication, exact Origin/JSON requirements and coherent expected hash checks.

- POST `/alpha-plan`: `{expectedHash, party, slot, patch, mode}`.
- POST `/alpha-plan-add`: `{expectedHash, options, mode}`.

`mode` is `Compatible` or `Custom`. Response:

```text
AlphaPlan {
  available, code, hash, mode, minimumLevel: number|null,
  requiresOriginChange,
  patch: PokemonPatch|null, options: AddOptions|null,
  preview: PokemonDetails|null,
  changes: [{field, before: string, after: string}], warnings: string[]
}
```

Available codes are `alpha_compatible_plan` and `alpha_custom_plan`. Unavailable codes are `alpha_origin_unavailable`, `alpha_encounter_unavailable` and `alpha_constraints_conflict`. An unavailable plan does not change a draft or save. It may provide a minimum level and explain that another selection or explicit Custom mode is necessary.

The returned patch includes the incoming user patch plus required Alpha/scale/IV/Hyper Training adjustments and any proposed level/met location/met level/obedience changes. Met location differences include the catalog name and numeric ID. Changes also compare actual Plus records and PID/EC. Existing Pokémon are cloned for analysis and keep their PID, EC, trainer identity, tracker, unrelated metadata and HP/status. Compatible planning can adapt supported origin fields using real current-species or pre-evolution Alpha encounters. Evolution thresholds are included: a level-35 Charmander encounter does not justify a level-35 Charizard; its proposed current level is at least 36.

If an adjusted origin makes a move slot invalid and PKHeX identifies an explicit expected move, the Compatible plan proposes that replacement in `patch.moves` and the before/after diff. It leaves already valid slots alone. Plus records and calculated PP are shown as dependent changes. This addresses the demonstrated level-5 Chikorita gift → level-25 Alpha origin case, where the new encounter required four initial moves. Custom mode does not perform this origin-dependent repair. No replacement is silently committed; applying the plan remains explicit.

Compatible means an Alpha origin can be matched under these constraints. It does **not** certify the entire Pokémon as globally legal or manufacture a positive result. The real PKHeX report and warnings remain visible, including any RNG correlation or other unrelated issues. Origin search considers at most 24 prioritized candidate templates; unsuitable constrained templates are skipped. The database and bounded search do not prove every possible provenance for every species.

Custom explicitly applies Alpha with a preserved/requested origin while allowing only the Alpha encounter-mismatch check to fail. It never relaxes the format constraints: Scale 255, at least three natural IVs 31, native height/weight zero or HOME scalars matching Scale, compatible Hyper Training flags, normal value/array bounds and protected metadata. It does not forge an encounter or alter PKHeX analysis. Mewtwo at level 1 can therefore be an explicitly custom Alpha with a truthful incompatible report.

`PokemonPatch.alphaMode` is nullable `Compatible|Custom`; omitted/null preserves the existing strict behavior. Mode-only patches are accepted when the effective Alpha flag (`patch.isAlpha ?? stored.isAlpha`) is true. `isAlpha:false` must omit/clear `alphaMode`. The mode is a validation policy, not a new persisted game field; snapshot policy is inferred from the actual Alpha encounter mismatch. A reopened incompatible custom Alpha exposes `values.alphaMode: Custom`; edits to its origin need that explicit policy retained. Nickname-only patches do not reinterpret its origin.

### Deterministic creation

Returned AddOptions contain `creationSeed`, an **unsigned 64-bit decimal JSON string**, and a materialized `details.metDate` plus compatible origin fields. Keep the seed as a string in browser memory and pass the returned options to subsequent Preview/Add. Do not convert it to a JavaScript Number. The same options reproduce the same Pokémon bytes. Changed nature/gender/shiny options are used for encounter filtering and generation criteria; an incompatible request returns a real validation/plan error rather than silently discarding those choices. A new accepted plan may supply another seed. Existing Pokémon conversion does not regenerate identity.

The shared `BuildNewPokemon` applies customization and then resets the **new** Pokémon's final party stats/health. This avoids template IV randomness leaving newly added Pokémon partially hurt after their final IVs are applied. Existing edits continue preserving HP, status and PP except their documented dependent calculations.

## Additional demonstrated audit fixes

1. An IV-only edit previously missed the Alpha invariant guard and could reduce an existing Alpha to six zero IVs. IV, relevant origin, identity and mode changes now enter the appropriate invariant checks. Compatible Alpha cannot silently move to an incompatible gift origin; explicit Custom policy remains available with a truthful report.
2. Backup previously called recursive GetFiles before rejecting reparse points. It now walks top-down, checks each entry before descending and rejects a recursive junction without traversing it. Full-folder verified backups remain mandatory.
3. `SaveCommitException` carries `backupDirectory`, `phase` and `mayHaveCommitted` once a verified backup exists. The host caches the failure receipt even when the current save cannot be reread, retaining `hash:null` if needed. It never loses known recovery information while attempting another failing hash read. Receipt replay does not invoke the write again.

Failure receipts can contain `{committed:false, outcomeUnknown, refreshRequired:true, hash:null|string, backupDirectory, phase}`. `phase` is `temporary_write`, `replace` or `final_verification`. Display the backup before branching on failure. Unknown replacement outcome requires reload/recovery review, not an automatic retry. A final-verification failure was simulated through the host write delegate for receipt testing; crash/power-loss and real-media post-replacement faults were not injected.

Spanish nature-recommendation explanations now retain the requested English nature names Adamant and Modest. No recommendation heuristic was changed.

Final generation regressions reproduced and corrected: a forced primary ability slot after RNG made some otherwise valid Chikorita gifts fail correlation; generation now preserves the selected RNG ability number and derives the stored ability ID through PKHeX `AbilityChangeRules.GetExpectedAbilityZA`. This also preserves Z-A birth-encounter abilities when the target is evolved, rather than incorrectly realigning its ability ID to the final species. Automatic gender is constrained to the final species' sole gender when applicable, preventing ancestor-generated female Gallade plans from failing. These corrections preserve explicitly chosen gender and valid generated ability slots.

## Executed verification

- **61 deterministic Alpha checks** in `HostTests --generated`: low-level Compatible Add, materialized seed/date, twelve identical seeded generations and byte-for-byte Add roundtrip, full health after final IVs, existing Alpha IV-only rejection, Custom Mewtwo level 1 with square shiny/zero EVs and a real incompatible report, mode-only changes, origin policy, fixed gender/nature overrides, starter origin adaptation with PID/EC preservation, HP/status, IV/HT reconciliation, mixed-batch rejection, Alpha off, recovery receipts, unreadable-main receipt retention, and recursive junction rejection before backup traversal. The level-5 Chikorita gift case additionally requires both Compatible preview and persisted/reopened result to be genuinely legal after its explicitly proposed move adjustments. Seeds 0–11 additionally prove legal Chikorita gift generation and automatic male Gallade planning through a lower-level ancestral encounter, including previously failing seeds.
- Evolved creation checks include Ivysaur, Charmeleon, Charizard and Meganium, their evolution thresholds, and absence of `StatAlphaInvalid`. **Annihilape 979** was found from the actual current catalog as an ancestor-only Alpha case and passed Compatible creation planning.
- **44 HTTP checks** in the same generated mode, including Alpha API hashes/no-write behavior, seed-as-string shape, invalid/overflow seed rejection, existing auth/Origin/Host security, preview, Save/Add, backups and idempotency.
- **67 existing detailed Engine checks**, executed on copies of the approved Demo202 fixture. The mint-only probe was corrected to avoid unrelated IV/Hyper Training mutations of an arbitrary Alpha input; detailed IV/HT coverage remains below it in the suite. The synthetic source hash remained `F5F6A5D2DF5B00819ED259551B4D0977011F842FD1FC1830E424CC7B53C33388`.
- Separate read-only Gallade 475 auto-gender probe: ten independent Compatible plans generated male targets. The hypothesized female-target bug was not reproduced, so no additional production change was made for that hypothesis.

The initial Gallade probe used level 100, selecting direct Gallade encounters. A later stronger probe below the direct encounter minimum **did** reproduce the ancestor-gender conflict, so the final implementation and seed regressions above supersede that initial inconclusive result.

SDK: 10.0.302. Isolated artifacts: `HostTests/bin/final202` and `Tests/bin/alpha210`. No tests used distributed/private game saves. Frontend visual checks, a published portable build and real-game loading remain separate acceptance evidence owned by the principal. All species/forms, RNG seeds, HOME transfers and constrained special evolutions were not exhaustively tested.

Residual review recommendation, not a reproduced release failure: manual existing-Pokémon species/form editing has its own ability reconciliation path in `ApplyDetails`. A future focused test should distinguish Z-A birth abilities from HOME/current-form realignment before changing that path. The current task's generation fix uses the vendor rule; full legality reports remain visible for arbitrary manual species edits.

Primary technical sources: vendored `MiscVerifierPA9`, `EncounterSlot9a`, `EncounterStatic9a`, `EncounterGift9a`, `LumioseRNG`, `GenerateParam9a`, and Z-A evolution forward/reverse tables. Upstream comparison: [PKHeX encounter data](https://github.com/kwsch/PKHeX/blob/master/PKHeX.Core/Legality/Encounters/Data/Gen9/Encounters9a.cs), [Alpha verifier](https://github.com/kwsch/PKHeX/blob/master/PKHeX.Core/Legality/Verifiers/Misc/MiscVerifierPA9.cs). The vendored version owns runtime behavior; no dependency upgrade was performed.
