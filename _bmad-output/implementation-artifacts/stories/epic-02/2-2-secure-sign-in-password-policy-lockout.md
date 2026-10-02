---
story: "2.2"
epic: 2
title: Secure sign-in, password policy and lockout
status: ready-for-dev
size: M
backlogItems: [HT-01]
frsCovered: [FR1]
nfrsTouched: [NFR5, NFR6, NFR11]
dependsOn: ["1.8", "2.1"]
---

# Story 2.2: Secure sign-in, password policy and lockout

Status: ready-for-dev

## Story

As a user,
I want to sign in with my own account, change my password and sign out, with the account protected against guessing,
So that nobody can act under my name.

## Acceptance Criteria

1. **Given** the `NguoiDung` table extended with `SoLanSai tinyint`, `KhoaDen datetime2(0) NULL`, `DangHoatDong bit` and `PhaiDoiMatKhau bit`
   **When** a user signs in with the correct password
   **Then** `SoLanSai` resets to 0, `ICurrentUser` is populated, and a `DangNhap` audit row is written with user, workstation and `IClock.Now`

2. **Given** a wrong password
   **When** the user fails for the 5th consecutive time
   **Then** the account is locked (`KhoaDen` set), the message says the account is locked, and an audit row is written
   **And** the error message is the same for an unknown user and a wrong password (no user enumeration)

3. **Given** a locked or inactive account
   **When** the user tries to sign in, even with the correct password
   **Then** sign-in is refused until an administrator unlocks the account (Story 2.4) or the lock period configured in `appsettings.json` expires

4. **Given** an account with `PhaiDoiMatKhau = 1` (the seeded `admin` and any newly created or reset account)
   **When** the user signs in
   **Then** they must set a new password before reaching the main shell

5. **Given** a new password
   **When** it is validated
   **Then** it must have at least 8 characters, including an upper-case letter, a lower-case letter and a digit, and must differ from the current one; the password is stored only as a salted PBKDF2 hash (`Rfc2898DeriveBytes.Pbkdf2`, SHA-256, ≥ 100,000 iterations, per-user random salt)

6. **Given** a signed-in user
   **When** they choose Change password (current + new) or Sign out
   **Then** the change requires the current password; sign-out returns to the login form and clears `ICurrentUser`

## Tasks / Subtasks

- [ ] **T1. Domain rules** (AC: 2, 3, 5)
  - [ ] On `NguoiDung` (Domain `HeThong/`, from Story 1.8), add `PhaiDoiMatKhau` and behaviour methods, all taking `now` from the caller (never `DateTime.Now`):
    - `bool DangBiKhoa(DateTime now)` → `KhoaDen is not null && KhoaDen > now`.
    - `bool GhiNhanDangNhapSai(DateTime now, TimeSpan? thoiGianKhoa)` → `SoLanSai++`; at `SoLanSai >= 5`, set `KhoaDen = now + thoiGianKhoa`, or `DateTime.MaxValue`-like far future (`9999-12-31`) when the period is `null`, meaning admin unlock only. Returns whether it just locked.
    - `GhiNhanDangNhapDung()` → `SoLanSai = 0`, `KhoaDen = null`.
    - `DoiMatKhau(string hashMoi)` → sets the hash and clears `PhaiDoiMatKhau`.
  - [ ] Keep the threshold as a Domain constant `SoLanSaiToiDa = 5`. The spec fixes it, so it isn't a setting.
  - [ ] Domain `HeThong/ChinhSachMatKhau.cs`: `IReadOnlyList<string> KiemTra(string matKhauMoi)` returns the Vietnamese violations (≥ 8 chars, an upper-case, a lower-case and a digit). Use `char.IsUpper`/`IsLower`, so "Đ" and "ă" count. A pure function, unit-tested.
  - [ ] Unit tests: the 4th failure doesn't lock and the 5th does; an expired lock lets sign-in through; a `null` period locks indefinitely; the policy table (7 chars, no digit, no upper-case, all valid, Vietnamese letters).
