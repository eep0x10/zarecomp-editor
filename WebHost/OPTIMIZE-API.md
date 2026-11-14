# Whole-party optimization plan — 2.2.0

2026-11-14. Technical contract for non-writing team plans. The existing explicit `SaveEdits` transaction remains the only writer. All write verification used disposable synthetic saves.

## Endpoint and replacement contract

Authenticated POST `/api/v1/sessions/{id}/team-optimize-plan`:

```json
{
  "expectedHash": "encrypted-save-hash",
  "teamEdits": [],
  "options": {
    "bestNatures": true,
    "alpha": false,
    "maxIVs": false,
    "shiny": false,
    "maxSize": false,
    "maxFriendship": false,
    "level100": false
  },
  "alphaMode": "Compatible",
  "language": "pt-BR"
}
```

`teamEdits` contains existing **party** drafts in the existing TeamEdit format. Options default to false; language defaults to pt-BR and may also be en/es. Alpha mode is Compatible or Custom, using the existing Alpha policy. A false option preserves that field; it does not disable an already enabled Alpha/shiny state. The host verifies the expected hash and coherent reads before returning a plan.

Response fields: `available`, `code`, `hash`, `teamEdits`, `items`, `warnings`, `natures`.

- An available result includes **exactly one** `teamEdits` entry for every occupied party slot: `{slot, details:{...}}`. Its details dictionary is sparse relative to the **save on disk** and contains no null placeholders. An unchanged/reverted slot is explicitly `{slot, details:{}}`.
- These are complete canonical replacements for the covered party drafts, not additive deltas. Replace each covered party draft, removing it when details is empty. Preserve box drafts separately. Otherwise a previous level-50 draft would survive an optimization that restores its stored level 100, because the canonical details rightly omits `level`.
- `items` provides `{slot,name,species,form,available,code,before,preview,patch,changes,warnings,nature,natureReason}`. `before` is the effective state with the existing draft; `preview` is the final real Engine preview. Item `patch` and `changes` describe the optimization's contribution relative to that effective state, and are intended for review. Apply the canonical top-level `teamEdits`, not the item patch.
- If any slot fails, `available:false` and top-level `teamEdits:[]` prevent partial application. Successful per-slot previews may still be displayed, alongside the failed slot's reason. Codes are `optimize_plan`, `optimize_unavailable`, `invalid_team_draft`, or a per-slot existing Alpha failure code.

Planning creates no backup and writes no save. Applying to browser drafts also writes no save. The existing Save changes action commits the resulting nonempty party drafts (and any preserved box drafts) with the current hash and automatic verified backup. Filter empty replacement entries from the actual SaveEdits request, as ordinary empty drafts are not mutations.

## Option semantics

Best natures changes **StatNature/Mint only**, retaining original Nature/PID/RNG. Recommendations use canonical Catalog base stats for species/form; caller-supplied derived stats, current nature, moves, level, IVs, EVs and locale do not affect the chosen ID. The simple profile selects physical, special, mixed or slow defensive emphasis. It is a stable explanatory heuristic, not a universal competitive optimum; moves, role, Mega Evolution and opponents can justify another choice. Language changes the explanation, not the decision.

When Level 100 is selected, it is composed before Alpha planning. Alpha promotes the selected useful three natural IVs, prioritizing the effective Mint's boosted stat and the HP/offense/speed-or-defense profile. Existing 31s and all other IV values are preserved, so the result may have more than three perfect IVs. Max IVs explicitly sets all six to 31. Promoted perfect IVs clear their incompatible Hyper Training flags.

Shiny changes only non-shiny → Star and preserves Square. Max size sets Scale 255 with native height/weight zero or HOME height/weight matching Scale. Max friendship uses `PA9.CurrentFriendship`: OT friendship for handler 0, handling-trainer friendship for handler 1. It does not overwrite the other trainer's friendship. Level/IV/Mint optimization preserves existing HP/status through the normal edit path.

Compatible Alpha uses the existing origin-adjustment plan and fails the entire party plan if any member cannot be planned. Explicit Custom permits the existing Alpha encounter mismatch while retaining size/IV/metadata constraints. The real PKHeX legality report is never replaced with a fabricated positive result. An already stored Custom Alpha retains the necessary explicit policy in nonempty canonical drafts even when the Alpha option is disabled. Empty no-op entries do not manufacture mode-only drafts.

## Diagnosis and verification

The old advisor counted current move categories and accepted caller-derived base stats. Alpha origin/level adjustments can change moves, so an apparently repeated optimization could feed a different profile back into the next recommendation. The old client also changed original Nature and StatNature together. The new canonical decision basis and Mint-only plan remove those dependencies.

Executed with SDK 10.0.302 in isolated `HostTests/bin/optimize220` artifacts:

- **45 optimization Engine/API checks**: all seven flags persisted across six members; canonical nature stability for different client stats/moves/nature/level and PT/EN/ES; Mint/PID preservation; disk-level and Mint reversion with retained nickname; complete empty replacement entries; role IV promotion preserving existing perfect/other IVs; no partial Compatible/invalid-draft application; Custom Alpha legality; Square; HOME/native size; Hyper Training; EV/nickname drafts; repeated plan/no-write hashes; repeated plan after Save becoming no-op; HP/status preservation; HT friendship 255 with OT friendship 77 retained; already-Custom policy with Alpha option off; sparse JSON; authenticated API, stale hash and bounded invalid-species rejection.
- **61 existing Alpha checks** and **44 existing HTTP checks** passed in the same generated fixture harness.
- **52 legacy Engine checks** passed with updated canonical-nature assertions, including persistence, backups, mixed batches, locked storage and cross-process stale-write protection. The approved synthetic source file was unchanged.

The new optimizer fixture is generated from blank/approved synthetic data and intentionally contains both native and HOME-shaped/custom records to exercise format rules; this is not proof those constructed records are playable or globally legal. The deterministic planner does not query an external competitive service. CUA desktop/390px, the final portable package and any real-game testing remain separate acceptance evidence owned by the principal/frontend specialist. No exhaustive competitive-quality claim is made.

Source owners: `Engine/NatureAdvisor.cs`, `Engine/SaveService.Optimize.cs`, Engine models, WebHost contracts/routes/OpenAPI, and `HostTests/OptimizeTests.cs`. Existing scalar semantics are confirmed by vendored `PA9.CurrentFriendship` and the existing `SaveService.ApplyDetails`/Alpha plan implementation.
