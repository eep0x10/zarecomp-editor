# Contributing

Use Windows with the .NET 10 SDK. Run `./build.ps1` for a portable build. See [README](README.md#build-from-source) for copied-save test commands. Save-writing changes should cover encrypted round trips, unrelated Pokémon preservation, backups, stale files and invalid/full destinations. Never test against your only save.

Keep UI strings aligned across PT-BR, English and Spanish. Preserve the transaction boundary in `Engine/SaveService.cs`; UI actions must not write save bytes directly. Describe limitations when changing nature suggestions. Keep the vendored PKHeX revision and notices explicit when updating it.

Bug reports should include application version, Windows version, steps, expected/actual behaviour and a sanitised error. Do not attach saves, backups, tokens, personal paths or trainer details to public issues. Prefer synthetic reproductions. Pull requests should explain the change and checks actually run; distinguish compilation from copied-save tests.

Contributions are distributed under GPL-3.0-or-later. Do not contribute game binaries, game dumps, personal saves or assets without appropriate rights.
