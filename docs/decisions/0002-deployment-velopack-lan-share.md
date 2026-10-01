# ADR 0002: Deployment — Velopack updates from a LAN shared folder

- **Date:** 2026-10-02 (spike SP-02 / Story 1.6 executed on this date)
- **Status:** Proposed — spike validated; the full rollout is Story 7.4, where signing and
  install-mode decisions are taken
- **References:** Story 1.6 (SP-02), Story 7.4 (full rollout), Story 1.2 (DB/schema-version
  check), ADR 0001 (WebView2 offline runtime), tech stack PDF (Deployment row, Updates row);
  `epics.md` › NFR7 (offline), NFR9 (self-contained, no runtime install), NFR13 (LAN, no
  Internet)

## Context

Workstations are Windows 10/11 x64 on a static-LAN, no Internet, no .NET runtime installed
(planned: self-contained). The admin must roll out new versions in one step — put a folder on
a share, and every workstation picks it up on next start. No API server exists and none is
planned.

Velopack 1.2.161 (NuGet package **and** `vpk` CLI tool, both newest stable, checked 2026-10-02)
was validated in spike SP-02 with a throw-away app (`spikes/SP-02-Velopack`, packId
`LuuKyCanTinSpike` so it can never collide with the real install). Everything below was run
for real on the dev machine (workgroup, local account, **non-elevated** shell); the only steps
not executed on real hardware are flagged.

## Decision

**Use Velopack to install and update LuuKyCanTin from a LAN share.** Self-contained
`net10.0-windows` publish → `vpk pack` → copy `Setup.exe` + the release feed to a share →
workstations install once from `Setup.exe` (per-user, no admin) and self-update on every
start from the same share. No Internet involved at any point.

### Versions

| Component | Version | Notes |
|---|---|---|
| Velopack (NuGet) | 1.2.161 | newest stable (2026-09-29); API below verified against this version |
| vpk (CLI) | 1.2.161 | installed as a **local tool** (`spikes/dotnet-tools.json`), pinned to match the package |
| .NET | SDK 10.0.400, `net10.0-windows`, self-contained `win-x64` | repo `global.json` pins 10.0.201, rollForward latestFeature |

### Commands (exact, as run)

```powershell
dotnet publish spikes/SP-02-Velopack/SP-02-Velopack.csproj -c Release -r win-x64 --self-contained true -p:Version=0.1.0 -o <publishDir>

dotnet tool run vpk pack --packId LuuKyCanTinSpike --packVersion 0.1.0 --packDir <publishDir> `
  --mainExe SP-02-Velopack.exe --packAuthors "LuuKyCanTin Spike" --packTitle "LuuKyCanTin Spike" `
  --runtime win-x64 --outputDir <releasesDir>
```

Outputs per pack: `<PackId>-win-Setup.exe`, `<PackId>-<v>-full.nupkg`,
`<PackId>-<v>-delta.nupkg` (only when the previous full nupkg is already in the output dir),
`<PackId>-win-Portable.zip`, `releases.win.json` (the feed the client reads), `RELEASES`
(legacy index), `assets.win.json`.

**Always pass `--runtime win-x64`.** Without it vpk warns "No architecture specified with
--runtime, defaulting to x86" and the produced nupkg differs, which breaks delta
compatibility: the spike re-packed 0.1.0 with the flag and the delta (0.1.0 → 0.2.0, built
against the original 0.1.0) failed to apply on top of it — Velopack then safely fell back to
the full package (verified). The delta is only as good as its byte-identical base.

