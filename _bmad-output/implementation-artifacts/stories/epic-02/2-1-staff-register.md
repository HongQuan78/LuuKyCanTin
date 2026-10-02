---
story: "2.1"
epic: 2
title: Staff register
status: review
size: S
backlogItems: [DM-04]
frsCovered: [FR17]
nfrsTouched: [NFR4, NFR5, NFR10, NFR11]
dependsOn: ["1.2", "1.3"]
---

# Story 2.1: Staff register

Status: review

## Story

As an administrator,
I want to maintain the list of staff (cán bộ) with their position and whether they are a warden,
So that accounts, signatories, wardens of detainees and document creators all refer to real people.

## Acceptance Criteria

1. **Given** the `CanBo` table (`Id`, `MaCanBo varchar(20)` UQ, `HoTen nvarchar(100)` NOT NULL, `ChucVu nvarchar(100)`, `LaQuanGiao bit`, `DangCongTac bit` default 1, plus audit columns)
   **When** the administrator adds a staff member with code, name, position and the warden flag
   **Then** the record is saved and appears in the staff list, searchable by code or name without diacritics

2. **Given** an existing staff code
   **When** another staff member is saved with the same `MaCanBo`
   **Then** the save is rejected with "Mã cán bộ đã tồn tại" (code already exists)

3. **Given** a staff member who has left
   **When** the administrator sets `DangCongTac = 0`
   **Then** the person is hidden from every selection list (wardens, signers, accounts) but stays on historical documents and in reports
   **And** staff are never hard-deleted

4. **Given** the business roles from the spec (warden, canteen, commander, accountant, leadership)
   **When** a staff member is recorded
   **Then** the role shows in `ChucVu` and `LaQuanGiao` marks wardens; system permissions come from account roles (Story 2.3), not from this record

5. **Given** two users edit the same staff record
   **When** the second one saves
   **Then** the `RowVer` conflict is detected and the user sees "Dữ liệu đã bị người khác thay đổi, vui lòng tải lại" (data changed by someone else, please reload)

## Tasks / Subtasks

- [x] **T1. Domain entity** (AC: 1, 3, 4)
  - [x] Domain `DanhMuc/CanBo.cs : AuditableEntity, IAuditable` with `MaCanBo`, `HoTen`, `ChucVu?`, `LaQuanGiao`, `DangCongTac`. Master data is audited too: the epic rule is "every account-administration and configuration change is written to `NhatKyThaoTac`".
  - [x] No delete method. Deactivating is `NgungCongTac()` / `CongTacLai()` (or a plain setter). The Story 1.3 interceptor already throws on a `Deleted` `IAuditable` entry, so a hard delete can't slip through EF.
  - [x] Normalize input in the entity or the service: trim both fields, store `MaCanBo` upper-case.
- [x] **T2. Persistence** (AC: 1, 2, 5)
  - [x] `Persistence/Configurations/DanhMuc/CanBoConfiguration.cs`: `MaCanBo varchar(20)` NOT NULL + unique index; `HoTen nvarchar(100)` NOT NULL + index `IX_CanBo_HoTen`; `ChucVu nvarchar(100)` NULL; `LaQuanGiao bit` NOT NULL default 0; `DangCongTac bit` NOT NULL default 1. Audit columns and `RowVer` come from the Story 1.2 base configuration.
  - [x] Migration `AddCanBo`. Run `--migrate` locally afterwards: the startup schema-version check refuses an out-of-date DB.
