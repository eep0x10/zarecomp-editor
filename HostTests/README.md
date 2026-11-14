# HTTP and artwork integration harness

Run on Windows with .NET 10 SDK and Node.js 24/npm. From the repository root, build the frontend before any host build because its hashed assets are embedded during compilation:

```powershell
npm ci --prefix WebClient
npm run build --prefix WebClient
npm test --prefix WebClient
dotnet run --project HostTests/HostTests.csproj -- --generated
dotnet run --project HostTests/HostTests.csproj --no-build -- --sprites
```

Local 2.1.0 verification passed **61 Alpha, 44 generated-save HTTP, 67 Engine and 39 client regression checks**. The generated mode runs Alpha checks before the HTTP suite. CI is configured to run client regressions after the frontend build and generated-save/sprite modes after publishing the application; local verification does not establish a published CI result. The earlier 32 deterministic sprite checks remain historical evidence, unchanged and not rerun for this change. Report the actual command/output when claiming a pass. The [2.1.0 audit](../docs/audits/2026-11-13-alpha-review.md) records executed checks and limits.

## Team optimization 2.2.0

Generated mode also executes **45 Optimize Engine/API checks**, alongside 61 Alpha and 44 HTTP checks. Separately, 71 client checks and 52 legacy Engine checks passed. The optimizer checks all seven options/save-reopen, no-write planning, complete canonical replacement including empty patches, stable natures, all-or-nothing errors, original nature/Square/EV/metadata preservation, natural-IV priorities, HOME/native size, current-handler friendship and existing Custom Alpha policy. Local results do not establish published CI; record its final run separately. See [contract](../WebHost/OPTIMIZE-API.md) and [UI evidence](../docs/design/team-optimize-2.2.md).

## Generated-save HTTP checks

`--generated` constructs an encrypted synthetic save from vendored `SAV9ZA` blank blocks, creates one party Pokémon using the Engine, then exercises the HTTP suite on an independent copy. No game/save download or private input is required. Files stay in the ignored output `data` directory; no game is launched. The suite checks authentication/Host/Origin boundaries, catalog/form DTOs, validation, stale hashes, backed-up writes, operation replay, embedded assets and host shutdown. This is a structural persistence/API probe, not proof that the generated save is playable or accepted online.

Alpha-plan regressions cover no-write planning for both existing records and creation; Compatible encounter/level proposals and necessary move corrections; explicit Custom with real invalid reports when appropriate; Scale 255 and at least three perfect IVs in both modes; stale-hash rejection; and disabling Alpha without retaining its conversion policy. Persistence checks cover backup paths containing reparse points/junctions and failures after a possible write. A failed receipt must not be treated as proof that the save stayed unchanged.

Blank vendor blocks omit their scalar/object types. The fixture helper assigns synthetic types and an inert padding object to satisfy the existing fixed-size save detector. Reflection is limited to the test helper. A schema-length/detection assertion stops the helper if the vendor schema drifts.

An approved disposable fixture can also be used; the harness tests another copy and checks that the source remains untouched:

```powershell
dotnet run --project HostTests/HostTests.csproj -- "C:/DisposableTestSave/main"
```

Never pass your only personal save. The separate Engine harness has its own copied-save workflow:

```powershell
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- --details "C:/DisposableTestSave/main"
```

The detailed Engine suite passed all 67 checks locally for 2.1.0, separately from the 61 Alpha and 44 HTTP checks. The fixture path above is a placeholder: use a disposable approved copy.

## Artwork checks

`--sprites` needs no save input or network. Generated PNGs and mock HTTP exercise all 20 Vivillon patterns in normal/shiny, exact-form fallback, offline memory/disk cache, explicit unavailable results, retry recovery and rejection of blank/corrupt PNGs. This proves resolver behaviour, not availability of every upstream image or browser rendering.

Optional live probes:

```powershell
dotnet run --project HostTests/HostTests.csproj --no-build -- --sprites-live
```

This mode adds actual PokéAPI/GitHub retrieval. The [recorded sprite review](../WebHost/SPRITE-REVIEW.md) passed 32 deterministic plus 24 sampled live checks; it also records skipped samples and non-exhaustive coverage. These are historical results. Sources may change or be unavailable, and no complete artwork catalog is bundled.

If a QA host holds the normal Debug DLL, isolate build artifacts:

```powershell
dotnet run --project HostTests/HostTests.csproj --artifacts-path HostTests/bin/isolated-artifacts -- --generated
```

Windows is intentional: ACL and launcher-contract checks target the shipped product. See the [current editing guide](../docs/EDITING.md) for write semantics and the [client guide](../WebClient/README.md) for browser verification. Older [host verification notes](../WebHost/VERIFICATION.md) describe their recorded release stage.