- [ ] **T2. Persistence** (AC: 1, 4)
  - [ ] Migration `AddDangNhapBaoMat`: add `PhaiDoiMatKhau bit NOT NULL DEFAULT 0` (the other three columns exist from 1.8; confirm their defaults: `SoLanSai` 0, `DangHoatDong` 1). In the same migration, `UPDATE NguoiDung SET PhaiDoiMatKhau = 1 WHERE TenDangNhap = 'admin'`, so the seeded admin is forced to change the password. Nothing is deployed yet, so this is safe.
  - [ ] Confirm that `MatKhauHash` carries `[KhongGhiNhatKy]` (Story 1.3/1.8). Add an integration test that changes a password and asserts the hash string is absent from every `NhatKyThaoTac.DuLieuCu/DuLieuMoi` and from the Serilog output of the test.
- [ ] **T3. Sign-in service** (AC: 1–4)
  - [ ] Extend Story 1.8's `DangNhapService.DangNhapAsync(tenDangNhap, matKhau)`. It returns a result, not an exception: `ThanhCong`, `PhaiDoiMatKhau`, `SaiThongTin` ("Tên đăng nhập hoặc mật khẩu không đúng"), `TaiKhoanBiKhoa` ("Tài khoản đã bị khoá, liên hệ quản trị viên"), `TaiKhoanNgungHoatDong`.
  - [ ] Flow:
    1. Load by `TenDangNhap`.
    2. **Unknown user**: still run one `IMatKhauHasher.Verify` against a fixed dummy hash, so response time doesn't reveal whether the account exists. Return `SaiThongTin`.
    3. Inactive or `DangBiKhoa(clock.Now)`: refuse **without** verifying the password and without changing `SoLanSai`.
    4. Wrong password: `GhiNhanDangNhapSai`, save, write the event row. Return `TaiKhoanBiKhoa` if it just locked, else `SaiThongTin`.
    5. Right password: `GhiNhanDangNhapDung`, save, set `CurrentUserSession`, write `DangNhap`.
  - [ ] **Inactive message.** The AC lets locked and inactive accounts be refused. Show inactive accounts the same text as locked ones, so a deactivated person learns nothing new. Only an existing account can produce the locked text. That's an accepted, small enumeration leak on an internal LAN app; record it in the Completion Notes.
  - [ ] Lock period from config: `"DangNhap": { "ThoiGianKhoaPhut": 15 }` in `appsettings.json`, bound to an options class. `0` means only an administrator can unlock. Workstations share the DB, so every machine must use the same value. Say so in the install notes.
  - [ ] **Audit events** (`HanhDong` has only the 6 DB-design values): write every sign-in event as `HanhDong.DangNhap`, `TenBang = "NguoiDung"`, `BanGhiId` = the user id (null for an unknown name). Pass the outcome as the payload, e.g. `{ "SuKien": "DangNhap" | "DangNhapSai" | "KhoaTaiKhoan" | "DangXuat" | "DoiMatKhau", "TenDangNhap": "..." }`. Never include the typed password. Story 2.7 uses the same pattern for lock/unlock, and Story 2.10 shows the payload.
  - [ ] `NguoiDung` is `IAuditable` (account administration must be audited, Story 2.4). The `SoLanSai`/`KhoaDen` changes therefore also produce an automatic `Sua` row next to the explicit event. That's fine: the `Sua` row is the field-level trail and the `DangNhap` row says why. Don't try to suppress either.
  - [ ] Concurrent failures from two workstations on one account: save with `RowVer`. On a concurrency conflict, reload and re-apply the failure once. Never lose a failed attempt.
- [ ] **T4. Change password and sign out** (AC: 4, 5, 6)
  - [ ] `DoiMatKhauService.DoiMatKhauAsync(matKhauHienTai, matKhauMoi, xacNhan)` for the **current** user only: verify the current password (a wrong one counts toward lockout, as in sign-in), run `ChinhSachMatKhau`, require `matKhauMoi != matKhauHienTai` (compare the plain inputs; with salted hashes there's nothing else to compare), require new == confirmation, then hash with `IMatKhauHasher` (Story 1.8, 600,000 iterations ≥ the AC's 100,000) and `DoiMatKhau(hash)`. Write the `DoiMatKhau` event.
  - [ ] `DangXuatAsync()`: write the `DangXuat` event, then `CurrentUserSession.DangXuat()`.
