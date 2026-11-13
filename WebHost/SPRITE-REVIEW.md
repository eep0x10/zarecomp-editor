# Sprite resolution and Alpha review — 2.0.1

2026-11-13. Scope: WebHost and HostTests; Engine read-only. No personal save, game process, installation or release publication was changed.

## Confirmed cause and fix

`SpriteCache` previously searched only `pokemon-species.varieties` for nonzero forms. PokéAPI represents cosmetic Vivillon patterns as `pokemon-form` entries, with only one species variety. The failed match returned a 1×1 PNG with HTTP 200, which the frontend enlarged as an image. Independently, the old form-zero shortcut used `666.png` (API default Meadow), while PKHeX form 0 is Icy Snow and Meadow is form 6. The current Engine form list and live API metadata confirm that mismatch.

The resolver now follows species → variety → exact cosmetic form when necessary, including form zero. It selects `front_default` or `front_shiny` from that resource and tries official artwork, HOME and standard PNG in that order. The pipeline never downgrades shiny to normal or another visible form. Scatterbug/Spewpa are explicit exceptions because their inherited future Vivillon patterns do not change their current appearance. Metadata URLs must reference allowlisted HTTPS PokéAPI resources; image URLs must stay inside the PokeAPI sprites repository. Redirects remain disabled.

There is no successful placeholder-image response. Missing/offline exact artwork returns HTTP 404 with `code: sprite_unavailable`; the frontend owns a visibly unavailable state and bounded retry. Failures are not cached as successes. The new `cache/pokemon/v2` namespace ignores images with earlier incorrect identity mapping. Stored PNGs require dimensions 16–4096, complete chunks, valid CRCs, pixel data and final IEND; the old 1×1 placeholder and truncated/corrupt caches are rejected. Successful images remain keyed by species/form/shiny; successful metadata persists for seven days for offline use. Existing timeout, 5 MiB limit, four network slots and bounded memory remain.

## Executed evidence

With SDK 10.0.302, isolated compilation plus `HostTests --sprites` passed **32 deterministic checks**. They cover all 20 Vivillon patterns in normal and shiny, exact-form HOME fallback when artwork is missing, no base-form substitution, memory/disk cache, rejection and repair of a cached 1×1 response, explicit 404, no normal substitute for absent shiny, successful recovery after a failed request, and invalid PNG rejection. Fixtures are generated PNGs and an injected mock HTTP handler; no network is needed for this mode.

`HostTests --sprites-live` passed the same 32 plus **24 real image retrieval checks**: normal/shiny for Vivillon Icy Snow, Continental, Meadow and Poké Ball; Flabébé red/blue; Floette white; Florges yellow; Furfrou natural/La Reine; Pumpkaboo average/small. Retrieved PNGs passed structural validation. Deerling samples were explicitly skipped because species 585 has no forms in the Engine's current Z-A catalog; it was not silently substituted. No sampled eligible variant was missing. All forms of every Z-A species were not exhaustively probed, and the upstream service can change or be unavailable.

The **39 generated-save HTTP integration checks** also passed against version 2.0.1 after the sprite change. Frontend browser rendering and packaged-executable verification remain with the principal/frontend specialist; successful PNG retrieval is not itself a visual browser check.

Commands from repository root (finish frontend build before host compilation):

```powershell
dotnet run --project HostTests/HostTests.csproj -- --sprites
dotnet run --project HostTests/HostTests.csproj -- --sprites-live
dotnet run --project HostTests/HostTests.csproj -- --generated
```

Use `--artifacts-path HostTests/bin/isolated-artifacts` before `--` if a QA host holds the normal output DLL. The live mode is explicitly optional; deterministic mode is suitable for CI. No full upstream artwork catalog is downloaded or bundled.

## Alpha: simplify presentation, retain Engine rules

Read-only review of `SaveService.ApplyDetails`, `CreatePokemon` and the current frontend toggle confirms the required dependencies: Alpha flag, Scale 255, at least three natural IVs at 31; native height/weight zero or HOME height/weight matching Scale. An IV promoted to 31 cannot remain Hyper Trained. The Engine reconciles required Plus records and checks compatible Alpha encounter data. Add additionally requires an Alpha encounter for the selected species/form/level. A flag/size preset cannot manufacture that encounter provenance for every existing Pokémon.

Keep one Alpha toggle. Enabling applies the existing dependent draft preset and runs the existing Engine preview immediately; explain any incompatible encounter inline, without silently changing species, level or origin. Disabling changes `isAlpha` only, preserving chosen size and IVs. Hide technical size fields behind advanced controls if desired, but do not bypass validation or claim universal legality. No Engine change is necessary for this presentation simplification; no Engine edits were made in this task.

Primary upstream references consulted: [PokéAPI Pokémon forms documentation](https://pokeapi.co/docs/v2#pokemon-forms), [official form identifiers](https://github.com/PokeAPI/pokeapi/blob/master/data/v2/csv/pokemon_forms.csv), [PokeAPI sprite repository](https://github.com/PokeAPI/sprites), and live [`pokemon/666`](https://pokeapi.co/api/v2/pokemon/666/) / [`vivillon-continental` form](https://pokeapi.co/api/v2/pokemon-form/vivillon-continental/). Local owning sources are the Engine Catalog/FormConverter and save service; these remain authoritative for accepted Z-A forms and Alpha patches.
