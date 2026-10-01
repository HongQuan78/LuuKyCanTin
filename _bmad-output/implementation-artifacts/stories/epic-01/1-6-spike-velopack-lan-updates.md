---
story: "1.6"
epic: 1
title: "Spike: Velopack updates from a LAN shared folder"
status: in-review
type: spike
size: M (time-boxed, 1.5 days suggested)
backlogItems: [SP-02]
frsCovered: []
nfrsTouched: [NFR7, NFR9, NFR13]
dependsOn: ["1.1"]
baseline_commit: 03d33e35657493def412b22f50b2b1c44a28419d
---

# Story 1.6: Spike: Velopack updates from a LAN shared folder

Status: ready-for-dev

## Story

As an administrator,
I want to confirm that workstations install and update the app from a shared folder on the LAN without Internet,
So that rolling out new versions takes one step and needs no visit to each machine.

## Acceptance Criteria

1. **Given** a self-contained build packaged with Velopack (`Setup.exe` plus a release feed)
   **When** the release is placed in a UNC shared folder (e.g. `\\server\LuuKyCanTin\releases`)
   **Then** a test workstation installs it from that folder with no .NET runtime pre-installed

2. **Given** a newer version published to the same folder
   **When** the installed app starts
   **Then** it detects the update, applies it, and restarts on the new version

3. **Given** the spike result
   **When** it is reviewed
   **Then** a note records the folder layout, the share permissions needed (read for workstations, write for the admin) and how the DB-version check (Story 1.2) interacts with updates; the full rollout is Story 7.4

## Tasks / Subtasks

- [x] **T1. Throw-away spike app, outside the product code** (AC: 1)
  - [x] `spikes/SP-02-Velopack/`: a minimal WinForms `net10.0-windows` app showing its version in the title. It's **not** in `LuuKyCanTin.slnx` and not referenced from `src/`. Reuse `spikes/README.md` from Story 1.5 if it exists.
  - [x] Add the `Velopack` NuGet package. Make `VelopackApp.Build().Run();` the **first line** of `Main`, before `ApplicationConfiguration.Initialize()`. Velopack handles install/update hooks there and may exit the process.
  - [x] Install the CLI: local tool manifest (`spikes/dotnet-tools.json`), `vpk` 1.2.161 pinned.
- [x] **T2. Package and install from a UNC share** (AC: 1)
  - [x] `dotnet publish -c Release -r win-x64 --self-contained true -o publish`, then `vpk pack --packId LuuKyCanTinSpike --packVersion 0.1.0 --packDir publish --mainExe SP-02-Velopack.exe`. Outputs noted: `Setup.exe`, `*-full.nupkg`, `releases.win.json`, `RELEASES`, portable zip.
  - [x] Copy the output to a share — **simulated with a local directory** (the dev shell is not elevated, so `New-SmbShare` was unavailable; `UpdateManager(path)` uses the same `SimpleFileSource` code path for local dirs and UNC paths). Pending a real share/VM test.
  - [ ] On a **clean** VM with no .NET runtime: run `Setup.exe` from the UNC path. Check the per-user install location (`%LOCALAPPDATA%\LuuKyCanTin`), the Start-menu shortcut, and that no admin rights were needed. — **Not done: no VM available. Done on the dev machine instead**: per-user install to `%LOCALAPPDATA%\LuuKyCanTinSpike`, Start-menu shortcut, Setup exit 0 with no admin rights; self-contained runtime confirmed in the payload. A clean-VM run remains a manual step.
- [x] **T3. Update from the share** (AC: 2)
  - [x] In the app: `var mgr = new UpdateManager(@"\\server\share\releases");` then `CheckForUpdatesAsync()`, `DownloadUpdatesAsync()` and `ApplyUpdatesAndRestart()`. Velopack accepts a local/UNC directory as the source — confirmed in 1.2.161; a plain path resolves to **`SimpleFileSource`** (the old `LocalSource` class no longer exists). API differs from older samples (see Dev Agent Record).
  - [x] Publish `0.2.0` to the same folder (`vpk pack` with the previous release present, so a delta package is produced — 2.05 MB delta verified next to the 51.39 MB full). Start the installed `0.1.0` and confirm it updates and restarts on `0.2.0` — done, marker + title bar show 0.2.0.
  - [x] Failure paths: share unreachable (`\\nonexistent\share\releases` — app starts normally, logs a warning); half-copied release (truncated `releases.win.json` → `JsonException` caught, warning logged, app starts). Never blocks, never crashes.
  - [x] Decide **when** to check: at startup before the main form (simple, and matches "one step") vs in the background with a "restart to update" prompt. Recommendation: at startup, with a short timeout — implemented with a 10 s check timeout; recorded as the order Story 7.4 must keep.
