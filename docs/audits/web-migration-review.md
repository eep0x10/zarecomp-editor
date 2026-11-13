# Local web migration review

2026-11-13 — historical pre-implementation design review. The visual proposal was subsequently approved and implemented; see the workbench evidence and host verification for executed results. At this review checkpoint, No saves, game processes, dependencies or application code changed. Sources: `App/Program.cs`, `MainWindow.cs`, `PokemonPatchTools.cs`, `PokemonSpriteCache.cs`, localization; Engine models, Catalog, NatureAdvisor, SaveService and Details; project files, build script and test sources. No repository AGENTS.md found at the project root. Historical verification text was read as a report, not rerun evidence.

## Recommended boundary

Replace the WinForms entry point with a .NET 10 Windows WinExe hosting ASP.NET Core Kestrel and embedded frontend assets. Keep Engine and vendored PKHeX as project references. Bind explicitly with `Listen(IPAddress.Loopback, 0)`, await startup, then read the actual address and launch the default browser. Do not use `ListenLocalhost(0)`; it does not support dynamic ports. Do not accept URL/environment overrides that expose a wildcard interface. No Node runtime, Electron, database, account system or separate server installation is needed. Frontend build tooling, if selected, is a developer prerequisite only; its compiled assets ship inside the EXE.

Keep self-contained win-x64 single-file publication, version/licenses and existing distribution exclusions. Serve manifest-listed embedded resources, not a directory rooted beside the EXE (which also contains preferences/cache). The current single-file build permits native-library extraction: portable does not mean zero files written. Validate a published build from a read-only folder and a machine without .NET/Node before claiming portability. Embedding, MIME types and API fallback exclusions need explicit tests.

Use one small host/session adapter; retain the Engine as the only persistence owner. `SaveEdits` already commits mixed party/box edits together. `Add` remains a separate commit. Preserve expected hash checks, per-path mutex, process-local write gate, game guard, full-folder verified backup, temp-file flush, reopen validation, atomic replacement and final verification. Never reimplement these in JavaScript or duplicate PKHeX rules in controllers.

## Proposed API contract v1

All `/api/v1` requests require the runtime bearer token. Responses use explicit DTOs and camelCase; indices are zero-based, box slot IDs are global `box * 30 + offset`. Enum wire values are named strings, dates `YYYY-MM-DD`; hashes are opaque. Patch null/omitted means preserve, while false, zero and empty string remain meaningful. Stats arrays stay **HP, Atk, Def, Spe, SpA, SpD**. PID/EC are numbers; serialize 64-bit HOME Tracker as a decimal string to avoid JavaScript precision loss. Do not deserialize a whole read-only details object as a patch. Reject unknown JSON members and numeric enum values.

| Method/path | Input and behavior |
|---|---|
| GET `/bootstrap` | Version, supported locales, capabilities, safe preferences; no token in response/logs. |
| POST `/discovery` | Run existing bounded discovery on demand; return opaque candidate IDs, label and modified time. No write. |
| POST `/sessions` | Open `{candidateId}` or explicit user-entered `{path}`; inspect, assign random session ID, return summary/team/first box and base hash. |
| GET `/sessions/{id}` | Refresh coherent summary/team and hash; frontend decides inline whether to discard dirty drafts. |
| GET `/sessions/{id}/boxes/{box}` | Occupied entries plus slot identities; include hash. Empty positions are represented in the view from the 30-slot grid. |
| GET `/sessions/{id}/pokemon/{party\|box}/{slot}` | Existing `InspectPokemon`; return Values, ReadOnly, warnings, stats, legality and hash. |
| POST `/sessions/{id}/preview` | `{expectedHash, party, slot, patch}` → `PreviewPokemon`. No disk mutation. |
| POST `/sessions/{id}/preview-add` | `{expectedHash, options}` → `PreviewNewPokemon`. No disk mutation. |
| POST `/sessions/{id}/suggestions` | Hash plus current draft team values; call NatureAdvisor, never save automatically. |
| POST `/sessions/{id}/edits` | `{expectedHash, operationId, teamEdits, boxEdits}` → exactly one `SaveEdits`. Require nonempty hash. |
| POST `/sessions/{id}/add` | `{expectedHash, operationId, options}` → exactly one `Add`. UI must resolve existing edit drafts first. |
| DELETE `/sessions/{id}` | Release session handle; never delete a save. |
| GET `/catalogs` | Species/natures/balls/moves/languages/items/locations/ribbons, versioned for caching. |
| GET `/catalogs/species/{species}/forms/{form}` | Allowed forms/genders, base stats, PlusMoves; Engine remains authoritative. |
| GET `/sprites/{species}/{form}/{normal\|shiny}` | Validated image or local fallback; map IDs internally, no arbitrary URL parameter. |
| PUT `/preferences` | Allowlisted UI locale and last selected session path; atomic portable settings write, failure reported inline. |
| POST `/heartbeat` | Track active authenticated browser clients for orderly lifecycle. |
| POST `/shutdown` | Explicit inline Exit action; drain any in-flight commit before stopping. |