- [ ] **T5. WinForms flow** (AC: 4, 6)
  - [ ] `Shell/LoginForm` (from 1.8) shows each result's message. `TaiKhoanBiKhoa` clears the password box.
  - [ ] `Shell/DoiMatKhauForm` (+ `IDoiMatKhauView`, presenter), used in two modes: **forced** (after a `PhaiDoiMatKhau` sign-in, before the shell; Cancel signs out and returns to login) and **voluntary** ("Hệ thống › Đổi mật khẩu" in the shell). Show the policy rules as a hint under the box.
  - [ ] **Sign-out loop.** `Program` must be able to go login → shell → sign-out → login without restarting. Use an `ApplicationContext` that swaps its main form: closing the shell through "Đăng xuất" shows `LoginForm` again; closing the login form exits the app. Each signed-in session opens a fresh `MainForm`, so nothing from the previous user stays on screen.
  - [ ] Presenter tests: forced mode blocks the shell until success; Cancel in forced mode signs out; a policy violation shows the messages.
- [ ] **T6. Tests** (AC: 1–6)
  - [ ] Application unit tests (NSubstitute `IMatKhauHasher`, `FakeClock`): every branch of the sign-in flow; the unknown user still calls `Verify` once; a locked account isn't verified at all.
  - [ ] Integration: 5 wrong attempts lock the account and write the events; the correct password while locked is refused; `FakeClock.Advance(16 min)` with a 15-minute period lets it in and resets `SoLanSai`; the seeded admin gets `PhaiDoiMatKhau` and, after the change, signs in normally; the hash never appears in audit JSON.

## Dev Notes

### Current codebase state (after Story 1.8)

- `NguoiDung` (`TenDangNhap`, `MatKhauHash` [KhongGhiNhatKy], `DangHoatDong`, `SoLanSai`, `KhoaDen`), the seeded `admin`, `IMatKhauHasher`/`Pbkdf2MatKhauHasher` (`PBKDF2-SHA256$<iter>$<salt>$<hash>`), a thin `DangNhapService`, `LoginForm` shown before `MainForm`, and `CurrentUserSession`. **Read Story 1.8's Completion Notes first.** If 1.8 made `NguoiDung` non-`IAuditable`, make it `IAuditable` here.

### Design notes

- **No user enumeration (AC 2).** Unknown user and wrong password share one message and roughly one response time (the dummy verify).
- **Lock check before the password check.** A locked account doesn't verify the password. Otherwise an attacker could keep testing passwords against a locked account and learn from the message.
- **Forced change happens after authentication.** The session is signed in, but the shell doesn't open until the password is changed. The temporary password is the "current" password on the change form.
- **No password history, no expiry.** The spec asks only for strength, a first-login change and "differs from the current one" (KISS).

### Gotchas

- `KhoaDen` is `datetime2(0)` local time, like `IClock.Now` (Story 1.3). Don't mix in UTC.
- PBKDF2 at 600k iterations takes ~0.3–0.5 s, so run sign-in off the UI thread (`async` presenter, disable the button) or the form freezes.
- Sign-in runs before anyone is signed in, so the interceptor writes `NguoiSuaId = 0` (Story 1.3 note) for the `SoLanSai` update. Pass the user id explicitly for the explicit event rows.

### Out of scope

- Admin unlock and password reset (2.4). Roles and the permission-driven menu (2.3, 2.5). Session lock (2.7).

### References

- Epic 2 › Story 2.2; `epics.md` › FR1, NFR6
- DB design PDF: `NguoiDung` (p.4); seed data "1 tài khoản admin, buộc đổi mật khẩu lần đầu" (p.15)
- Tech Stack PDF: authentication row (PBKDF2 with salt, lockout, forced first change)
- Story 1.8 T3/T4 (hasher, thin login)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
