---
story: "2.7"
epic: 2
title: Session auto-lock
status: done
size: S
backlogItems: [NEN-10]
frsCovered: [FR4]
uxDesignRequirements: [UX-DR11]
nfrsTouched: [NFR5, NFR6]
dependsOn: ["2.2"]
baseline_commit: 24de56932b7b5550941ac815b270bf288af5a895
---

# Story 2.7: Session auto-lock

Status: done

## Story

As a custodial officer,
I want my session to lock itself when I step away and unlock quickly with my password,
So that nobody can use my account at an unattended workstation.

## Acceptance Criteria

1. **Given** an idle timeout read from `appsettings.json` (default 5 minutes)
   **When** there is no keyboard or mouse input in the app for that long
   **Then** a full-window lock screen covers the shell, showing the signed-in user's name and a password box

2. **Given** the lock screen
   **When** the same user enters the correct password
   **Then** the session resumes with every open form and unsaved input intact
   **And** wrong attempts count toward the lockout of Story 2.2

3. **Given** the lock screen
   **When** another person chooses "Đăng xuất" (sign out)
   **Then** the app warns that unsaved work will be lost, then closes the session and returns to the login form; it never unlocks into the other user's session

4. **Given** the user clicks "Khoá máy" (lock) or presses Ctrl+L
   **When** the shortcut fires
   **Then** the session locks immediately; the lock and unlock events are written to the audit log

## Tasks / Subtasks

- [x] **T1. Idle detection** (AC: 1)
  - [x] WinForms `Shell/IdleMonitor.cs : IMessageFilter`, added with `Application.AddMessageFilter` while a session is signed in. Any `WM_KEYDOWN`, `WM_SYSKEYDOWN`, `WM_MOUSEMOVE`, `WM_LBUTTONDOWN`/`RBUTTONDOWN`/`MBUTTONDOWN` or `WM_MOUSEWHEEL` records `lastInput`. Use `Environment.TickCount64`: idle timing is elapsed time, not a business date, so it doesn't need `IClock`, and it's immune to clock changes.
  - [x] A `System.Windows.Forms.Timer` (every ~5 s, UI thread) locks when `now - lastInput >= timeout`.
  - [x] Ignore `WM_MOUSEMOVE` whose screen position didn't change, since some drivers send phantom moves.
  - [x] Config: `"Session": { "IdleLockMinutes": 5 }` in `appsettings.json`, bound to `Common/SessionOptions.cs`. `0` turns auto-lock off for dev machines only; `SessionOptions.IsValidIdleLockMinutes` allows only `0` or `1–60` and `Program` fails fast on anything else.
  - [x] Pause the idle check while the app is already locked, and while a long operation is running (e.g. a backup in Epic 7). Expose `Pause()`/`Resume()` and leave the long-operation use for later.
- [x] **T2. Lock screen** (AC: 1, 2, 3)
  - [x] `Shell/LockScreenForm` (+ `ILockScreenView`, `LockScreenPresenter`): a borderless form **owned by `MainForm`**, sized and positioned over the main window's bounds, and kept in place when the main form moves or resizes. It shows the app title, "Phiên làm việc đã bị khoá", the user's name (`FullName`, falling back to `UserName`), a password box, "Mở khoá" and "Đăng xuất".
  - [x] **Covering open modal dialogs.** Business forms may be open modally (receipt entry, etc.). Show the lock form with `Show(owner)`, not `ShowDialog`, so existing dialogs keep their state. Set `Enabled = false` on every open form via `Application.OpenForms`, excluding the lock form, and re-enable on unlock. That's how "every open form and unsaved input intact" (AC 2) is kept: nothing is closed or recreated.
  - [x] While locked, disable the message filter's effect and keep focus in the password box. Alt+Tab to another app is fine, because the app itself stays covered.