`/openapi/v1.json` and `/api-docs` provide the required interactive contract using bundled local UI assets and the same authorization, not a CDN. Built-in OpenAPI generation alone does not provide an interactive UI. Pin the chosen integration version during implementation. API docs are a developer surface; link discreetly, outside the primary product workflow.

Return the commit hash and backup receipt even when subsequent UI reload fails; distinguish `committed: true, refreshRequired: true` from a rejected write. Cache operation receipts by `(sessionId, operationId)` with request fingerprint; replay matching requests and reject mismatches. A disconnected HTTP client must not cancel an Engine commit or trigger an automatic retry. Engine operations are synchronous and the mutex is thread-affine: execute each entire Engine operation on one worker thread; do not await inside its lock scope.

## Session state, drafts and consistency

Server memory owns session ID → canonical path mapping and successful-operation receipts. Browser memory owns base snapshot, current selection and draft patches keyed by `(party,slot)`. Do not put save contents, trainer identifiers, token or draft details in localStorage. Discard resets browser drafts only. Switching saves with dirty drafts uses an inline choice. Details and Add become inline panels in the same screen, preserving the explicit Apply-draft and Save-to-disk distinction.

Send only dirty fields. Quick Nature currently changes both Nature and StatNature; an unchanged quick field must never overwrite a detailed minted nature. Preserve Star/Square distinctions, merge/revert semantics and nullable false/zero. Preview uses Engine validation and must retain the base hash.

Current `Inspect`, `InspectTeam` and `InspectBox` independently reread the file; initial UI loading can therefore mix versions. The host must validate the hash before/after grouped reads and reject/retry boundedly if it changed; each later read/preview includes hash and the UI refuses to merge another version into drafts. Serialize operations per open path. This coordinates this host, while Engine hash/mutex remains necessary across instances. A future immutable byte-snapshot Engine read API is cleaner but not required for the first migration.

## File selection and security

Strict no-popup product behavior is achievable with discovery candidates and an inline absolute-path field for `main`. Avoid an unrestricted drive/folder browser API. A native picker and browser file input both open OS dialogs, so neither should be introduced silently. Browser upload does not reveal the original absolute path and cannot preserve in-place editing/full-folder backups; it would be a separate import/export product mode, outside this migration.

Canonicalize input, reject UNC/device paths and reparse-point paths, require an existing main file, validate supported save structure and a documented measured size bound before reading. Only authenticated explicit selection authorizes a path; downstream endpoints accept session IDs, never arbitrary paths. Do not offer filesystem download, arbitrary shell/open-path, restore, or directory listing endpoints. Show backup receipt/path inline with a copy action rather than launching Explorer. Existing backups remain the recovery mechanism; no data migration is needed.

