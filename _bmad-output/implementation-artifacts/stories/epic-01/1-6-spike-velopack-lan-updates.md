---
story: "1.6"
epic: 1
title: "Spike: Velopack updates from a LAN shared folder"
status: ready-for-dev
type: spike
size: M (time-boxed, 1.5 days suggested)
backlogItems: [SP-02]
frsCovered: []
nfrsTouched: [NFR7, NFR9, NFR13]
dependsOn: ["1.1"]
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

- [ ] **T1. Throw-away spike app, outside the product code** (AC: 1)
  - [ ] `spikes/SP-02-Velopack/`: a minimal WinForms `net10.0-windows` app showing its version in the title. It's **not** in `LuuKyCanTin.slnx` and not referenced from `src/`. Reuse `spikes/README.md` from Story 1.5 if it exists.
  - [ ] Add the `Velopack` NuGet package. Make `VelopackApp.Build().Run();` the **first line** of `Main`, before `ApplicationConfiguration.Initialize()`. Velopack handles install/update hooks there and may exit the process.
  - [ ] Install the CLI: `dotnet tool install -g vpk` (or a local tool manifest).
- [ ] **T2. Package and install from a UNC share** (AC: 1)
  - [ ] `dotnet publish -c Release -r win-x64 --self-contained true -o publish`, then `vpk pack --packId LuuKyCanTin --packVersion 0.1.0 --packDir publish --mainExe <exe>`. Note the outputs (`Setup.exe`, `*-full.nupkg`, `releases.win.json` / `RELEASES`).
  - [ ] Copy the output to a share (simulate with `\\<devmachine>\LuuKyCanTinReleases` or a Hyper-V/Windows Sandbox host share).
  - [ ] On a **clean** VM with no .NET runtime: run `Setup.exe` from the UNC path. Check the per-user install location (`%LOCALAPPDATA%\LuuKyCanTin`), the Start-menu shortcut, and that no admin rights were needed.
- [ ] **T3. Update from the share** (AC: 2)
  - [ ] In the app: `var mgr = new UpdateManager(@"\\server\share\releases");` then `CheckForUpdatesAsync()`, `DownloadUpdatesAsync()` and `ApplyUpdatesAndRestart()`. Velopack accepts a local/UNC directory as the source. Confirm this in the version you use and record which source class handles it.
  - [ ] Publish `0.2.0` to the same folder (`vpk pack` with the previous release present, so a delta package is produced). Start the installed `0.1.0` and confirm it updates and restarts on `0.2.0`.
  - [ ] Failure paths: share unreachable (the app must start normally on the current version and log a warning, never block or crash); a half-copied release (what happens if the admin is still copying files when a workstation checks?).
  - [ ] Decide **when** to check: at startup before the main form (simple, and matches "one step") vs in the background with a "restart to update" prompt. Recommendation: at startup, with a short timeout.
- [ ] **T4. Interaction with the DB-version check** (AC: 3)
  - [ ] Write down the order of operations for a release that contains a migration:
    1. The admin backs up the DB.
    2. The admin runs `--migrate` from the admin machine (Story 1.2).
    3. The admin copies the release to the share.
    4. Workstations update on next start.
  - [ ] Note what happens in between. Workstations still on the old version see "DB version does not match" until they update. Since the update check runs **before** the DB-version check, a workstation self-heals by updating first. Confirm this order works in the spike, or note it as a requirement for Story 7.4.
  - [ ] Consider the reverse risk: a new app version on the share **before** the migration ran. The new app refuses to start ("DB version does not match"), which is safe but noisy. The documented procedure prevents it.
- [ ] **T5. Outcome note** (AC: 3)
  - [ ] `docs/decisions/0002-deployment-velopack-lan-share.md`: Velopack + vpk versions, commands used, folder layout (`\\server\LuuKyCanTin\releases\` + `\\server\LuuKyCanTin\installer\` + WebView2 offline installer location), share and NTFS permissions (workstation users or the `Domain Computers`/local group get **Read**; the admin gets **Modify**), install location, update flow, the DB-version ordering, failure behaviour, open risks.
  - [ ] Record how the WebView2 offline runtime (Story 1.5) can ride along: a Velopack setup hook, or a separate admin step.
  - [ ] Note code signing. Unsigned `Setup.exe` triggers SmartScreen on first run. On an offline LAN SmartScreen can't reach its service either. Record what the user sees and whether an internal certificate is worth it (decided in 7.4).

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

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
