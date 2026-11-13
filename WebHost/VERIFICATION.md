# Current verification status — 2.0.3

The current shipped app is **2.0.3**. [Windows CI](https://github.com/eep0x10/zarecomp-editor/actions/runs/43469969015) passed the portable build, **40 generated-save HTTP checks** and **32 deterministic artwork checks**. Species-list coverage and the readonly forms endpoint were added after the original 39-check suite. The latest grid sorts all 364 current catalog species by their National Pokédex numbers.

Root separately verified the shipped EXE startup, embedded frontend/forms API, complete grid order, search order, archive/source integrity and public download hash. Earlier browser tests exercised editing/creation/backups, Alpha, form artwork recovery, keyboard/touch and 390px reflow on synthetic data. See [workbench evidence](../docs/design/za-workbench/EVIDENCE.md) and [sprite review](SPRITE-REVIEW.md) for exact versions, executed scopes and limits. This does not establish real-game/online acceptance or portability on every Windows machine.

## Historical first-host verification

The following records the original 2.0.0/2.0.1 checkpoint. Its version/counts and statements about what was not tested refer to that checkpoint, rather than the current release.

# Original local host verification — 2026-11-13

**2.0.1 follow-up:** sprite resolution and cache behavior below were superseded by the [sprite review](SPRITE-REVIEW.md). Exact artwork now returns an explicit 404 when unavailable, never a 1×1 placeholder. The generated HTTP harness passed all 39 checks again; sprite suites added 32 deterministic and 24 live retrieval checks.

Implemented host version 2.0.0 in `RecompPokemon.WebHost.csproj`, AssemblyName `RecompPokemon`, .NET 10 Windows WinExe. Engine and existing WinForms application files were not changed. Frontend build owns `wwwroot`; host embeds its generated files. No release publication was performed.

## Executed evidence

Using SDK 10.0.302, `dotnet build WebHost/RecompPokemon.WebHost.csproj --no-restore -v minimal` passed with zero warnings/errors. The final `dotnet run --project HostTests/HostTests.csproj --artifacts-path <isolated-artifacts> -- <approved-synthetic-main>` built the current code and passed **39 HTTP integration checks**:

- Missing credentials, foreign/missing/null Origin, foreign Host and another-local-origin cookie rejection.
- Bootstrap shape; HttpOnly/SameSite Strict session cookie and authenticated page-reload continuity without browser token storage.
- Coherent snapshot hash, HOME Tracker decimal-string serialization, preview with no save mutation, stale preview rejection.
- Field-coded level validation, numeric enum/unknown member/NaN JSON rejection.
- Artwork blob CSP permission; exact acronym JSON field names; null edit arrays/entries and null suggestion entries rejected with 400 before Engine calls.
- No backup or save writes before explicit commit; actual SaveEdits commit, full-folder backup contents/hash and preserved adjacent metadata.
- Identical operation receipt replay without another backup; mismatched reused operation ID and stale commit rejection.
- Actual Add commit and backup; UNC/relative path rejection.
- Local OpenAPI schema, embedded frontend index and exclusion of settings from static assets.
- Runtime credential JSON with protected ACL granting only the current user, authenticated heartbeat, explicit shutdown acknowledgement and completion.
- Approved synthetic source fixture hash unchanged. Generated copies/backups remain under ignored `HostTests/bin/.../data/` for inspection.

The harness starts an in-process ephemeral-loopback Kestrel server. It prints only check labels, never token, trainer identifiers or fixture contents. It is a console harness rather than a test-framework package and has no new NuGet dependency.

## Host integration

Launch `RecompPokemon.exe` normally to open the default browser. `--save <absolute-main>` overrides remembered selection. For controlled QA: `--no-browser --save <synthetic-copy/main> --runtime-file <private-runtime.json>`; optional `--port <n>` remains strictly loopback. Runtime file includes address/token/processId, is current-user restricted and is removed on orderly disposal. Do not include it in screenshots/distribution. `--no-browser` disables idle exit for QA. Normal launch uses a five-minute inactivity lease; the frontend posts heartbeat every 30 seconds and offers explicit Exit.

Authentication is a per-process random bearer token delivered in URL fragment. An authenticated response sets a separate per-process random-name HttpOnly session cookie; cookie-only access requires `Sec-Fetch-Site: same-origin`. Mutation also requires exact runtime Origin and JSON. GET/HEAD may omit Origin with valid bearer, supporting command-line QA. No bearer token is written to localStorage or sessionStorage; the browser manages the ephemeral HttpOnly session cookie. API errors use safe stable codes; allowlisted Engine field validation explains constraints without exposing arbitrary exception paths.

## Limits still requiring verification

No real-game load, personal-save mutation, published single-file/no-SDK-machine run, browser visual/accessibility/motion verification or installer/update behavior was tested here. Frontend owner/root owns CUA desktop/390px and release-build integration.

Crash/power-loss injection, HTTP disconnect during an Engine commit, an actual competing-writer race, backup permission/media failure, ambiguous post-replacement Engine IOException and throttled-browser idle expiry have not been fault-injected. Engine remains synchronous and independent of request cancellation; global host gate plus Engine mutex/hash checks guard mutations, but an uncooperative external writer's last-check/replace race remains inherited. Unknown post-write IO outcome is cached as `outcomeUnknown` and requires refresh, not retry. Existing backup recovery is manual.

Artwork is best-effort, restricted to fixed PokeAPI/GitHub HTTPS origins without redirects, capped at 5 MiB/4096px and four network requests. Cache memory is bounded to 128 entries; invalid/offline requests use a local transparent PNG. No real offline/network failure injection was run. External artwork requests reveal requested species/form/shiny and network address; no save bytes/trainer data are uploaded. Original English catalog labels and English upstream legality report are retained; application locale strings belong to frontend. Field-safe legacy validation detail can remain Portuguese.

OpenAPI 3.1 is generated locally from the explicit route table and reflected request DTO schemas. The bundled interactive reference uses local JavaScript and manual request submission; it is not Swagger/Scalar. Response descriptions exist, but exhaustive response-property/range schemas are not generated. No CDN or server-side arbitrary URL proxy is included.
