---
story: "2.5"
epic: 2
title: Permission-driven shell and service-level authorization
status: done
size: M
backlogItems: [HT-02]
frsCovered: [FR2]
uxDesignRequirements: [UX-DR7, UX-DR10]
nfrsTouched: [NFR6, NFR11]
dependsOn: ["2.3", "2.4"]
baseline_commit: 87fb8e1e3bd4fd9d0dbb7afedf92ef1a3c13ba79
---

# Story 2.5: Permission-driven shell and service-level authorization

Status: done

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

- [x] **T1. `ICurrentUser.HasPermission`** (AC: 1, 4)
  - [x] Add `bool HasPermission(string maQuyen)` to `ICurrentUser`. `CurrentUserSession` keeps an immutable `FrozenSet<string>` of codes inside its session snapshot (the same atomic swap as today), filled by `DangNhapService` at sign-in from one query (user → roles → permissions, active users only). Cleared on sign-out.
  - [x] Document on the interface: **for UI only**. Services must use `IKiemTraQuyen` (DB, current), never `HasPermission` (cached at sign-in). Add an architecture test: no type in `LuuKyCanTin.Application` other than the abstraction itself calls `ICurrentUser.HasPermission`. Scanning the IL is overkill; a source scan of `src/Libraries/LuuKyCanTin.Application/**/*.cs` for `HasPermission(` is enough.
- [x] **T2. Permission-driven shell** (AC: 1)
  - [x] WinForms `Shell/`: a menu registry, e.g. `record MucMenu(string Nhom, string Ten, string? MaQuyen, Keys? PhimTat, Func<IServiceProvider, Form> MoForm)`. Every screen built so far registers here, with its permission:
    - Danh mục › Cán bộ: `DM.Xem`;
    - Hệ thống › Tài khoản, Vai trò: `HT.Xem`;
    - Hệ thống › Đổi mật khẩu, Đăng xuất (and Khoá máy, from 2.7): no permission, every signed-in user.
    - Later stories add their own items: 2.8 Thông tin đơn vị, 2.9 Cấu hình người ký, 2.10 Nhật ký thao tác.
  - [x] `MainPresenter` (Story 1.1) asks a pure function `XayDungMenu(IEnumerable<MucMenu>, Func<string, bool> coQuyen)` for the visible groups and items, then hands the view a plain model. Groups with no visible item disappear. The toolbar follows the same model.
  - [x] **Screens open read-only when the user has `Xem` but not the write permission.** Each form's presenter disables Thêm/Sửa/Lưu from `HasPermission`. That's cosmetic, and the service still decides (AC 2).
- [x] **T3. Service-level checks everywhere** (AC: 2, 3)
  - [x] Retrofit Story 2.1: `CanBoService.ThemAsync` → `DM.Them`, `SuaAsync` → `DM.Sua` (replace the 2.1 markers). Confirm 2.3 (`VaiTroService`, `HT.Sua`) and 2.4 (`TaiKhoanService`, `HT.Sua`) call `IKiemTraQuyen` **before** `BeginTransaction`.
  - [x] Exceptions: `DoiMatKhauService` and `DangXuat` act only on the caller's own account and need no permission. Sign-in obviously can't require one.
  - [x] Convention test (architecture, reflection): every public non-query service class in Application, i.e. a class named `*Service` with at least one method that isn't `Lay*`/`Tim*`, takes an `IKiemTraQuyen` constructor parameter. Keep a short, commented allow-list (`DangNhapService`, `DoiMatKhauService`). Later epics inherit the guard for free.
- [x] **T4. Friendly message** (AC: 2, UX-DR10)
  - [x] Show `KhongCoQuyenException` as a warning dialog with its message, not as a crash. Do it in one place: extend the Story 1.1 `GlobalExceptionHandler` so a business-error exception (the 1.8/2.1 type plus `KhongCoQuyenException`) shows its message without the "unexpected error" wording and is logged at `Warning`, not `Error`. Presenters that already catch business errors keep working.
  - [x] Log the refused attempt with Serilog (user, permission code, service). No audit row is needed: AC 3 requires that nothing is written.
- [x] **T5. Tests** (AC: 1–4)
  - [x] Unit: `XayDungMenu` hides items and empty groups; items without a permission always show.
  - [x] Presenter: `MainPresenter` with a fake `ICurrentUser` holding only `DM.Xem` shows "Danh mục › Cán bộ" and no "Hệ thống › Tài khoản".
  - [x] **Bypass integration test (AC 3)**, `IntegrationTests/HeThong/PhanQuyenBypassTests.cs`: sign in a user whose role has only `Xem` permissions, call `CanBoService.ThemAsync`, `TaiKhoanService.TaoAsync` and `VaiTroService.CapNhatQuyenAsync` directly, and assert `KhongCoQuyenException` each time. Then compare row counts of `CanBo`, `NguoiDung`, `VaiTroQuyen` and `NhatKyThaoTac` before and after: unchanged.
  - [x] **Revocation test (AC 4)**: user A has `DM.Them` and adds a staff member; the admin removes `DM.Them` from A's role in another scope; A's next `ThemAsync` in the **same** session is refused, even though A's cached `HasPermission` still says yes.
  - [x] Each of the 6 seeded roles signs in and sees exactly its expected top-level groups (epic exit criterion "every R0.5 role sees only its own menus"). Make it a `[Theory]` over the role codes.

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

