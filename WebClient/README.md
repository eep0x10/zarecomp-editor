# ZA Workbench client

React/TypeScript client for the embedded local .NET host. Build-time Node is separate from the portable application runtime.

```powershell
npm ci --prefix WebClient
npm run build --prefix WebClient
```

The production build writes `WebHost/wwwroot`; rebuild/publish the host afterward because it embeds those files. Development server proxy defaults to loopback port 5075; use the embedded host for authenticated integration evidence.

## Design and interaction record

Approved direction: [A workbench](../docs/design/za-workbench/README.md), with existing generated laboratory/background and proportional emblem assets. One workspace replaces native edit/add dialogs. The first representative slice covers open → select → draft level/ball/IV/Alpha changes → switch selection → validate → explicit backup commit. No browser dialogs are used.

- Party and 30-slot box retain their spatial relationship to the inline inspector. Narrow layout uses a party strip and an inline box disclosure.
- Warm amber indicates selection/shiny; teal indicates writes/backup. Inter Variable fonts and Lucide icons are bundled locally. Source sprites are authenticated blobs from the local host, cached and revoked when the page leaves.
- Drafts are indexed by party/global-box slot. Selection and box changes preserve drafts. Create is a separate buffer and commit action; unresolved edit drafts block creation writes.
- The runtime token is extracted from the fragment and immediately removed from browser history; it stays in module memory. The host's HttpOnly cookie supports reload without persisting the token in browser storage.
- Anime.js 4.5 scopes own preview/selection, creation and feedback effects, with `scope.revert()` cleanup. Reduced-motion changes cancel nonessential motion. No business state waits for an animation callback.
- Selection, metadata, box and validation requests are cancellable. Validation responses are revision-checked. Mutation fields are disabled while committing. Confirmed/uncertain writes have separate outcomes; uncertain writes retain buffers and require explicit reconciliation.

## Evidence status

TypeScript and Vite production build passed. Parent CUA verified the first slice at 1488×1058: layout, opening, selection, retained drafts, preview, backup save/reload and decoded sprites passed. Initial 390px check found no horizontal page overflow but small touch targets; the focused correction raises editing/control targets to 44px and gives the fixed footer additional scroll clearance. Its 390px retake passed, reported by the parent CUA reviewer: numeric targets at least 44px, Hyper Training label hit area 44×67px, save targets 44px, page scroll width 375px within a 390px viewport. These are implementation checks, distinct from the generated proposal images. Profile, attributes, moves, origin and advanced groups are implemented; the expanded flow and final interaction/motion/keyboard review remain pending.

Official animation API reviewed: [React integration](https://animejs.com/documentation/getting-started/using-with-react/) and [scopes](https://animejs.com/documentation/scope/).

Session refresh uses only the non-secret session ID in `history.state` and resumes it through GET; expired IDs fall back to the launch save path supplied by the host. Browser history never stores a token, save path or draft. Reload intentionally clears unsaved in-memory drafts and the workspace displays an inline warning while any draft/create buffer exists.

The final source now includes the five complete field groups, creation automatic/custom overrides, existing native domain-message translations for English/Spanish, read-only metadata and original PKHeX reports, API developer access and an explicit local-host stop action. Suggestions project only supported DTO properties and load current base stats when identity changes. Known/unknown mutation outcomes remain distinct; a post-commit refresh failure keeps the confirmed backup receipt. Background refresh does not replay the selection entrance; selection motion is keyed to actual loaded details.

Final integration evidence and current screenshots are recorded in [workbench evidence](../docs/design/za-workbench/EVIDENCE.md). The final narrow check also measured the Add and receipt-close targets at44px. Release verification remains a separate packaging step.