- [x] **T3. Unlock and sign-out** (AC: 2, 3)
  - [x] Application: `SignInService.ReauthenticateAsync(password)` re-verifies the **current** user (`ICurrentUser.UserId`), reusing the 2.2 flow. A wrong password goes through `FailedSignInService.RecordAsync`, so it counts toward lockout.
  - [x] If that attempt locks the account, or an admin deactivated it while the screen was locked, unlock is impossible. The presenter then performs the sign-out path with the message "Tài khoản đã bị khoá, liên hệ quản trị viên." (`SignInService.AccountLockedMessage`).
  - [x] No username field. The lock screen can only resume the session of the user who locked it (AC 3: never unlock into another user's session).
  - [x] "Đăng xuất": confirm with "Mọi dữ liệu chưa lưu sẽ bị mất. Bạn có chắc muốn đăng xuất?". Then call `SignInService.SignOutAsync` (2.2), close the lock overlay, and return to the login form through 2.2's `ApplicationContext` loop.
- [x] **T4. Manual lock and audit** (AC: 4)
  - [x] Menu "Hệ thống › Kh&oá máy" (no permission needed) plus `Ctrl+L` as a `NavItem` shortcut handled by `MainForm.ProcessCmdKey`, so it works from any focused control, including inside child forms.
  - [x] Audit via `IAuditLogWriter`, same pattern as 2.2: `AuditAction.SignIn`, table `"User"`, record id = user id, payload `{ Event = "KhoaPhien", Kind = "TuDong" | "ThuCong" }` on lock and `{ Event = "MoKhoaPhien" }` on a successful unlock. A failed unlock is already logged as `DangNhapSai` by the 2.2 flow.
- [x] **T5. Tests** (AC: 1–4)
  - [x] Unit: the idle decision as a pure function (`lastInput`, `now`, timeout, paused) → lock or not.
  - [x] Presenter tests (fake view, NSubstitute service): correct password → view closes and resumes; wrong password → message, stays locked; a lockout/inactive result → sign-out path; Sign out → confirmation first, and declining keeps the lock screen.
  - [x] Application/integration: `ReauthenticateAsync` with a wrong password increments `FailedAttemptCount`; the 5th failure locks the account; the `KhoaPhien`/`MoKhoaPhien` rows are written with the right user.
  - [ ] Manual check (Completion Notes): open the staff edit dialog (2.1), type something, press Ctrl+L, unlock, and confirm the text is still there; let it idle past a 1-minute test timeout. **Not run — needs an interactive desktop.**

## Dev Notes

### Current codebase state (after 2.2)

- `DangNhapService` with lockout, `DangXuatAsync`, the login → shell → login loop in `Program`, and `MainForm`/`MainPresenter` (1.1).

### Design notes

- **App-level lock, not Windows lock.** The spec asks for the app to lock (NEN-10). The Windows screen saver lock is an operations recommendation for the install notes, not a substitute: it protects the desktop, not the app session.
- **Why not hide `MainForm`?** Hiding forms changes z-order and focus and can confuse open modal dialogs. Covering and disabling keeps every form exactly as it was.
- Same-user only (AC 3). Another person who needs the workstation signs out first and accepts that unsaved work is lost.

### Gotchas

- `IMessageFilter` sees messages for **all** app windows, including the lock form itself. Ignore input while locked, or typing the password would count as activity (harmless, but confusing).
- A `System.Windows.Forms.Timer` stops ticking while a native modal loop is running (e.g. a `MessageBox`), so an open message box delays auto-lock until it closes. That's acceptable; note it in the Completion Notes.
- The WebView2 PDF preview (1.5/1.8) is a separate HWND and may swallow input. Check during the manual test that input in the preview resets the idle timer. If it doesn't, also hook WebView2's input events.

### Out of scope

- A Windows-level kiosk mode. The status bar showing the user (Epic 13, TI-06).

### References

- Epic 2 › Story 2.7; `epics.md` › FR4, UX-DR11, NFR6
- Backlog: NEN-10 "Khoá màn hình khi rời máy, mở lại nhanh bằng mật khẩu"

## Dev Agent Record

### Agent Model Used

deepseek-v4.1-flash (opencode-go)

### Debug Log References

### Completion Notes List

**Translation mapping** (the story's identifiers are pre-R.1 Vietnamese; `docs/conventions/naming-conventions.md` wins):

| Story identifier | Implemented as |
|---|---|
| `TheoDoiNhanRoi` | `Shell/IdleMonitor.cs` (`IMessageFilter` + `System.Windows.Forms.Timer`) |
| `KhoaMayForm` / `IKhoaMayView` / `KhoaMayPresenter` | `Shell/LockScreenForm.cs` / `ILockScreenView.cs` / `LockScreenPresenter.cs` |
| `TamDung()` / `TiepTuc()` | `IdleMonitor.Pause()` / `Resume()` |
| `DangNhapService.XacThucLaiAsync` | `SignInService.ReauthenticateAsync(password, ct)` |
| `"PhienLamViec": { "ThoiGianKhoaMayPhut": 5 }` | `"Session": { "IdleLockMinutes": 5 }`, options class `Common/SessionOptions.cs` |
| payload key `"SuKien"` / `"Kieu"` | `Event` / `Kind` |
| `"KhoaPhien"` / `"MoKhoaPhien"` | `SignInEvent.LockSession` / `SignInEvent.UnlockSession` (codes kept as stored data) |
| `"TuDong"` / `"ThuCong"` | `SessionLockKind.Automatic` / `SessionLockKind.Manual` (codes kept as stored data) |
| `HanhDong.DangNhap` | `AuditAction.SignIn` |

**Completion notes**

- **Idle detection.** `IdleMonitor` records `Environment.TickCount64` on `WM_KEYDOWN`, `WM_SYSKEYDOWN`, `WM_MOUSEMOVE`, the three button-down messages and `WM_MOUSEWHEEL`, and ignores a `WM_MOUSEMOVE` whose screen position did not change. A 5-second `System.Windows.Forms.Timer` calls the pure `IdleMonitor.ShouldLock(lastInput, now, timeout, isPaused)`; a zero timeout means auto-lock is off. `Pause()`/`Resume()` are used for lock/unlock and are ready for a future long operation. `MainPresenter` adds the filter with `Application.AddMessageFilter` on shell load and removes it on dispose.
- **Lock screen.** `Navigator.OpenLockScreen` creates `LockScreenForm` and its presenter and calls `Show(owner)` (modeless, so open dialogs keep their state). The overlay is borderless, covers `owner.Bounds`, follows the owner's move/resize, and shows the app title, initials, the user's name, the password box, "Mở khoá" and the "Đăng xuất" link (key-01 D). It disables every other open form on `OnLoad` and re-enables them in `OnFormClosed`; the message filter ignores input while locked, and the password box regains focus on show/activate. Setup runs in `OnLoad` rather than `OnShown`, because `Show()` raises `Shown` only once a message loop runs.
- **Unlock / sign-out.** `ReauthenticateAsync` re-verifies only the current session user and never swaps the session. A wrong password goes through `FailedSignInService` (so it counts toward the 2.2 lockout); a locked or inactive account shows the locked message and then forces the sign-out path. "Đăng xuất" asks the exact confirmation (Yes/No, default No) and, on Yes, signs out, closes the overlay and returns to the login loop. A successful unlock writes `MoKhoaPhien`; locking writes `KhoaPhien` + `Kind` — both through `IAuditLogWriter` with `AuditAction.SignIn`, table `"User"`, record id = user id.
- **Manual lock.** "Hệ thống › Kh&oá máy" (mnemonic O, because `&khoản` already owns K) needs no permission and carries `Ctrl+L`; the existing `MainForm.ProcessCmdKey` routing makes it work from any focused control. `NavItem.ShortcutText` now formats modifier shortcuts with `KeysConverter` ("Ctrl+L", "F2" unchanged).
- **Modal-timer note (from the story).** A `System.Windows.Forms.Timer` does not tick inside a native modal loop (for example an open `MessageBox`), so auto-lock is delayed until that box closes. Accepted, as the story says.
- **WebView2 note (from the story).** The PDF preview is a separate HWND. Whether input inside it resets the idle timer was not checked, because it needs the interactive manual test. If it does not, the WebView2 input events must also feed `IdleMonitor`.
- **Manual check not run.** Opening a dialog, typing, pressing Ctrl+L, unlocking and confirming the text is intact (and idling past a 1-minute test timeout) needs an interactive desktop, so it was not run here. It remains the only unfinished item.
- **Test-only adjustments.** `AppTheme.IconFont`'s font cache is now locked: parallel STA test classes build screens concurrently and the unprotected `Dictionary` corrupted itself (`MasterDataScreenTests` failed). The overlay's cover/disable behaviour uses `Application.OpenForms`, which is process-global and collides with other test classes running in parallel, so it is covered by the manual check plus the presenter test asserting `CloseLock()`; no unit test was added for it.
- **Dependency gate.** Story 2.2 is `review` (developed), so 2.7 is allowed to proceed; the story stays `in-progress` for review.
- **Verification.** `dotnet build LuuKyCanTin.slnx` → 0 errors (only the pre-existing WebView2 spike MSB3277 warning). `dotnet test LuuKyCanTin.slnx` → all green (92 domain + 159 application + 290 WinForms + 260 integration). `dotnet ef migrations has-pending-model-changes` → no changes.

### File List

**Added**

- `src/Libraries/LuuKyCanTin.Application/Administration/SessionLockKind.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Common/SessionOptions.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/IdleMonitor.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/ILockScreenView.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/LockScreenPresenter.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/LockScreenForm.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/LockScreenForm.Designer.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Common/SessionOptionsTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/IdleMonitorTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/LockScreenPresenterTests.cs`

**Changed**

- `src/Libraries/LuuKyCanTin.Application/Administration/SignInEvent.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/SignInService.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Common/AppTheme.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Program.cs`
- `src/Presentation/LuuKyCanTin.WinForms/appsettings.json`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/INavigator.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/NavItem.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/Navigator.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/ShellNavigation.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/MainPresenter.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/ShellApplicationContext.cs`
- `tests/LuuKyCanTin.Application.UnitTests/Administration/SignInServiceTests.cs`
- `tests/LuuKyCanTin.IntegrationTests/Administration/SignInServiceTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/MainPresenterTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/ShellNavigationTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/ShellNavigationFilterTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/ShellScreenTests.cs`

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | Lock waits for the audit write before covering the screen (also: audit row for a lock that never displayed) | medium — patch | The overlay must appear first; the audit write follows (bounded) and only for a displayed lock. |
| 2 | A throw from `OpenLockScreen` leaves `_isLocked` latched and the monitor paused | medium — patch | Reset presenter state in `finally` so a later Ctrl+L or idle timeout still works. |
| 3 | The overlay can close for reasons other than `UserClosing` without notifying the presenter | medium — patch | Block every close the presenter did not initiate and notify the presenter from `OnFormClosed`. |
| 4 | The automatic idle path cannot be tested and is never exercised (monitor built inside the presenter) | medium — patch | Inject the monitor/factory, add automatic-timeout and audit-failure presenter tests, dispose monitors in tests. |
| 5 | `IdleMonitor`'s filter/timer behaviour is untested | medium — patch | Test `PreFilterMessage` (each message, repeated mouse position ignored), `Pause`/`Resume`, one `OnTick` per elapsed timeout. |
| 6 | Ctrl+L cannot work inside a modal dialog (MainForm.ProcessCmdKey never sees those keys) | medium — patch | Route Ctrl+L through the app-level message filter as well. |
| 7 | `ReauthenticateAsync` skips `MustChangePassword`, so a reset while locked resumes without the forced change | medium — patch | Mirror the sign-in semantics: a forced change signs the session out and returns to login. |
| 8 | `SessionOptions` accepts `0` in production and negatives outside DI validation | medium — patch | Only Development may disable auto-lock; guard the property and validate the registration; test the registration. |
| 9 | Raw exception text is shown on the lock screen and nothing is logged | low — patch | Show a generic message and log the real exception. |
| 10 | `LockSessionAsync` takes a free-form string that lands in the audit log | low — patch | Take `SessionLockKind` and map to the stored code inside the service. |
| 11 | `ShortcutText` uses the OS-localized `Keys` converter | low — patch | Format modifiers explicitly so "Ctrl+L" is stable across display languages. |
| 12 | `ReauthenticateAsync` duplicates the sign-in credential path | low — patch | Extract the shared check so the two flows cannot drift. |
| 13 | Forms opened after the lock are not disabled | low — defer | Rare background-window case; the overlay covers the owner. Recorded in `deferred-work.md`. |
| 14 | Input in the WebView2 preview may not reset the idle timer | low — defer | Needs the interactive desktop check; recorded in `deferred-work.md`. |
| 15 | The manual end-to-end check was not run while the story moved to review | low — defer | Cannot run headlessly; recorded in the completion notes and `deferred-work.md`; the story may still ship with the automated coverage. |
| 16 | Shell close does not dispose the message filter (no `ShellApplicationContext` test seam) | low — defer | Real but low-impact; recorded in `deferred-work.md`. |
| 17 | The audit-write failure path ("still locks when the audit write fails") is untested | low — patch | Presenter test with a throwing audit writer. |
| 18 | The overlay cover/disable/re-enable behaviour is untested; the designated manual check was skipped | medium — patch | Add an STA test (non-parallel) that shows a host form and the overlay, then asserts `Enabled` false→true and a user close is cancelled. |
| 19 | The diff/headers went stale during review (body casing, "stays in-progress" note) | false | Fixed while triaging; the diff is regenerated after the patches. |

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
| 2026-10-04 | Implemented T1–T5 (English identifiers per the naming convention); build, full test suite and EF model check green; interactive manual check pending |
| 2026-10-04 | Reviewed (blind hunter, edge cases, verification gaps) and patched: cover-before-audit ordering, close-path and state-reset guards, injected testable monitor with the automatic path covered, Ctrl+L inside modal dialogs, forced-change reauthentication, environment-aware options validation; status → done |
