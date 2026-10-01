# SP-02 — Velopack updates from a LAN shared folder

Throw-away WinForms spike (`net10.0-windows`, self-contained `win-x64`) that proves
workstations can install and update LuuKyCanTin from a UNC share with no Internet.
Outcome: `docs/decisions/0002-deployment-velopack-lan-share.md`.

Not part of `LuuKyCanTin.slnx`, not referenced from `src/`. The real rollout is Story 7.4.

## What the app does

- `VelopackApp.Build().Run();` is the **first line** of `Main` (Velopack hooks may exit the process).
- Shows `LuuKyCanTin Spike v<version>` in the title bar; writes its version to
  `%LOCALAPPDATA%\LuuKyCanTinSpike\version-marker.txt` and appends events to `update.log`.
- Update source: `--source <path>` on the command line (default `\\server\LuuKyCanTin\releases`).
- At startup (before the main form, 10 s timeout): `CheckForUpdatesAsync` →
  `DownloadUpdatesAsync` → `ApplyUpdatesAndRestart`. ANY failure is logged as a warning
  and the app continues on the current version — never blocks, never crashes.

## Commands used (all verified 2026-10-02, Velopack/vpk 1.2.161)

```powershell
# publish
dotnet publish SP-02-Velopack.csproj -c Release -r win-x64 --self-contained true -p:Version=0.1.0 -o artifacts/publish-0.1.0

# pack (run from spikes/, the local tool manifest is spikes/dotnet-tools.json)
dotnet tool run vpk pack --packId LuuKyCanTinSpike --packVersion 0.1.0 --packDir artifacts/publish-0.1.0 `
  --mainExe SP-02-Velopack.exe --packAuthors "LuuKyCanTin Spike" --packTitle "LuuKyCanTin Spike" `
  --runtime win-x64 --outputDir artifacts/releases

# install (per-user, no admin)
.\artifacts\releases\LuuKyCanTinSpike-win-Setup.exe

# run the installed app with a local folder standing in for the UNC share
& "$env:LOCALAPPDATA\LuuKyCanTinSpike\LuuKyCanTin Spike.exe" --source D:\...\artifacts\releases
```

Always pass `--runtime win-x64` to `vpk pack` (without it vpk warns "defaulting to x86" and the
produced nupkg differs, which breaks delta compatibility with earlier packs of the same version).

## Results (sizes)

| Artifact | 0.1.0 | 0.2.0 |
|---|---|---|
| publish dir | 117.3 MB (272 files) | 117.3 MB |
| `*-full.nupkg` | 51.06 MB | 51.39 MB |
| `*-delta.nupkg` | — | **2.05 MB** (0.1.0 → 0.2.0) |
| `*-win-Setup.exe` | 58.22 MB | 58.56 MB |
| `*-win-Portable.zip` | 51.06 MB | 51.39 MB |

Update test: installed 0.1.0 → ran with `--source` → detected 0.2.0, applied (delta used),
restarted, title + marker show 0.2.0. Failure paths (unreachable UNC, malformed feed): app
starts normally and logs a warning.

See `artifacts/releases/` for the feed and `docs/decisions/0002-...` for the full write-up.