Note: `vpk pack` overwrites `<PackId>-win-Setup.exe` in the output dir. The Setup.exe in the
release feed folder always reflects the **latest** version, so the first install must come
from a Setup.exe of the intended version (the admin copies it to `installer\` once).

### Measured sizes (spike)

| Artifact | 0.1.0 | 0.2.0 |
|---|---|---|
| `dotnet publish` dir (self-contained) | 117.3 MB, 272 files | ~117 MB |
| `*-full.nupkg` | 51.06 MB (53,536,146 B) | 51.39 MB (53,888,638 B) |
| `*-delta.nupkg` | — | 2.05 MB (2,149,399 B) |
| `*-win-Setup.exe` | 58.22 MB (61,050,258 B) | 58.56 MB (61,402,750 B) |
| `*-win-Portable.zip` | 51.06 MB | 51.39 MB |

Real product is expected in the same ballpark (70–150 MB per NFR9).

### Folder layout on the server

```
\\server\LuuKyCanTin\
├── installer\                       # first-time install files (admin copy)
│   ├── LuuKyCanTin-win-Setup.exe    # latest Setup.exe (single-file, per-user, no admin)
│   └── WebView2\
│       └── MicrosoftEdgeWebView2RuntimeInstallerX64.exe   # offline runtime (ADR 0001)
└── releases\                        # vpk output dir — the update feed (admin copy, never edited by hand)
    ├── LuuKyCanTin-win-Setup.exe
    ├── LuuKyCanTin-<v>-full.nupkg
    ├── LuuKyCanTin-<v>-delta.nupkg
    ├── LuuKyCanTin-win-Portable.zip
    ├── releases.win.json
    ├── assets.win.json
    └── RELEASES
```

Keep a handful of past full/delta nupkgs in `releases\`: deltas chain from the previous
release, and a workstation that missed an intermediate version needs the pieces (Velopack
falls back to a full download when no applicable delta exists).

### Share and NTFS permissions

- Share + NTFS on `\\server\LuuKyCanTin`:
  - **Workstations: Read** (share) + Read/Execute (NTFS) on `releases\` and `installer\`.
  - **Admin: Modify** (share + NTFS) on `releases\` (and full control on `installer\`).
- Domain environment: grant `Domain Computers` (or a dedicated AD group, e.g.
  `LuuKyCanTin-Workstations`) Read. Machines join with domain accounts → the group covers
  every user on every workstation.
- **Workgroup environment (the site may be one):** there is no domain group to grant. Two
  workable options, both recorded for 7.4:
  1. **"Everyone: Read"** on the share — simplest, acceptable on an offline LAN, but every
     authenticated local account on the LAN can read the release folder (no secret there,
     only binaries).
  2. **Matching local accounts** — create the same username+password locally on server and
     workstations; SMB authenticates local-to-local. Finicky to maintain (password rotation),
     not recommended unless policy forbids "Everyone".
  - The spike machine itself is a **workgroup** machine (PartOfDomain = False) with a local
    account; a real share + workgroup auth test is pending (below).
- The spike could **not** create a real SMB share (`New-SmbShare` needs an elevated shell;
    the dev shell was not elevated). The UNC leg was simulated by pointing the app at a local
    directory (which is the same code path — `SimpleFileSource`). **Pending manual step:** on
    an elevated admin machine, `New-SmbShare -Name LuuKyCanTin -Path D:\LuuKyCanTin -FullAccess <admin> -ReadAccess Everyone`, install from `\\<server>\LuuKyCanTin\installer\Setup.exe` on a
    clean VM, and update from `\\<server>\LuuKyCanTin\releases`.

### Install location and per-user behaviour

- Velopack installs **per user** by default: `%LOCALAPPDATA%\LuuKyCanTinSpike` (real app:
  `%LOCALAPPDATA%\LuuKyCanTin`), a Start-menu shortcut (`…\Start Menu\Programs\LuuKyCanTin
  Spike.lnk` — verified), an uninstall entry under `HKCU\...\Uninstall`, **no admin rights**
  (Setup.exe exit code 0 as a non-elevated user — verified). `Setup.exe` launches the app at
  the end of a fresh install (observed).
- Several Windows users on one workstation each get their own install; each updates itself.
  A machine-wide install (per-machine, admin) is possible but was **not** exercised in the
  spike — decide in 7.4 whether the site needs it (it changes upgrade mechanics: per-machine
  updates need elevation; Velopack supports both, default is per-user).
- vpk logs "Shortcuts: Desktop,StartMenuRoot" but only the Start-menu shortcut appeared in
  the spike; treat the desktop shortcut as not guaranteed.

### Update flow (verified end to end)

App startup order (recommended, and what the spike implements):

1. `VelopackApp.Build().Run();` — **first line** of `Main`, before
   `ApplicationConfiguration.Initialize()`. Handles install/uninstall/update hooks; may exit
   the process.
2. Update check (before the main form): `new UpdateManager(source)` where `source` is the
   UNC folder `\\server\LuuKyCanTin\releases` (command-line `--source` overrides; default
   built into the app). A plain path resolves to **`SimpleFileSource`** (confirmed in 1.2.161
   docs/API; the old `LocalSource` class no longer exists).
3. `CheckForUpdatesAsync()` (10 s timeout in the spike) → if an update exists:
   `DownloadUpdatesAsync(update, progress, token)` (120 s timeout) →
   `ApplyUpdatesAndRestart(update, args)` — exits, applies, relaunches with the same args.
   The relaunched instance re-checks, finds nothing newer, and shows the main form.
4. On **any** failure — unreachable share, malformed `releases.win.json`, timeout — log a
   warning (`update.log`) and continue on the current version. Verified: a dead UNC
   (`\\nonexistent\share\releases`) and a truncated feed both start the app normally with a
   warning logged, never a crash or a hang.

Verified update: installed 0.1.0 → ran with `--source` pointing at the feed → found 0.2.0 →
applied → restarted → title bar and `version-marker.txt` show 0.2.0. The **delta package was
used**: the client's `packages\` folder ended up with a 0.2.0 nupkg whose bytes differ from
the server's copy (53,888,405 vs 53,888,638 — it was reconstructed from the delta, not
copied). If delta application fails, Velopack falls back to downloading the full package
(verified in the re-pack mismatch scenario) — the workstation always converges.

API notes for 1.2.161 (differs from older samples): `UpdateManager` is **not** `IDisposable`;
`CheckForUpdatesAsync()` takes no cancellation token (use `Task.WaitAsync` for a timeout);
`DownloadUpdatesAsync(UpdateInfo, Action<int>?, CancellationToken)`; `ApplyUpdatesAndRestart(VelopackAsset, string[])` (implicit conversion from `UpdateInfo`); `VelopackApp.Build().Run()` unchanged.

### Ordering vs the DB-version check (Story 1.2)

Documented release procedure for a version that ships a migration:

1. Admin backs up the DB.
2. Admin runs `--migrate` (admin machine only; workstations never migrate).
3. Admin copies the release into `\\server\LuuKyCanTin\releases\`.
4. Workstations update on next start.

In between, workstations still on the old app hit "DB version does not match" — expected and
safe. Because the update check runs **before** the DB-version check, a workstation self-heals
on the next start (updates first, then the new app matches the DB). The spike's order — update
first, main form (and everything else) after — is the order 7.4 must keep in `Program.cs`.

Reverse risk: a new app lands on the share **before** the migration ran. The new app refuses
to start ("DB version does not match") — safe but noisy. The documented procedure prevents it.

### WebView2 runtime ride-along (ADR 0001)

The WebView2 Evergreen runtime is delivered separately from the app. On the offline LAN the
admin installs `MicrosoftEdgeWebView2RuntimeInstallerX64.exe` once per workstation (from
`installer\WebView2\`) — simplest and matches "the admin's one visit". Velopack setup hooks
(`OnAfterInstallFastCallback` / `OnAfterUpdateFastCallback` on `VelopackApp.Build()`) can
silently run it, but the runtime installer is a one-time machine-wide thing, not a per-release
app thing, so a **separate admin step is preferred**; decide in 7.4. The spike did not wire a
hook.

### Code signing / SmartScreen

The spike packed unsigned: vpk logs `WRN No signing parameters provided`. An unsigned
`Setup.exe` downloaded from the Internet would trigger SmartScreen; on the LAN the file is
copied from a share (no Mark-of-the-Web by default), and an offline SmartScreen cannot reach
its service anyway — the user may still see an "unknown publisher" prompt depending on policy.
SmartScreen wording and whether an internal certificate is worth it are decided in 7.4; the
spike did not sign.

## Consequences

- Rollout becomes one step: copy the feed, workstations self-update. No per-machine visits
  except the first install and the WebView2 runtime.
- First install still needs one manual step per workstation (`Setup.exe` from the share).
- Per-user installs: every Windows user of a workstation has their own app instance and
  updates; if the unit's workstations are single-user (typical), this is a non-issue.
- Bandwidth: a typical update is ~2 MB (delta) or ~50 MB (full fallback) per workstation on
  the LAN — fine.
- The `releases\` folder must not be edited by hand; the admin copies vpk output. A
  half-copied feed is harmless (client logs a warning and keeps running — verified).
- Startup cost: up to ~10 s (the check timeout) before the main form when the share is
  slow/unreachable; acceptable at the unit's scale, and the production app can show a
  splash/"checking update" window (Story 7.4).

## Open risks

1. **Real UNC share + clean VM untested.** No SMB share could be created (shell not elevated,
   no VM available). The `SimpleFileSource` code path is identical for local dirs and UNC
   paths, and the failure modes (unreachable, half-copied) were verified, but the full
   story — install from `\\server\...\Setup.exe`, update from the share, workgroup auth
   (matching local accounts vs "Everyone: Read") — must be exercised on a clean VM by the
   user before 7.4.
2. **Workgroup authentication model undecided** — "Everyone: Read" vs matching local
   accounts; must be confirmed with the unit's actual setup.
3. **Signing/SmartScreen decision pending** (7.4): unsigned Setup.exe may show an unknown
   publisher prompt on first run.
4. **Machine-wide install mode untested** — only per-user was exercised; needed only if a
   workstation has several Windows users.
5. **Delta fragility**: re-packing an already-released version with different `vpk` flags
   produces a different nupkg and silently disables the delta for that base (fallback to
   full). Procedure: never re-pack a released version; if a release is broken, bump the
   version.
6. **Startup timeout** of 10 s was chosen for the spike; tune for the real app (splash
   window) in 7.4.

## Alternatives

1. **Manual installs / USB sticks** — today's baseline; rejected: every version needs a
   visit to every machine.
2. **HTTP(S) update server on the LAN** (`SimpleWebSource`) — slightly different client code,
   needs an always-on web service; a plain share needs nothing new on the server. Revisit
   only if SMB to workstations becomes a problem.
3. **Winget / MSIX** — needs a service or signing infra and Internet-aligned tooling; heavier
   than the requirement.
4. **No update mechanism** (reinstall from `installer\` each time) — works but loses the
   self-healing one-step rollout that this ADR's flow gives; kept only as the fallback if
   Velopack is rejected in 7.4.