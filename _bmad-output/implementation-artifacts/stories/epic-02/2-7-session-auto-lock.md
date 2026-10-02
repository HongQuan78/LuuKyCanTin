---
story: "2.7"
epic: 2
title: Session auto-lock
status: ready-for-dev
size: S
backlogItems: [NEN-10]
frsCovered: [FR4]
uxDesignRequirements: [UX-DR11]
nfrsTouched: [NFR5, NFR6]
dependsOn: ["2.2"]
---

# Story 2.7: Session auto-lock

Status: ready-for-dev

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

- [ ] **T1. Idle detection** (AC: 1)
  - [ ] WinForms `Shell/TheoDoiNhanRoi.cs : IMessageFilter`, added with `Application.AddMessageFilter` while a session is signed in. Any `WM_KEYDOWN`, `WM_SYSKEYDOWN`, `WM_MOUSEMOVE`, `WM_LBUTTONDOWN`/`RBUTTONDOWN`/`MBUTTONDOWN` or `WM_MOUSEWHEEL` records `lastInput`. Use `Environment.TickCount64`: idle timing is elapsed time, not a business date, so it doesn't need `IClock`, and it's immune to clock changes.
  - [ ] A `System.Windows.Forms.Timer` (every ~5 s, UI thread) locks when `now - lastInput >= timeout`.
  - [ ] Ignore `WM_MOUSEMOVE` whose screen position didn't change, since some drivers send phantom moves.
  - [ ] Config: `"PhienLamViec": { "ThoiGianKhoaMayPhut": 5 }` in `appsettings.json`, bound to an options class. `0` turns auto-lock off for dev machines only; validate the value as `0` or `1–60`.
  - [ ] Pause the idle check while the app is already locked, and while a long operation is running (e.g. a backup in Epic 7). Expose `TamDung()`/`TiepTuc()` and leave it unused for now.
- [ ] **T2. Lock screen** (AC: 1, 2, 3)
  - [ ] `Shell/KhoaMayForm` (+ `IKhoaMayView`, `KhoaMayPresenter`): a borderless form **owned by `MainForm`**, sized and positioned over the main window's bounds, `TopMost` within the app, and kept in place when the main form moves or resizes. It shows the app title, "Phiên làm việc đã bị khoá", the user's name (`ICurrentUser.HoTen`, 2.4, or `TenDangNhap` if 2.4 isn't done yet), a password box, "Mở khoá" and "Đăng xuất".
  - [ ] **Covering open modal dialogs.** Business forms may be open modally (receipt entry, etc.). Show the lock form with `Show(owner)`, not `ShowDialog`, so existing dialogs keep their state. Set `Enabled = false` on every open form via `Application.OpenForms`, excluding the lock form, and re-enable on unlock. That's how "every open form and unsaved input intact" (AC 2) is kept: nothing is closed or recreated.
  - [ ] While locked, disable the message filter's effect and keep focus in the password box. Alt+Tab to another app is fine, because the app itself stays covered.
- [ ] **T3. Unlock and sign-out** (AC: 2, 3)
  - [ ] Application: `DangNhapService.XacThucLaiAsync(matKhau)` re-verifies the **current** user (`ICurrentUser.NguoiDungId`), reusing the 2.2 flow. A wrong password goes through `GhiNhanDangNhapSai`, so it counts toward lockout.
  - [ ] If that attempt locks the account, or an admin deactivated it while the screen was locked, unlock is impossible. The presenter then performs the sign-out path with the message "Tài khoản đã bị khoá, vui lòng liên hệ quản trị viên".
  - [ ] No username field. The lock screen can only resume the session of the user who locked it (AC 3: never unlock into another user's session).
  - [ ] "Đăng xuất": confirm with "Mọi dữ liệu chưa lưu sẽ bị mất. Bạn có chắc muốn đăng xuất?". Then call `DangXuatAsync` (2.2), close every open form, and return to the login form through 2.2's `ApplicationContext` loop.
- [ ] **T4. Manual lock and audit** (AC: 4)
  - [ ] Menu "Hệ thống › Khoá máy" (no permission needed) plus `Ctrl+L` as a `MainForm` `KeyPreview` shortcut / `ProcessCmdKey`, so it works from any focused control, including inside child forms.
  - [ ] Audit via `IGhiNhatKy`, same pattern as 2.2: `HanhDong.DangNhap`, `TenBang = "NguoiDung"`, `BanGhiId` = user id, payload `{ "SuKien": "KhoaPhien", "Kieu": "TuDong" | "ThuCong" }` on lock and `{ "SuKien": "MoKhoaPhien" }` on a successful unlock. A failed unlock is already logged as `DangNhapSai` by the 2.2 flow.
- [ ] **T5. Tests** (AC: 1–4)
  - [ ] Unit: the idle decision as a pure function (`lastInput`, `now`, timeout, paused) → lock or not.
  - [ ] Presenter tests (fake view, NSubstitute service): correct password → view closes and forms re-enable; wrong password → message, stays locked; a lockout result → sign-out path; Sign out → confirmation first, and declining keeps the lock screen.
  - [ ] Application/integration: `XacThucLaiAsync` with a wrong password increments `SoLanSai`; the 5th failure locks the account; the `KhoaPhien`/`MoKhoaPhien` rows are written with the right user.
  - [ ] Manual check (Completion Notes): open the staff edit dialog (2.1), type something, press Ctrl+L, unlock, and confirm the text is still there; let it idle past a 1-minute test timeout.

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

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
