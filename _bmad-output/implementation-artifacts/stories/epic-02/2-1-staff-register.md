---
story: "2.1"
epic: 2
title: Staff register
status: ready-for-dev
size: S
backlogItems: [DM-04]
frsCovered: [FR17]
nfrsTouched: [NFR4, NFR5, NFR10, NFR11]
dependsOn: ["1.2", "1.3"]
---

# Story 2.1: Staff register

Status: ready-for-dev

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

- [ ] **T1. Domain entity** (AC: 1, 3, 4)
  - [ ] Domain `DanhMuc/CanBo.cs : AuditableEntity, IAuditable` with `MaCanBo`, `HoTen`, `ChucVu?`, `LaQuanGiao`, `DangCongTac`. Master data is audited too: the epic rule is "every account-administration and configuration change is written to `NhatKyThaoTac`".
  - [ ] No delete method. Deactivating is `NgungCongTac()` / `CongTacLai()` (or a plain setter). The Story 1.3 interceptor already throws on a `Deleted` `IAuditable` entry, so a hard delete can't slip through EF.
  - [ ] Normalize input in the entity or the service: trim both fields, store `MaCanBo` upper-case.
- [ ] **T2. Persistence** (AC: 1, 2, 5)
  - [ ] `Persistence/Configurations/DanhMuc/CanBoConfiguration.cs`: `MaCanBo varchar(20)` NOT NULL + unique index; `HoTen nvarchar(100)` NOT NULL + index `IX_CanBo_HoTen`; `ChucVu nvarchar(100)` NULL; `LaQuanGiao bit` NOT NULL default 0; `DangCongTac bit` NOT NULL default 1. Audit columns and `RowVer` come from the Story 1.2 base configuration.
  - [ ] Migration `AddCanBo`. Run `--migrate` locally afterwards: the startup schema-version check refuses an out-of-date DB.
- [ ] **T3. Application service** (AC: 1–5)
  - [ ] Application `DanhMuc/CanBoService.cs`: `ThemAsync`, `SuaAsync` (includes the `DangCongTac` flag), `TimAsync(string? tuKhoa, bool baoGomNgungCongTac)`. DTOs `CanBoDto` and `LuuCanBoRequest` (with `RowVer` for edits).
  - [ ] FluentValidation `LuuCanBoRequestValidator`: `MaCanBo` required, ≤ 20, letters/digits/`-`/`_` only (it's `varchar`, so no diacritics); `HoTen` required, ≤ 100; `ChucVu` ≤ 100.
  - [ ] Duplicate code: check with `AnyAsync` before saving, and keep the unique index as the backstop. Both paths give the same business error "Mã cán bộ đã tồn tại".
  - [ ] Edit: load the entity, set `OriginalValues[RowVer]` to the client's `RowVer`, change it, save. A concurrency failure becomes the business error "Dữ liệu đã bị người khác thay đổi, vui lòng tải lại".
  - [ ] A small read query for selection lists, `LayCanBoDangCongTacAsync(bool chiQuanGiao)`, returns active staff only. Stories 2.4, 2.9 and 3.x (warden picker) reuse it, so "hidden from every selection list" lives in one place.
  - [ ] **Permission check:** the `IKiemTraQuyen` port arrives in Story 2.3, after this story. Leave a `// Story 2.5: require DM.Them / DM.Sua` marker on each write method. Story 2.5 wires it in and adds the bypass test.
- [ ] **T4. Business-error plumbing** (AC: 2, 5), first master-data screen, so shared from here on
  - [ ] Reuse the business-error type from Story 1.8 if it exists (e.g. `LoiNghiepVuException` carrying a Vietnamese message). Otherwise add it in Application `Common/`.
  - [ ] Infrastructure translates EF failures in one place (the `IAppDbContext.SaveChangesAsync` implementation or a small helper): `DbUpdateConcurrencyException` → `XungDotDuLieuException` (message above); `SqlException` 2601/2627 → a unique-violation error that the service maps to its own message. Application never sees EF or SqlClient exception types.
  - [ ] Stories 2.4, 2.8 and 2.9 reuse both.
- [ ] **T5. WinForms screen** (AC: 1, 3, 5)
  - [ ] `DanhMuc/`: `ICanBoView` + `CanBoPresenter` + `CanBoForm`. A grid (Mã, Họ tên, Chức vụ, Quản giáo, Đang công tác) with a search box, an "Hiện cả người đã nghỉ" (show inactive) checkbox, and Thêm/Sửa opening an edit dialog.
  - [ ] Search runs while typing with a ~300 ms debounce. Use the query pattern recorded by the Story 1.7 spike if it's done; otherwise `LIKE '%' + @tuKhoa + '%'` on `MaCanBo` and `HoTen`, which `Vietnamese_CI_AI` makes diacritic- and case-insensitive. The staff list stays small (tens to a few hundred rows).
  - [ ] Every load and save creates a new DI scope. The form never holds a DbContext.
  - [ ] Add a "Danh mục › Cán bộ" menu item. Story 2.5 attaches its permission.
- [ ] **T6. Tests** (AC: 1–5)
  - [ ] Application unit tests (NSubstitute): the validator rules; a duplicate code returns the business error and saves nothing.
  - [ ] Integration (`[SqlServerFact]`): add, then search "nguyen van" finds "Nguyễn Văn …"; a duplicate `MaCanBo` is rejected; set `DangCongTac = 0` → missing from `LayCanBoDangCongTacAsync` but still found by `TimAsync(..., baoGomNgungCongTac: true)`; two contexts edit the same row → the second gets `XungDotDuLieuException`; each add/edit writes one `Them`/`Sua` row in `NhatKyThaoTac`.
  - [ ] Presenter test: a save error is shown on the view and the dialog stays open.

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

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
