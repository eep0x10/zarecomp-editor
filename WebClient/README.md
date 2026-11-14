# ZA Workbench client

React/TypeScript client for the embedded local .NET 10 host, version 2.2.0. Node.js 24/npm are build tools; portable users run the self-contained Windows EXE. Show the version supplied by host bootstrap in the interface so a stale portable build is identifiable.

From the repository root:

```powershell
npm ci --prefix WebClient
npm run build --prefix WebClient
npm test --prefix WebClient
```

The production build writes `WebHost/wwwroot`. Finish the frontend build before building/publishing the host: those files are embedded at compilation. `./build.ps1` performs both stages and creates `dist/RecompPokemon.exe`. The development proxy defaults to loopback port 5075; use the embedded host for authenticated integration verification.

## Team Optimize 2.2.0

Seven independent options open in an inline panel. Preview is a non-writing API call; Apply validates session/hash/revision/draft signature and the complete returned slot set before replacing canonical party drafts, including empty patches that clear reverted values. Box drafts stay intact. Cancel and late responses cannot apply changes. Nature recommendation changes only Mint and is stable by canonical species/form; select options show names once and a single colored effect line below.

71 client checks, TypeScript/Vite and root desktop/390px interaction verification passed. See [API contract](../WebHost/OPTIMIZE-API.md) and [delivery evidence](../docs/design/team-optimize-2.2.md).

## Current interaction contract

- Party, 30-slot box and inspector form one workspace. Narrow layouts use a party strip and inline box disclosure. Edit drafts survive selection changes; creation uses a separate buffer and write action. Resolve edit drafts before adding.
- Add opens the species picker directly: the full Engine catalog (364 species), sorted by National Pokédex ID. Search matches names or an exact numeric ID, with an optional `#`. Cards support touch/click, arrows, Home/End, Enter and Escape. A shared visibility observer loads images near the visible area; catalog entries do not depend on image availability.
- Choosing a species loads its first supported form and valid gender. It preserves other creation details and resets `moves`/`plusMoves` to automatic generation. Metadata requests are cancellable and responses are revision-checked before applying.
- All 25 ball PNGs are bundled. Nature and Mint controls show +10% / −10% or neutral; neutral Mint choices remain available. Without an explicit Mint override, effective nature follows the original. Form changes must retain a valid gender. Automatic shiny remains null, distinct from an explicit non-shiny choice.
- Alpha uses `/sessions/{id}/alpha-plan` or `/alpha-plan-add`. Compatible may propose origin, minimum level and necessary move changes; explicit Custom preserves requested origin and shows the real PKHeX findings. Display before/after origin, level, scalars, IVs/Hyper Training, moves and Plus records before applying the response to the draft. Both modes retain Scale 255/three perfect IV requirements. Applying a plan never writes the save. Turning Alpha off clears the mode. Bind responses to the current session/hash/selection/revision so an obsolete plan cannot be applied.
- Sprite blobs are authenticated, keyed by species/form/shiny and decoded before success. Concurrent consumers share requests; a recovered request updates every subscribed copy. Failed requests are not successful cache entries. The host resolves exact visible identity and stores successful images under `cache/pokemon/v2`; unavailable images have explicit feedback and bounded retry.
- PT-BR, English and Spanish strings stay aligned. Move/nature catalog names and upstream PKHeX reports remain English. Inter, Lucide and frontend assets are bundled; no runtime CDN is required. Optional Pokémon artwork still depends on cached images or external PokéAPI/GitHub availability.
- Anime.js scopes own selection, creation and feedback motion, with `scope.revert()` cleanup and reduced-motion support. Business state never waits for animation callbacks.

## Local session and writes

The runtime token is extracted from the URL fragment and removed from browser history. It stays in module memory; an HttpOnly same-origin cookie permits refresh. Only the non-secret session ID is stored in `history.state`, never tokens, paths or drafts. Reload clears unsaved buffers; inline warnings explain pending changes.

Selection, metadata, box and validation requests are cancellable. Ignore late responses from previous sessions, selections, boxes or draft revisions, including validation failures; cancellation alone is insufficient to prove response freshness. Mutation fields are disabled during commits. Confirmed and uncertain writes remain distinct: an uncertain result preserves buffers for explicit reconciliation, while a refresh failure after a confirmed commit retains the backup receipt. Background refresh does not replay selection entrance motion.

## Verification and visual evidence

Run the frontend build above and the [host/sprite harnesses](../HostTests/README.md) for the surface changed. Actual desktop and 390px browser interactions are required for UI changes; compilation and PNG retrieval alone do not prove browser behaviour.

Local 2.1.0 verification passed **39 client regression checks**, separately from 61 Alpha, 44 generated-save HTTP and 67 Engine checks. Client checks exercise the draft/plan helpers and request context guards; they do not replace browser verification. `npm test --prefix WebClient` runs them from the repository root. CI is configured to run this command after the build; the [audit](../docs/audits/2026-11-13-alpha-review.md) distinguishes local results from published CI evidence. The previous 32 deterministic sprite checks remain historical and were not rerun for this change.

Alpha screenshots use a synthetic save in build 2.1.0: [plan](../docs/screenshots/web/alpha-plan-2.1.0.jpg), [explicit Custom](../docs/screenshots/web/alpha-custom-2.1.0.jpg), [narrow layout](../docs/screenshots/web/alpha-narrow-2.1.0.jpg). General-workspace captures remain labelled build 2.0.3: [desktop](../docs/screenshots/web/current-desktop.jpg), [species picker](../docs/screenshots/web/current-species.jpg), [details](../docs/screenshots/web/current-details.jpg), [creation](../docs/screenshots/web/current-add.jpg), [narrow layout](../docs/screenshots/web/current-narrow.jpg). They show the captured states, not exhaustive coverage of every species, image or device. The [2.1.0 audit](../docs/audits/2026-11-13-alpha-review.md) records executed verification and its limits.

Earlier [design proposals](../docs/design/za-workbench/README.md) and [implementation evidence](../docs/design/za-workbench/EVIDENCE.md) are historical records. Their dated checks and pending items describe those stages, not an aggregate current-release verdict.
