# Contributing

Use Windows with the .NET 10 SDK and Node.js 24/npm. Run `./build.ps1` for a portable build; Node is a build tool and is not needed by portable users. See [README](README.md#build-from-source) for copied-save test commands. Save-writing changes should cover encrypted round trips, unrelated Pokémon preservation, backups, stale files and invalid/full destinations. Never test against your only save.

Keep UI strings aligned across PT-BR, English and Spanish. Preserve the transaction boundary in `Engine/SaveService.cs`; UI actions must not write save bytes directly. Describe limitations when changing nature suggestions. Keep the vendored PKHeX revision and notices explicit when updating it.

The web workbench uses one inline workspace, Anime.js with reduced-motion/cleanup, and embedded assets with no runtime CDN. Preserve local Host/Origin/authentication checks, expected hashes, operation receipts and private runtime diagnostics. Verify actual desktop/390px interactions; a build or generated design image is not evidence of working UI. Exercise refresh, out-of-order responses and ambiguous commit outcomes when changing the host/client boundary.

Bug reports should include application version, Windows version, steps, expected/actual behaviour and a sanitised error. Do not attach saves, backups, tokens, personal paths or trainer details to public issues. Prefer synthetic reproductions. Pull requests should explain the change and checks actually run; distinguish compilation from copied-save tests.

Contributions are distributed under GPL-3.0-or-later. Do not contribute game binaries, game dumps, personal saves or assets without appropriate rights.
