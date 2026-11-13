# HTTP and artwork integration harness

Run on Windows with .NET 10 SDK and Node.js 24/npm. From the repository root, build the frontend before any host build because its hashed assets are embedded during compilation:

```powershell
npm ci --prefix WebClient
npm run build --prefix WebClient
dotnet run --project HostTests/HostTests.csproj -- --generated
dotnet run --project HostTests/HostTests.csproj --no-build -- --sprites
```

The current 2.0.3 harness contains **40 generated-save HTTP checks** and **32 deterministic sprite checks**. CI runs both modes after publishing the portable application. These counts describe the suites; report the actual command/output when claiming a fresh pass.

## Generated-save HTTP checks

`--generated` constructs an encrypted synthetic save from vendored `SAV9ZA` blank blocks, creates one party Pokémon using the Engine, then exercises the HTTP suite on an independent copy. No game/save download or private input is required. Files stay in the ignored output `data` directory; no game is launched. The suite checks authentication/Host/Origin boundaries, catalog/form DTOs, validation, stale hashes, backed-up writes, operation replay, embedded assets and host shutdown. This is a structural persistence/API probe, not proof that the generated save is playable or accepted online.

Blank vendor blocks omit their scalar/object types. The fixture helper assigns synthetic types and an inert padding object to satisfy the existing fixed-size save detector. Reflection is limited to the test helper. A schema-length/detection assertion stops the helper if the vendor schema drifts.

An approved disposable fixture can also be used; the harness tests another copy and checks that the source remains untouched:

```powershell
dotnet run --project HostTests/HostTests.csproj -- "C:/DisposableTestSave/main"
```

Never pass your only personal save. The separate Engine harness has its own copied-save workflow:

```powershell
dotnet run --project Tests/ZaTool.Tests.csproj -c Release -- --details "C:/DisposableTestSave/main"
```

Its recorded 67-check pass is historical evidence, not a claim that this suite was freshly rerun for every release.

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
