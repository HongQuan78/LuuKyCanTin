---
story: "2.5"
epic: 2
title: Permission-driven shell and service-level authorization
status: ready-for-dev
size: M
backlogItems: [HT-02]
frsCovered: [FR2]
uxDesignRequirements: [UX-DR7, UX-DR10]
nfrsTouched: [NFR6, NFR11]
dependsOn: ["2.3", "2.4"]
---

# Story 2.5: Permission-driven shell and service-level authorization

Status: ready-for-dev

## Story

As a unit leader,
I want each user to see only the functions they may use, and every write to be authorized again by the service,
So that hiding a button is never the only protection.

## Acceptance Criteria

1. **Given** `ICurrentUser` exposing `HasPermission(string code)` loaded from the user's roles at sign-in
   **When** the main shell builds its menu and toolbar
   **Then** each item declares the permission it needs; items the user lacks are hidden; a module with no visible items is hidden

2. **Given** an Application service method that writes data
   **When** the current user lacks the required permission
   **Then** the service throws a domain `KhongCoQuyenException` (no permission) before opening a transaction, and the UI shows "Bạn không có quyền thực hiện thao tác này" (you are not allowed to do this)

3. **Given** an integration test that calls a service directly, bypassing the hidden menu
   **When** the caller has only `Xem` rights
   **Then** the write is rejected and no row (data or audit) is written

4. **Given** an administrator who changes a user's roles
   **When** that user next opens a function
   **Then** permissions are re-read from the DB at sign-in, and services check the current DB permission on writes (a revoked permission stops working immediately)

## Tasks / Subtasks

- [ ] **T1. `ICurrentUser.HasPermission`** (AC: 1, 4)
  - [ ] Add `bool HasPermission(string maQuyen)` to `ICurrentUser`. `CurrentUserSession` keeps an immutable `FrozenSet<string>` of codes inside its session snapshot (the same atomic swap as today), filled by `DangNhapService` at sign-in from one query (user → roles → permissions, active users only). Cleared on sign-out.
  - [ ] Document on the interface: **for UI only**. Services must use `IKiemTraQuyen` (DB, current), never `HasPermission` (cached at sign-in). Add an architecture test: no type in `LuuKyCanTin.Application` other than the abstraction itself calls `ICurrentUser.HasPermission`. Scanning the IL is overkill; a source scan of `src/Libraries/LuuKyCanTin.Application/**/*.cs` for `HasPermission(` is enough.
- [ ] **T2. Permission-driven shell** (AC: 1)
  - [ ] WinForms `Shell/`: a menu registry, e.g. `record MucMenu(string Nhom, string Ten, string? MaQuyen, Keys? PhimTat, Func<IServiceProvider, Form> MoForm)`. Every screen built so far registers here, with its permission:
    - Danh mục › Cán bộ: `DM.Xem`;
    - Hệ thống › Tài khoản, Vai trò: `HT.Xem`;
    - Hệ thống › Đổi mật khẩu, Đăng xuất (and Khoá máy, from 2.7): no permission, every signed-in user.
    - Later stories add their own items: 2.8 Thông tin đơn vị, 2.9 Cấu hình người ký, 2.10 Nhật ký thao tác.
  - [ ] `MainPresenter` (Story 1.1) asks a pure function `XayDungMenu(IEnumerable<MucMenu>, Func<string, bool> coQuyen)` for the visible groups and items, then hands the view a plain model. Groups with no visible item disappear. The toolbar follows the same model.
  - [ ] **Screens open read-only when the user has `Xem` but not the write permission.** Each form's presenter disables Thêm/Sửa/Lưu from `HasPermission`. That's cosmetic, and the service still decides (AC 2).
- [ ] **T3. Service-level checks everywhere** (AC: 2, 3)
  - [ ] Retrofit Story 2.1: `CanBoService.ThemAsync` → `DM.Them`, `SuaAsync` → `DM.Sua` (replace the 2.1 markers). Confirm 2.3 (`VaiTroService`, `HT.Sua`) and 2.4 (`TaiKhoanService`, `HT.Sua`) call `IKiemTraQuyen` **before** `BeginTransaction`.
  - [ ] Exceptions: `DoiMatKhauService` and `DangXuat` act only on the caller's own account and need no permission. Sign-in obviously can't require one.
  - [ ] Convention test (architecture, reflection): every public non-query service class in Application, i.e. a class named `*Service` with at least one method that isn't `Lay*`/`Tim*`, takes an `IKiemTraQuyen` constructor parameter. Keep a short, commented allow-list (`DangNhapService`, `DoiMatKhauService`). Later epics inherit the guard for free.