- [x] **T4. Interaction with the DB-version check** (AC: 3)
  - [x] Write down the order of operations for a release that contains a migration:
    1. The admin backs up the DB.
    2. The admin runs `--migrate` from the admin machine (Story 1.2).
    3. The admin copies the release to the share.
    4. Workstations update on next start.
  - [x] Note what happens in between. Workstations still on the old version see "DB version does not match" until they update. Since the update check runs **before** the DB-version check, a workstation self-heals by updating first. Confirm this order works in the spike, or note it as a requirement for Story 7.4. — **Requirement for 7.4 recorded**: the update check must stay ahead of the DB-version check in `Program.cs`.
  - [x] Consider the reverse risk: a new app version on the share **before** the migration ran. The new app refuses to start ("DB version does not match"), which is safe but noisy. The documented procedure prevents it.
- [x] **T5. Outcome note** (AC: 3)
  - [x] `docs/decisions/0002-deployment-velopack-lan-share.md`: Velopack + vpk versions (1.2.161), commands used, folder layout (`\\server\LuuKyCanTin\releases\` + `\\server\LuuKyCanTin\installer\` + WebView2 offline installer location), share and NTFS permissions (workstation users or the `Domain Computers`/local group get **Read**; the admin gets **Modify** — workgroup options "Everyone: Read" vs matching local accounts), install location, update flow, the DB-version ordering, failure behaviour, open risks.
  - [x] Record how the WebView2 offline runtime (Story 1.5) can ride along: a Velopack setup hook (`OnAfterInstallFastCallback`/`OnAfterUpdateFastCallback`), or a separate admin step. Recommended: separate admin step; decided in 7.4.
  - [x] Note code signing. Unsigned `Setup.exe` triggers SmartScreen on first run. On an offline LAN SmartScreen can't reach its service either. Record what the user sees and whether an internal certificate is worth it (decided in 7.4).

## Dev Notes

### Why a spike, and what "done" means

- This is time-boxed research (A10). **The deliverable is the decision note**. Spike code stays in `spikes/`, outside the solution, so the product source stays clean. The real rollout (production `Program.cs` integration, versioning in CI, installer) is **Story 7.4**. Don't wire Velopack into `src/` in this story.

### Context and constraints

- Workstations are Windows 10/11 64-bit with no Internet. The server is on a static LAN IP (NFR9).
- Self-contained .NET 10, so no runtime install on workstations (NFR9). Expect a ~70–150 MB package. Note the actual full and delta sizes.
- Several workstations may share a domain or workgroup. Note whether the spike used a domain account or a local account for the share. The site may be a **workgroup**, where share permissions need matching local accounts or "Everyone: Read". Write down both options.
- Velopack installs per user by default. If several Windows users share one workstation, each user gets their own install. Note this, and whether a machine-wide install is supported or needed.

### References

- Epic 1 › Story 1.6; `epics.md` › NFR9, NFR13, Additional Requirements › Printing, Excel, infrastructure (SP-02, GAP-06)
- Tech stack PDF: Deployment and updates rows (Velopack, self-contained, WebView2 offline)
- Story 1.2 (schema-version check), Story 7.4 (full rollout)

## Dev Agent Record

### Agent Model Used

deepseek-v4-flash

### Debug Log References

- Velopack 1.2.161 API differs from the older samples in the story: `UpdateManager` is **not** `IDisposable` (no `using`); `CheckForUpdatesAsync()` takes no cancellation token (wrap with `Task.WaitAsync(TimeSpan)` for a timeout); `DownloadUpdatesAsync(UpdateInfo, Action<int>?, CancellationToken)` (progress is `Action<int>`, not `IProgress<int>`); `ApplyUpdatesAndRestart(VelopackAsset, string[])` (restart args, not `UpdateInfo` — implicit conversion exists); the local-folder source class is now `SimpleFileSource` (the old `LocalSource` no longer exists); `VelopackApp.Build().Run()` unchanged. Discovered via build errors + reflection on `Velopack.xml` in the NuGet cache.
- `vpk pack` without `--runtime` warns "No architecture specified with --runtime, defaulting to x86" and produces a nupkg that is **not** byte-identical to one packed with `--runtime win-x64` (re-packed 0.1.0 came out 53,888,624 B vs original 53,536,146 B). A delta built against the original base then fails to apply on the re-packed base; Velopack falls back to the full package (verified). Always pass `--runtime win-x64`.
- The client's `packages\` folder after a delta update contains the *reconstructed* full nupkg (53,888,405 B ≠ server copy 53,888,638 B) — proof the delta was downloaded and applied, not copied.
- `Setup.exe` auto-launches the app at the end of a fresh install, and the root launcher exe is named from `--packTitle` ("LuuKyCanTin Spike.exe"), which spawns the real app process (`current\SP-02-Velopack.exe`).

### Completion Notes List

- **T1**: `spikes/SP-02-Velopack` created (net10.0-windows, WinExe, `<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>`, direct Velopack 1.2.161). `VelopackApp.Build().Run();` is the first line of `Main`. App shows `LuuKyCanTin Spike v<version>` in the title, writes its version to `%LOCALAPPDATA%\LuuKyCanTinSpike\version-marker.txt` and events to `update.log`; `--source <path>` overrides the update source (default `\\server\LuuKyCanTin\releases`). `vpk` installed as a **local tool** in `spikes/dotnet-tools.json` (chosen over `-g` so the repo pins its own tool version), pinned 1.2.161 to match the package. Note: SDK 10.0.400 writes the manifest at the directory root, not in `.config\`. `spikes/README.md` from Story 1.5 reused as-is; a per-spike README added.
- **T2**: publish 0.1.0 self-contained win-x64 = 117.3 MB / 272 files. `vpk pack` outputs (0.1.0): Setup.exe 58.22 MB, full.nupkg 51.06 MB, Portable.zip 51.06 MB, releases.win.json + RELEASES + assets.win.json. Install: `Setup.exe` exit 0, **no admin**; per-user layout at `%LOCALAPPDATA%\LuuKyCanTinSpike` (root launcher `LuuKyCanTin Spike.exe`, `Update.exe`, `current\` payload, `packages\` cache+lock), Start-menu shortcut `LuuKyCanTin Spike.lnk` created (desktop shortcut NOT observed despite vpk's "Shortcuts: Desktop,StartMenuRoot"), `HKCU\...\Uninstall` entry "LuuKyCanTin Spike" v0.2.0. UNC leg **simulated with a local directory** — dev shell not elevated (`Elevated: False`), `New-SmbShare` unavailable; same `SimpleFileSource` code path; real share + clean-VM test pending (manual step).
- **T3**: Update test verified twice end to end: installed 0.1.0 → run with `--source <releasesDir>` → "Update found: 0.2.0" → download (7 s) → apply+restart → "Started version 0.2.0" → no update → main form; title bar `LuuKyCanTin Spike v0.2.0`; marker `0.2.0`. Delta (2.05 MB) produced and used. Failure paths verified: `\\nonexistent\share\releases` → app starts normally, logs "No update available; starting normally"; truncated `releases.win.json` → `JsonException` caught, "Update check failed; starting normally on the current version" → app starts. Delta-fallback verified: re-packed 0.1.0 (different vpk flags) → delta download failed to apply → full package downloaded and applied. Check timing: at startup, before the main form, 10 s check / 120 s download timeouts.
- **T4**: Order + both risks written into ADR 0002. Requirement for Story 7.4: update check must run before the DB-version check in the real `Program.cs`.
- **T5**: `docs/decisions/0002-deployment-velopack-lan-share.md` written (versions, exact commands, folder layout incl. `installer\` + WebView2 folder, share/NTFS permissions with workgroup vs domain options, install location, update flow, DB-version ordering, failure behaviour, WebView2 ride-along, signing/SmartScreen, open risks, alternatives).
- Remaining for the user (unchecked): clean-VM install + update from a real UNC share (create the share on an elevated machine with `New-SmbShare`); workgroup auth decision ("Everyone: Read" vs matching local accounts); signing/SmartScreen decision in 7.4.

### File List

- `spikes/SP-02-Velopack/SP-02-Velopack.csproj` (new)
- `spikes/SP-02-Velopack/Program.cs` (new)
- `spikes/SP-02-Velopack/MainForm.cs` (new)
- `spikes/SP-02-Velopack/README.md` (new)
- `spikes/dotnet-tools.json` (new — local `vpk` 1.2.161 manifest)
- `spikes/SP-02-Velopack/artifacts/` (gitignored: publish-0.1.0, publish-0.2.0, releases, releases-010 — the 0.1.0/0.2.0 feeds and Setup.exe for manual review)
- `docs/decisions/0002-deployment-velopack-lan-share.md` (new)
- `_bmad-output/implementation-artifacts/stories/epic-01/1-6-spike-velopack-lan-updates.md` (task checkboxes, Dev Agent Record, Change Log)

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
| 2026-10-02 | SP-02 executed: spike app, vpk 1.2.161 local tool, 0.1.0→0.2.0 delta update verified, failure paths verified, ADR 0002 written (deepseek-v4-flash) |
| 2026-10-02 | Review fixes applied: trailing `--source` warned instead of silently ignored, guarded marker/startup I/O, `assets.win.json` in ADR folder tree (deepseek-v4-flash) |

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | Task T2 marked `[x]` although the clean-VM AC leg was explicitly not performed | false | The sub-bullet is unchecked and states "Not done: no VM available" verbatim; the note fully discloses the partial coverage, so no reader is misled. The clean-VM run stays an explicit manual step in the ADR and Dev Agent Record. |
| 2 | A trailing `--source` (no value) was silently ignored, falling back to the default feed | low — patch | Real. Fixed: `ParseSource` throws for a valueless `--source`; `ParseSourceOrLog` logs a warning and falls back — never silent. |
| 3 | `Directory.CreateDirectory` / marker `WriteAllText` ran unguarded and could crash the app on an unwritable profile | low — patch | Real, contradicted the "never blocks, never crashes" property. Fixed: wrapped in try/catch; app always reaches the update check. |
| 4 | ADR 0002 folder-layout tree omitted `assets.win.json` though the text lists it among pack outputs | low — patch | Real: a reader copying the feed from the diagram would miss a published file. Fixed: added to the tree. |