Create a cryptographically random per-process token (at least 256 bits), launch it in the URL fragment, read/remove the fragment immediately, retain only in browser memory and send Authorization headers. No cookie/session CSRF dependence. For mutation require exact runtime Origin, JSON content type and token; reject null/missing/foreign Origin. Reject foreign Origin on reads too; token-bearing non-browser reads may omit Origin. Disable CORS; validate Host equals the actual `127.0.0.1:port` to resist DNS rebinding. Request limits, no-store for private API responses, CSP with local scripts/images, frame-ancestors none, no-referrer and no external scripts reduce token/data exposure. Authentication does not protect against malware/extensions already controlling the same user/browser.

Do not log token, raw patches, trainer data, full save paths or stack traces in product responses. External artwork currently contacts PokeAPI/GitHub, exposing network address and requested species/form/shiny, not save bytes. Keep outbound artwork optional with offline fallback, allowlisted hosts, redirect validation, response size/time limits and bounded memory cache. Existing cache uses a 5 MiB response bound, four requests and validated atomic disk writes, but its static memory dictionary is unbounded and cannot be directly reused in a trimmed browser-only host because it returns System.Drawing Image. Port the byte-cache behavior or target Windows explicitly; never proxy client-provided URLs.

Localization supports pt-BR/en/es in App, while Catalog is English and Engine errors are mostly Portuguese; legality reports remain English. Move UI strings to embedded locale dictionaries, return stable machine error codes plus safe fallback detail, and preserve upstream report language disclosure. Do not change shared PKHeX global language state per concurrent request.

## Errors and remaining risks

Use ProblemDetails `{status, code, title, detail, fieldErrors?, retryable, traceId}`. 400 malformed body/unknown field; 401 token missing/invalid; 403 origin/host rejected; 404 unknown session/slot; 409 stale hash, game running, capacity conflict, duplicate operation mismatch; 413 size limit; 422 domain validation/unsupported or corrupt save; 423 writer busy; 500 unexpected/verification failure; 503 shutdown/busy. Do not classify every IOException as stale hash: it also covers missing media, backup/replace errors, access and mutex timeout. Existing Engine exceptions need a typed adapter or narrowly scoped internal exception classification; avoid frontend parsing Portuguese strings.

Current game guard detects process-name substrings and installation containment, not every possible external writer. Hash verification does not close the final race between last hash check and replacement with an uncooperative writer. Backup recursively enumerates before checking reparse entries, and a broad selected directory can make backup slow/large; validate selected path and backup topology carefully. Failure after replacement may mean the save changed successfully even though the call throws; preserve backup recovery details and require refresh rather than retry. None of these are evidence of observed data loss in this review.

Browser close cannot guarantee a shutdown event. Explicit Exit plus a short documented heartbeat lease can end the host after all clients disappear; drain writes and consider throttled/sleeping tabs before timeout. Never kill a process to implement exit. Browser startup failure needs a local URL/lifecycle fallback, to be designed with the principal rather than adding a modal by default.

## Verification scope and acceptance

Read-only source review completed; no builds, test harnesses, writes to saves or game operations executed. Existing console tests require an input save and generate fixtures; source covers add/edit preservation, mixed transaction rejection, stale hash, corrupt/full/locked storage, competing writers, and detailed patches. `Tests/verification.txt` contains historical results only. There are no HTTP/security, embedded-asset, lifecycle or publish tests yet.

Before release: run Engine tests on explicitly synthetic fixtures; host tests for all auth/origin/host failures, path rejection, JSON/enum/64-bit contracts, stale read/preview/save, duplicate operation delivery, disconnect during commit, post-commit reload failure, backup failure and shutdown drain; smoke a published single EXE without SDK/Node and without Internet. Frontend owner validates no-modals draft/Add flow and desktop/390px layouts after visual approval. No publication is authorized by this report.

Primary references: [Kestrel endpoints](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel/endpoints?view=aspnetcore-10.0), [ListenLocalhost implementation](https://github.com/dotnet/aspnetcore/blob/main/src/Servers/Kestrel/Core/src/KestrelServerOptions.cs), [OpenAPI support](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview?view=aspnetcore-10.0). Architecture/security controls above are design recommendations, not a claim of tested behavior.
