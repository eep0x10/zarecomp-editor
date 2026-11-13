# HTTP integration harness

After `npm ci` and `npm run build` in `WebClient`, run from the repository root on Windows with .NET 10:

```powershell
dotnet run --project HostTests/HostTests.csproj -- --generated
```

This mode generates an encrypted synthetic save from vendored `SAV9ZA` blank blocks, creates one party Pokémon using the unchanged Engine, then exercises the HTTP suite on another copy. No bundled game/save input is required or downloaded. Generated files are confined to the ignored output `data` directory; no game is launched. The test is a structural persistence/API probe, not proof that a generated save is playable.

Blank vendor blocks omit their scalar/object types. The fixture helper assigns synthetic types and an inert padding object to satisfy the existing fixed-size save detector. Reflection is limited to this test helper; production loading, validation and Engine persistence are unchanged. A schema-length/detection assertion stops the helper if the vendor schema drifts.

Optional approved fixture mode remains available:

```powershell
dotnet run --project HostTests/HostTests.csproj -- "C:/path/to/synthetic/main"
```

When a QA host is holding the normal Debug DLL, isolate all build artifacts:

```powershell
dotnet run --project HostTests/HostTests.csproj --artifacts-path HostTests/bin/isolated-artifacts -- --generated
```

Frontend builds must finish before any host build starts because generated hashed assets are embedded during compilation. Run the harness on Windows; its ACL and launcher-contract checks intentionally target the shipped Windows product.
