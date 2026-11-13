# ZA Workbench client

React/TypeScript client for the embedded local .NET 10 host, version 2.0.3. Node.js 24/npm are build tools; portable users run the self-contained Windows EXE.

From the repository root:

```powershell
npm ci --prefix WebClient
npm run build --prefix WebClient
```

The production build writes `WebHost/wwwroot`. Finish the frontend build before building/publishing the host: those files are embedded at compilation. `./build.ps1` performs both stages and creates `dist/RecompPokemon.exe`. The development proxy defaults to loopback port 5075; use the embedded host for authenticated integration verification.

## Current interaction contract

- Party, 30-slot box and inspector form one workspace. Narrow layouts use a party strip and inline box disclosure. Edit drafts survive selection changes; creation uses a separate buffer and write action. Resolve edit drafts before adding.
- Add opens the species picker directly: the full Engine catalog (364 species), sorted by National Pokédex ID. Search matches names or an exact numeric ID, with an optional `#`. Cards support touch/click, arrows, Home/End, Enter and Escape. A shared visibility observer loads images near the visible area; catalog entries do not depend on image availability.
- Choosing a species loads its first supported form and valid gender. It preserves other creation details and resets `moves`/`plusMoves` to automatic generation. Metadata requests are cancellable and responses are revision-checked before applying.
- All 25 ball PNGs are bundled. Nature and Mint controls show +10% / −10% or neutral. Alpha immediately previews compatibility while retaining the Engine's original-encounter constraints.
- Sprite blobs are authenticated, keyed by species/form/shiny and decoded before success. Concurrent consumers share requests; a recovered request updates every subscribed copy. Failed requests are not successful cache entries. The host resolves exact visible identity and stores successful images under `cache/pokemon/v2`; unavailable images have explicit feedback and bounded retry.
- PT-BR, English and Spanish strings stay aligned. Move/nature catalog names and upstream PKHeX reports remain English. Inter, Lucide and frontend assets are bundled; no runtime CDN is required. Optional Pokémon artwork still depends on cached images or external PokéAPI/GitHub availability.
- Anime.js scopes own selection, creation and feedback motion, with `scope.revert()` cleanup and reduced-motion support. Business state never waits for animation callbacks.

## Local session and writes

The runtime token is extracted from the URL fragment and removed from browser history. It stays in module memory; an HttpOnly same-origin cookie permits refresh. Only the non-secret session ID is stored in `history.state`, never tokens, paths or drafts. Reload clears unsaved buffers; inline warnings explain pending changes.

Selection, metadata, box and validation requests are cancellable. Mutation fields are disabled during commits. Confirmed and uncertain writes remain distinct: an uncertain result preserves buffers for explicit reconciliation, while a refresh failure after a confirmed commit retains the backup receipt. Background refresh does not replay selection entrance motion.

## Verification and visual evidence

Run the frontend build above and the [host/sprite harnesses](../HostTests/README.md) for the surface changed. Actual desktop and 390px browser interactions are required for UI changes; compilation and PNG retrieval alone do not prove browser behaviour.

Current release screenshots use a synthetic save in build 2.0.3: [desktop](../docs/screenshots/web/current-desktop.jpg), [species picker](../docs/screenshots/web/current-species.jpg), [details](../docs/screenshots/web/current-details.jpg), [creation](../docs/screenshots/web/current-add.jpg), [narrow layout](../docs/screenshots/web/current-narrow.jpg). They show the captured states, not exhaustive coverage of every species, image or device.

Earlier [design proposals](../docs/design/za-workbench/README.md) and [implementation evidence](../docs/design/za-workbench/EVIDENCE.md) are historical records. Their dated checks and pending items describe those stages, not an aggregate current-release verdict.