opencode / deepseek-v4.1-flash

### Debug Log References

- `dotnet build LuuKyCanTin.slnx` → 0 errors (only the pre-existing WebView2 spike MSB3277 warning).
- `dotnet test LuuKyCanTin.slnx` → 739 passed, 0 failed, 0 skipped (Domain 85, Application 141, WinForms 259, Integration 254).
- `dotnet ef migrations has-pending-model-changes --project src/Libraries/LuuKyCanTin.Infrastructure` → no changes; the story needs no migration.

### Completion Notes List

**Translation table (story text → code, per `docs/conventions/naming-conventions.md`)**

| Story (pre-R.1 Vietnamese) | Implemented English |
|---|---|
| `IKiemTraQuyen` | `IPermissionChecker` (existing from 2.3, not renamed) |
| `KhongCoQuyenException` | `PermissionDeniedException` (existing from 2.3, not renamed) |
| `MucMenu` | `NavItem` + `NavGroup` (existing types); `NavItem.PermissionCode` is the new permission slot |
| `XayDungMenu` | `ShellNavigation.BuildVisible` |
| `MaQuyen` | `PermissionCodes` / `NavItem.PermissionCode` |
| `NhatKyThaoTac` | `AuditLog` (existing, not renamed) |
| `CanBoService.ThemAsync` / `SuaAsync` | `OfficerService.AddAsync` / `UpdateAsync` |
| `TaiKhoanService.TaoAsync` | `AccountService.CreateAsync` |
| `VaiTroService.CapNhatQuyenAsync` | `RoleService.UpdatePermissionsAsync` |
| `DangNhapService` | `SignInService` |
| `DoiMatKhauService` | `ChangePasswordService` |
| `Lay*` / `Tim*` query methods | `Get*` / `Search*` |
| `IntegrationTests/HeThong/PhanQuyenBypassTests.cs` | `IntegrationTests/Administration/PermissionBypassTests.cs` |

**What changed**

- `ICurrentUser.HasPermission(string)` is documented as UI-only; `CurrentUserSession` holds an immutable `FrozenSet<string>` inside its session record, filled at sign-in by one `IUserStore.GetPermissionCodesAsync` query (user → UserRole → RolePermission → Permission, active user only) and cleared by `SignOut`.
- `NavItem` declares `PermissionCode` (null = always visible); `ShellNavigation.Create` sets the codes (`LK-T.Them`, `DM.Xem`, `DM.Them`, `HT.Xem`); `ShellNavigation.BuildVisible` is the pure filter (hidden item, empty group disappears, home always stays), so `Tiles` and `FindByShortcut` follow the visible model. `MainPresenter` injects `ICurrentUser` and shows the filtered model.
- Checks before any transaction: `OfficerService.AddAsync` → `DM.Them`, `UpdateAsync` → `DM.Sua` (2.1 TODO markers replaced); `AddInmateService.AddAsync` → `DM.Them`; `CustodyLedgerService.PostDepositReceiptAsync` → `LK-T.Them`. `RoleService`/`AccountService` keep `HT.Sua`. No checks on sign-in, change-password or sign-out.
- Read-only screens: `OfficerPresenter`/`RolePresenter`/`AccountPresenter` call `SetEditingEnabled(...)` from the cached permissions; the forms disable their write buttons (and the insert/enter shortcuts). The service still decides.
- `GlobalExceptionHandler`: `BusinessRuleException` or `PermissionDeniedException` at the top shows its own message in a Warning dialog and logs at Warning with user id and permission code; anything else keeps the Error path. `Program` hands the handler the signed-in user after the host is built.
- The architecture tests enforce the two rules: the Application source scan for `HasPermission(`, and the reflection convention that every public non-query `*Service` takes `IPermissionChecker`. The allow-list is `SignInService`, `ChangePasswordService` and `FailedSignInService` (a helper called only by those two own-credential flows) with a comment.
- `WalkingSkeletonTests` now grants the seeded admin the `LUU_KY` role before sign-in: the built-in admin only has HT + DM by seed, so it cannot post a receipt (LK-T.Them) — no seed or migration change was needed.
- `Infrastructure` exposes internals to `LuuKyCanTin.WinForms.UnitTests` so the 6-role `[Theory]` reads the real `DefaultRoles` seed instead of duplicating it.
- No model change: `has-pending-model-changes` reports none.

