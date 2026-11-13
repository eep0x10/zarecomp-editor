# Contributing

Use Windows with the .NET 10 SDK and Node.js 24/npm. Run `./build.ps1` for a portable build; Node is a build tool and is not needed by portable users. See [README](README.md#build-from-source) for copied-save test commands. Save-writing changes should cover encrypted round trips, unrelated Pokémon preservation, backups, stale files and invalid/full destinations. Never test against your only save.

Keep UI strings aligned across PT-BR, English and Spanish. Preserve the transaction boundary in `Engine/SaveService.cs`; UI actions must not write save bytes directly. Describe limitations when changing nature suggestions. Keep the vendored PKHeX revision and notices explicit when updating it.

The web workbench uses one inline workspace, Anime.js with reduced-motion/cleanup, and embedded assets with no runtime CDN. Preserve local Host/Origin/authentication checks, expected hashes, operation receipts and private runtime diagnostics. Verify actual desktop/390px interactions; a build or generated design image is not evidence of working UI. Exercise refresh, out-of-order responses and ambiguous commit outcomes when changing the host/client boundary.

The 2.0.3 creation picker must retain the complete Engine catalog (currently 364 species), National Pokédex ID order, name/exact-ID search, lazy images and touch/keyboard selection. Species changes preserve other creation fields while resetting moves/Plus to automatic generation and selecting a supported form/gender. Keep all 25 offline ball PNGs and nature/Mint effect labels aligned across languages. Alpha preview must preserve the existing encounter constraints.

Artwork must keep exact visible form/shiny identity. Preserve `cache/pokemon/v2`, explicit unavailable responses, bounded retries and recovery updates to every mounted copy of a sprite. Never present a different form or normal image as a successful shiny result. Test resolver changes with `dotnet run --project HostTests/HostTests.csproj -- --sprites`; live network probes are optional. See the [harness guide](HostTests/README.md). The current suite has 40 generated-save HTTP and 32 deterministic sprite checks; historical Engine/live results must be labelled as such rather than reported as freshly executed.

Bug reports should include application version, Windows version, steps, expected/actual behaviour and a sanitised error. Do not attach saves, backups, tokens, personal paths or trainer details to public issues. Prefer synthetic reproductions. Pull requests should explain the change and checks actually run; distinguish compilation from copied-save tests.

Contributions are distributed under GPL-3.0-or-later. Do not contribute game binaries, game dumps, personal saves or assets without appropriate rights.