- [ ] **T4. Friendly message** (AC: 2, UX-DR10)
  - [ ] Show `KhongCoQuyenException` as a warning dialog with its message, not as a crash. Do it in one place: extend the Story 1.1 `GlobalExceptionHandler` so a business-error exception (the 1.8/2.1 type plus `KhongCoQuyenException`) shows its message without the "unexpected error" wording and is logged at `Warning`, not `Error`. Presenters that already catch business errors keep working.
  - [ ] Log the refused attempt with Serilog (user, permission code, service). No audit row is needed: AC 3 requires that nothing is written.
- [ ] **T5. Tests** (AC: 1–4)
  - [ ] Unit: `XayDungMenu` hides items and empty groups; items without a permission always show.
  - [ ] Presenter: `MainPresenter` with a fake `ICurrentUser` holding only `DM.Xem` shows "Danh mục › Cán bộ" and no "Hệ thống › Tài khoản".
  - [ ] **Bypass integration test (AC 3)**, `IntegrationTests/HeThong/PhanQuyenBypassTests.cs`: sign in a user whose role has only `Xem` permissions, call `CanBoService.ThemAsync`, `TaiKhoanService.TaoAsync` and `VaiTroService.CapNhatQuyenAsync` directly, and assert `KhongCoQuyenException` each time. Then compare row counts of `CanBo`, `NguoiDung`, `VaiTroQuyen` and `NhatKyThaoTac` before and after: unchanged.
  - [ ] **Revocation test (AC 4)**: user A has `DM.Them` and adds a staff member; the admin removes `DM.Them` from A's role in another scope; A's next `ThemAsync` in the **same** session is refused, even though A's cached `HasPermission` still says yes.
  - [ ] Each of the 6 seeded roles signs in and sees exactly its expected top-level groups (epic exit criterion "every R0.5 role sees only its own menus"). Make it a `[Theory]` over the role codes.

## Dev Notes

### Current codebase state (after 2.4)

- `IKiemTraQuyen` + `KhongCoQuyenException` (2.3), used by `VaiTroService` and `TaiKhoanService`. `ICurrentUser` has `NguoiDungId`, `TenDangNhap`, `DaDangNhap`, `CanBoId`, `HoTen` (2.4). `MainForm`/`MainPresenter`/`IMainView` exist from 1.1, and the menu is hard-coded so far.

### Design notes

- **Two checks, two purposes.** `HasPermission` (cached) drives what's *visible*. `IKiemTraQuyen` (DB, every write) decides what's *allowed*. The cache can be stale for one session; the DB check never is. That's the full meaning of AC 4.
- **Before the transaction** (AC 2): the check is a cheap read. Doing it first means a refused call opens no transaction, allocates no document number and writes no audit row.
- **Read permissions in services?** The AC covers writes. Read-only queries rely on the menu, plus the fact that every screen opens through it. Add an `Xem` check to a query only when the data is sensitive (none in this epic).

### Gotchas

- The shell is rebuilt for each sign-in (2.2's `ApplicationContext` loop creates a fresh `MainForm`), so no stale menu survives a user switch.
- Don't put permission checks in Presenters as the only guard. A Presenter test with a fake service can't prove authorization; only the integration bypass test does.

### Out of scope

- Unit info (2.8), signers (2.9) and audit-log (2.10) menu items, which those stories add. Keyboard shortcuts for business screens (Epic 13).

### References

- Epic 2 › Story 2.5; `epics.md` › FR2, UX-DR7, UX-DR10, NFR6
- Tech Stack PDF: "Menu và nút ẩn theo quyền; service kiểm tra lại quyền trước khi ghi (không tin giao diện)"
- `CLAUDE.md`: "Services re-check permissions before writing, because hidden UI elements don't count as authorization"

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