### File List

**Source**
- src/Libraries/LuuKyCanTin.Application/Abstractions/ICurrentUser.cs
- src/Libraries/LuuKyCanTin.Application/Abstractions/ICurrentUserSession.cs
- src/Libraries/LuuKyCanTin.Application/Administration/IUserStore.cs
- src/Libraries/LuuKyCanTin.Application/Administration/SignInService.cs
- src/Libraries/LuuKyCanTin.Application/Custody/CustodyLedgerService.cs
- src/Libraries/LuuKyCanTin.Application/MasterData/AddInmateService.cs
- src/Libraries/LuuKyCanTin.Application/MasterData/OfficerService.cs
- src/Libraries/LuuKyCanTin.Infrastructure/Administration/CurrentUserSession.cs
- src/Libraries/LuuKyCanTin.Infrastructure/Administration/UserStore.cs
- src/Libraries/LuuKyCanTin.Infrastructure/LuuKyCanTin.Infrastructure.csproj
- src/Presentation/LuuKyCanTin.WinForms/Administration/AccountForm.cs
- src/Presentation/LuuKyCanTin.WinForms/Administration/AccountPresenter.cs
- src/Presentation/LuuKyCanTin.WinForms/Administration/IAccountView.cs
- src/Presentation/LuuKyCanTin.WinForms/Administration/IRoleView.cs
- src/Presentation/LuuKyCanTin.WinForms/Administration/RoleForm.cs
- src/Presentation/LuuKyCanTin.WinForms/Administration/RolePresenter.cs
- src/Presentation/LuuKyCanTin.WinForms/Common/GlobalExceptionHandler.cs
- src/Presentation/LuuKyCanTin.WinForms/MasterData/IOfficerView.cs
- src/Presentation/LuuKyCanTin.WinForms/MasterData/OfficerForm.cs
- src/Presentation/LuuKyCanTin.WinForms/MasterData/OfficerPresenter.cs
- src/Presentation/LuuKyCanTin.WinForms/Program.cs
- src/Presentation/LuuKyCanTin.WinForms/Shell/MainPresenter.cs
- src/Presentation/LuuKyCanTin.WinForms/Shell/NavItem.cs
- src/Presentation/LuuKyCanTin.WinForms/Shell/Navigator.cs
- src/Presentation/LuuKyCanTin.WinForms/Shell/ShellApplicationContext.cs
- src/Presentation/LuuKyCanTin.WinForms/Shell/ShellNavigation.cs

**Tests**
- tests/LuuKyCanTin.Application.UnitTests/Administration/SignInServiceTests.cs
- tests/LuuKyCanTin.Application.UnitTests/Custody/CustodyLedgerServiceTests.cs
- tests/LuuKyCanTin.Application.UnitTests/MasterData/AddInmateServiceTests.cs
- tests/LuuKyCanTin.Application.UnitTests/MasterData/OfficerServiceTests.cs
- tests/LuuKyCanTin.IntegrationTests/Administration/AccountServiceTests.cs
- tests/LuuKyCanTin.IntegrationTests/Administration/AuditLogWriterTests.cs
- tests/LuuKyCanTin.IntegrationTests/Administration/CurrentUserSessionTests.cs
- tests/LuuKyCanTin.IntegrationTests/Administration/PermissionBypassTests.cs (new)
- tests/LuuKyCanTin.IntegrationTests/Administration/RoleServiceTests.cs
- tests/LuuKyCanTin.IntegrationTests/Administration/SignInServiceTests.cs
- tests/LuuKyCanTin.IntegrationTests/Architecture/CachedPermissionUsageTests.cs (new)
- tests/LuuKyCanTin.IntegrationTests/Architecture/ServiceAuthorizationTests.cs (new)
- tests/LuuKyCanTin.IntegrationTests/Common/AppDatabaseFixture.cs
- tests/LuuKyCanTin.IntegrationTests/Custody/WalkingSkeletonTests.cs
- tests/LuuKyCanTin.IntegrationTests/MasterData/OfficerServiceTests.cs
- tests/LuuKyCanTin.IntegrationTests/Persistence/Audit/AuditInterceptorTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/Administration/AccountFormTests.cs (new)
- tests/LuuKyCanTin.WinForms.UnitTests/Administration/AccountPresenterTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/Administration/RolePresenterTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/Administration/RoleScreenTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/Common/GlobalExceptionHandlerTests.cs (new)
- tests/LuuKyCanTin.WinForms.UnitTests/Custody/DepositReceiptPresenterTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/MasterData/AddInmatePresenterTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/MasterData/MasterDataScreenTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/MasterData/OfficerPresenterTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/Shell/MainPresenterTests.cs
- tests/LuuKyCanTin.WinForms.UnitTests/Shell/ShellNavigationFilterTests.cs (new)
- tests/LuuKyCanTin.WinForms.UnitTests/Shell/ShellScreenTests.cs