- [x] **T3. Application service** (AC: 1–5)
  - [x] Application `DanhMuc/CanBoService.cs`: `ThemAsync`, `SuaAsync` (includes the `DangCongTac` flag), `TimAsync(string? tuKhoa, bool baoGomNgungCongTac)`. DTOs `CanBoDto` and `LuuCanBoRequest` (with `RowVer` for edits).
  - [x] FluentValidation `LuuCanBoRequestValidator`: `MaCanBo` required, ≤ 20, letters/digits/`-`/`_` only (it's `varchar`, so no diacritics); `HoTen` required, ≤ 100; `ChucVu` ≤ 100.
  - [x] Duplicate code: check with `AnyAsync` before saving, and keep the unique index as the backstop. Both paths give the same business error "Mã cán bộ đã tồn tại".
  - [x] Edit: load the entity, set `OriginalValues[RowVer]` to the client's `RowVer`, change it, save. A concurrency failure becomes the business error "Dữ liệu đã bị người khác thay đổi, vui lòng tải lại".
  - [x] A small read query for selection lists, `LayCanBoDangCongTacAsync(bool chiQuanGiao)`, returns active staff only. Stories 2.4, 2.9 and 3.x (warden picker) reuse it, so "hidden from every selection list" lives in one place.
  - [x] **Permission check:** the `IKiemTraQuyen` port arrives in Story 2.3, after this story. Leave a `// Story 2.5: require DM.Them / DM.Sua` marker on each write method. Story 2.5 wires it in and adds the bypass test.
- [x] **T4. Business-error plumbing** (AC: 2, 5), first master-data screen, so shared from here on
  - [x] Reuse the business-error type from Story 1.8 if it exists (e.g. `LoiNghiepVuException` carrying a Vietnamese message). Otherwise add it in Application `Common/`.
  - [x] Infrastructure translates EF failures in one place (the `IAppDbContext.SaveChangesAsync` implementation or a small helper): `DbUpdateConcurrencyException` → `XungDotDuLieuException` (message above); `SqlException` 2601/2627 → a unique-violation error that the service maps to its own message. Application never sees EF or SqlClient exception types.
  - [x] Stories 2.4, 2.8 and 2.9 reuse both.
- [x] **T5. WinForms screen** (AC: 1, 3, 5)
  - [x] `DanhMuc/`: `ICanBoView` + `CanBoPresenter` + `CanBoForm`. A grid (Mã, Họ tên, Chức vụ, Quản giáo, Đang công tác) with a search box, an "Hiện cả người đã nghỉ" (show inactive) checkbox, and Thêm/Sửa opening an edit dialog.
  - [x] Search runs while typing with a ~300 ms debounce. Use the query pattern recorded by the Story 1.7 spike if it's done; otherwise `LIKE '%' + @tuKhoa + '%'` on `MaCanBo` and `HoTen`, which `Vietnamese_CI_AI` makes diacritic- and case-insensitive. The staff list stays small (tens to a few hundred rows).
  - [x] Every load and save creates a new DI scope. The form never holds a DbContext.
  - [x] Add a "Danh mục › Cán bộ" menu item. Story 2.5 attaches its permission.
- [x] **T6. Tests** (AC: 1–5)
  - [x] Application unit tests (NSubstitute): the validator rules; a duplicate code returns the business error and saves nothing.
  - [x] Integration (`[SqlServerFact]`): add, then search "nguyen van" finds "Nguyễn Văn …"; a duplicate `MaCanBo` is rejected; set `DangCongTac = 0` → missing from `LayCanBoDangCongTacAsync` but still found by `TimAsync(..., baoGomNgungCongTac: true)`; two contexts edit the same row → the second gets `XungDotDuLieuException`; each add/edit writes one `Them`/`Sua` row in `NhatKyThaoTac`.
  - [x] Presenter test: a save error is shown on the view and the dialog stays open.

## Dev Notes

### Current codebase state

- Done (1.1–1.4): host, `AppDbContext`, `AuditableEntity` + base configuration, `HasEnumCheck`, the audit interceptor (`IAuditable`, `[KhongGhiNhatKy]`, `IGhiNhatKy`), `IClock`/`FakeClock`, `CurrentUserSession`, and the `[SqlServerFact]` + `TestDatabase` integration setup.
- Story 1.8 (ready-for-dev) adds `NguoiDung`, `ThongTinDonVi`, `IAppDbContext` and the login. This story needs only 1.2 and 1.3, so it can start before 1.8 is finished. If `IAppDbContext` doesn't exist yet, follow the Story 1.8 T0 decision (DbSet exposure vs narrow ports) and create the minimum it needs.

### Design notes

- **Business role vs system role (alignment A15).** `ChucVu` is free text such as "Cán bộ quản giáo" or "Chỉ huy phụ trách". `LaQuanGiao` is the only structured flag. Access rights never come from this table.
- **Signer roles that aren't staff** (sender, detainee, buyer, deliverer) never need a `CanBo` row (Story 2.9).
- `DangCongTac = 0` hides a person from *selection*, never from *display*. Historical documents keep their FK and show the name.
- A unique index on `MaCanBo` is enough. Staff codes are reused rarely, and an inactive person keeps their code.

### Out of scope

- Linking staff to accounts (2.4). The warden assignment on a detainee (Epic 3, `DoiTuong.CanBoQuanGiaoId`). Excel import of staff, which isn't in the spec.

### References

- Epic 2 › Story 2.1; `epics.md` › FR17, Backlog Alignment A7, A15
- DB design PDF: `CanBo` (p.6)
- Feature list PDF: DM-04 (p.3); user roles table (p.2)
- `CLAUDE.md`: rowversion for optimistic concurrency, forms never hold a DbContext

## Dev Agent Record

### Agent Model Used

Claude Opus 5.5 (claude-opus-5-5)

### Debug Log References

- `Vietnamese_CI_AI` probe, run against SQL Server 2022: tone marks are ignored (`Ánh` = `anh`, `Hồ` = `ho`), but ă, â, ê, ô, ơ, ư and đ are separate letters (`Văn` ≠ `van`, `Đức` ≠ `duc`). `Latin1_General_100_CI_AI` folds all of them.

### Completion Notes List

- **Story 1.8 T0, settled as "yes".** Application now references `Microsoft.EntityFrameworkCore` (core only). `IAppDbContext` exposes `DbSet<CanBo>`, `Entry` and `SaveChangesAsync`. `ProjectReferenceRules.ApplicationAllowedPackages` admits the core package, and a negative test keeps every provider package forbidden (SqlServer, Sqlite).
- **Search collation (T5 dev note corrected).** `LIKE` under the database collation `Vietnamese_CI_AI` can't find "Nguyễn Văn" from "nguyen van" (see the debug log). `CanBo.HoTen` gets the column collation `AppDbContext.CollationTimKiem` = `Latin1_General_100_CI_AI`. Trade-off: names sort in Latin order (Đ with D), not strict Vietnamese alphabetical order. The 1.7 spike should confirm this, or replace it, before `DoiTuong.HoTen` copies it.
- **Business errors.** These live in Application `Common/`: `LoiNghiepVuException`, `XungDotDuLieuException` (a subclass with the fixed message) and `TrungGiaTriDuyNhatException` (unique index; the service maps it to its own message). `AppDbContext` translates EF/SqlClient exceptions only in its explicit `IAppDbContext.SaveChangesAsync`, so Infrastructure code that calls the context directly keeps EF's exception types.
- **Validation.** `CanBoService` runs `LuuCanBoRequestValidator` on trimmed values. A failure becomes one `LoiNghiepVuException` holding every message, one per line, so the presenter has a single error path.
- **Permission marker.** Each write method has `// TODO: require permission DM.Them/DM.Sua once IKiemTraQuyen exists.` It carries no story number, because story references don't belong in source comments.
- **`DangCongTac` default.** The column's `DEFAULT 1` needs `HasSentinel(true)`. Without it EF would leave an explicit `false` out of the INSERT, and a person added as already gone would be stored as active. A regression test covers this.
- **WinForms.** The list form has a 300 ms debounce timer. The presenter ignores out-of-order search results. The edit dialog stays open on a business error. `IDieuHuong`/`DieuHuong` composes the screen, so `MainPresenter` never creates forms. Every load and save creates its own DI scope.
- **Not done here:** `--migrate` on a dev DB and a manual run of the screen, because this workstation has no `src/Presentation/LuuKyCanTin.WinForms/.env`.
- Tests: Domain 50, Application 42, WinForms 37, Integration 163 (SQL Server 2022 in Docker). All pass.

### File List

- `Directory.Packages.props`: FluentValidation 12.1.1, EF Core 10.0.12, EF InMemory 10.0.12 (tests)
- `src/Libraries/LuuKyCanTin.Domain/DanhMuc/CanBo.cs`
- `src/Libraries/LuuKyCanTin.Application/LuuKyCanTin.Application.csproj`, `DependencyInjection.cs`
- `src/Libraries/LuuKyCanTin.Application/Abstractions/IAppDbContext.cs`
- `src/Libraries/LuuKyCanTin.Application/Common/{LoiNghiepVuException,XungDotDuLieuException,TrungGiaTriDuyNhatException}.cs`
- `src/Libraries/LuuKyCanTin.Application/DanhMuc/{CanBoDto,LuuCanBoRequest,LuuCanBoRequestValidator,ICanBoService,CanBoService}.cs`
- `src/Libraries/LuuKyCanTin.Infrastructure/DependencyInjection.cs`, `Persistence/AppDbContext.cs`
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/DanhMuc/CanBoConfiguration.cs`
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Migrations/20261002101002_AddCanBo{,.Designer}.cs`, `AppDbContextModelSnapshot.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Program.cs`
- `src/Presentation/LuuKyCanTin.WinForms/DanhMuc/{ICanBoView,ICanBoEditView,CanBoPresenter,CanBoEditPresenter,CanBoForm,CanBoForm.Designer,CanBoEditForm,CanBoEditForm.Designer}.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/{IMainView,MainForm,MainForm.Designer,MainPresenter,IDieuHuong,DieuHuong}.cs`
- `tests/LuuKyCanTin.Domain.UnitTests/DanhMuc/CanBoTests.cs`
- `tests/LuuKyCanTin.Application.UnitTests/DanhMuc/{CanBoServiceTests,LuuCanBoRequestValidatorTests}.cs`, `TestUtilities/InMemoryAppDbContext.cs`, csproj
- `tests/LuuKyCanTin.IntegrationTests/Common/AppDatabaseFixture.cs`, `DanhMuc/CanBoServiceTests.cs`, `Persistence/CanBoModelTests.cs`, `Persistence/InfrastructureRegistrationTests.cs`, `Architecture/ProjectReferenceRules{,Tests}.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/DanhMuc/{CanBoPresenterTests,CanBoEditPresenterTests}.cs`, `Shell/MainPresenterTests.cs`

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
| 2026-10-02 | Implemented; status set to review. HoTen uses Latin1_General_100_CI_AI for diacritic-free search (Vietnamese_CI_AI keeps ă/ơ/ư/đ distinct) |