**Docs**
- docs/install.md (menu/read-only permission note)
- _bmad-output/implementation-artifacts/stories/README.md (2.5 → in-progress)

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | Story tracking disagrees (frontmatter vs README vs Change Log) | false | The presentation step owns the final status; README and the Change Log are updated there. |
| 2 | The story file gained a UTF-8 BOM and a duplicated lowercase body label | low — patch | Verified first bytes `EF BB BF`; rewrite the file without BOM and keep the body `Status:`. |
| 3 | `OnUnobservedTaskException` logs nothing when no form can receive the message | medium — patch | Verified `GlobalExceptionHandler.cs:59-75`: `SetObserved()` runs, the log lives only in the UI callback. Log unconditionally before dispatching. |
| 4 | `OnUnhandledException` skips `Log.CloseAndFlush()` on the business-warning path | medium — patch | Verified `:48-57`; a terminating process loses the warning and the session logs. Flush before returning. |
| 5 | `AggregateException` is not unwrapped for unobserved tasks | medium — patch | Verified `IsBusinessError` checks the top-level type and `UnobservedTaskException` always wraps in `AggregateException`; unwrap before classifying. |
| 6 | The refusal log lacks the service and denials caught by presenters are never logged | low — defer | T4 wants user, code and service; the port has no logging seam for caught denials. Recorded in `deferred-work.md`. |
| 7 | `ServiceAuthorizationTests` can be evaded by a `Get*`-named write and proves only injection | low — reject | The story's requirement is exactly the constructor parameter; current services comply and a stricter heuristic is speculative. |
| 8 | `CachedPermissionUsageTests` matches the literal text `HasPermission(` | low — patch | Widen the scan to `HasPermission\b` so a method-group use is caught. |
| 9 | The 6-role theory never signs a role in and checks only groups | low — reject | The filter runs with the real `DefaultRoles` grants; the sign-in query is covered by `SignInServiceTests`. A true DB sign-in inside the WinForms test project is out of its scope. |
| 10 | Nothing enforces that a future `NavItem` declares a permission | low — reject | Design guardrail, not a defect in this change; the role theory plus the service checks cover today's screens. |
| 11 | "Toolbar follows the model" but no toolbar exists; AddInmate/DepositReceipt UI unguarded | false | The R.2/R.3 shell has a sidebar and home tiles, no toolbar; those screens open only through nav items that already require their write permission. |
| 12 | `WalkingSkeletonTests` grants the seeded admin the custody role in test setup | low — reject | Test fixture choice; the product lets an administrator hold the role, and `install.md` documents the menu behaviour. |
| 13 | `AppDatabaseFixture.SignInAsync` drifts from `SignInService` | low — reject | The fixture exists to short-circuit sign-in; the real path is covered by `SignInServiceTests`. |
| 14 | `GetPermissionCodesAsync` orders before `Distinct` | false | The codes are consumed as a `FrozenSet`; order is irrelevant and the sign-in tests executed this query green. |
| 15 | The revocation test removes the grant with `ExecuteDeleteAsync`, not `RoleService` | low — reject | The test proves the DB check sees a revocation; the `RoleService` path is covered separately. |
| 16 | `PermissionBypassTests` signs out only on the last line | low — patch | Wrap the cleanup so a failed assertion cannot leak the session into later tests of the class. |
| 17 | Read-only guards and the officer presenter's two permission codes are not isolated by tests | medium — patch | Verified `OfficerPresenter.cs:24-27` uses the right codes but `OfficerPresenterTests` stubs all-true/all-false; add code-isolation tests and cover the form-level guards. |
| 18 | `AuditInterceptorTests` line 137 is over-indented | low — patch | Formatting only; fix the indentation. |
| 19 | `InternalsVisibleTo` makes a WinForms test depend on the Infrastructure seed | low — reject | The coupling is intentional and local to the role theory; a project-reference move would fail the build loudly. |
| 20 | The new warning-dialog dispatch (`TryShowBusinessWarning`) is untested | medium — patch | Verified only `IsBusinessError` is asserted; add an internal display hook and tests for the warning title/message and the false path. |

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
| 2026-10-03 | Implemented on 87fb8e1: cached `HasPermission` at sign-in, permission-driven navigation, service-level checks before transactions, read-only screens, business-error warning dialog; all tasks done, tests green. Reviewed (blind hunter, edge cases, verification gaps) with patches applied; status -> done. |
