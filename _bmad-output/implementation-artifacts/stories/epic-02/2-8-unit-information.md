---
story: "2.8"
epic: 2
title: Unit information
status: done
size: S
backlogItems: [HT-03]
frsCovered: [FR5]
nfrsTouched: [NFR4, NFR5, NFR8]
dependsOn: ["2.5"]
baseline_commit: 81c1fd4652a8451fd10449e1934c76fe692df304
---

# Story 2.8: Unit information

Status: done

## Story

As an administrator,
I want to enter the unit's parent agency, name and address once,
So that every printed form carries the correct heading.

## Acceptance Criteria

1. **Given** the `ThongTinDonVi` table from Story 1.8 (`Id = 1` only, `TenCoQuanChuQuan nvarchar(200)` optional, `TenDonVi nvarchar(200)` NOT NULL, `DiaChi nvarchar(300)` NOT NULL)
   **When** an administrator with `HT.Sua` edits and saves the unit information
   **Then** the single row is updated, audited with before/after values, and used by every print from then on

2. **Given** an empty unit name or address
   **When** the user saves
   **Then** validation shows which field is required and nothing is saved

3. **Given** a user without `HT.Sua`
   **When** they open the screen
   **Then** it is read-only, and the service rejects any save attempt

## Tasks / Subtasks

- [x] **T1. Entity and audit** (AC: 1)
  - [x] `ThongTinDonVi` (Domain `HeThong/`, from 1.8) implements `IAuditable`, so each save writes a `Sua` row with the changed fields' before/after values automatically. If 1.8 didn't derive it from `AuditableEntity`, do it here and add the audit columns + `RowVer` in the migration. Two admins editing at once then get the Story 2.1 conflict message.
  - [x] Migration (only if the columns change): `UpdateThongTinDonViAudit`. The seeded row `Id = 1` keeps its empty strings, so set the new audit columns' defaults (`NgayTao` = migration time via SQL default, `NguoiTaoId = 0`) so the existing row stays valid.
- [x] **T2. Application** (AC: 1, 2, 3)
  - [x] `HeThong/ThongTinDonViService.cs`: `LayAsync()` returns a DTO with `RowVer`; `LuuAsync(LuuThongTinDonViRequest)` does `IKiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua)` → validate → load `Id = 1` → apply → save.
  - [x] FluentValidation: `TenDonVi` required, ≤ 200; `DiaChi` required, ≤ 300; `TenCoQuanChuQuan` optional, ≤ 200. Trim everything. Whitespace-only counts as empty. Messages per field, e.g. "Tên đơn vị không được để trống".
  - [x] Add `bool DaCauHinh` (name and address not empty) to the DTO. The first-run checklist (Story 7.6) and the print frame (4.4) can then tell "never filled in" from filled.
  - [x] Never insert or delete. The service only updates the single seeded row. A missing row is an installation error: throw `InvalidOperationException`.
- [x] **T3. WinForms screen** (AC: 2, 3)
  - [x] `HeThong/`: `IThongTinDonViView` + presenter + form. Three text boxes with length limits, and a small preview label showing how the header lines will print (agency in capitals on top if set, then unit name, then address).
  - [x] Validation errors show next to the field (`ErrorProvider`) and focus the first invalid one.
  - [x] Read-only when `!HasPermission(HT.Sua)`: boxes read-only, Lưu hidden.
  - [x] Register "Hệ thống › Thông tin đơn vị" in the 2.5 menu registry with `HT.Xem`.
- [x] **T4. Tests** (AC: 1–3)
  - [x] Unit: validator (empty, whitespace, too long, optional agency).
  - [x] Integration:
    - save → row updated, and one `Sua` row with before/after JSON of the changed fields only;
    - empty name → nothing saved, no audit row;
    - a user with only `HT.Xem` → `KhongCoQuyenException`, no change;
    - stale `RowVer` → conflict error;
    - the Story 1.8 print query (`LayBienNhanThuDeInQuery` or its successor) returns the **new** name after a save. That covers "used by every print from then on".
  - [x] Presenter: read-only mode for a user without `HT.Sua`.

## Dev Notes

### Current codebase state (after 2.5)

- `ThongTinDonVi` table and seeded empty row (1.8), read by the receipt print query (1.8). The menu registry, `IKiemTraQuyen` and the friendly business messages (2.5).

### Design notes

- **No history or snapshot of the unit header.** Old documents reprint with the current header. The spec snapshots names, types and buyers on documents (NFR8), not the unit's own name, and a unit renaming itself is rare. If the PO wants reprints to show the old header, that's a new requirement: raise it, don't build it.
- The print frame (Story 4.4) reads this row at print time. Don't cache it in a singleton, or another workstation's edit wouldn't show until restart.

### Out of scope

- A logo on prints (not in the spec). The first-run checklist (7.6).

### References

- Epic 2 › Story 2.8; `epics.md` › FR5, FR42
- DB design PDF: `ThongTinDonVi` (p.4); seed "một dòng trống để quản trị điền" (p.15)
- Process doc: every template starts with "Tên đơn vị", "Địa chỉ đơn vị"

## Dev Agent Record

### Agent Model Used

opencode / deepseek-v4.1-flash

### Debug Log References

- `dotnet build LuuKyCanTin.slnx` → 0 errors; only the pre-existing SP-01 QuestPDF WebView2 `MSB3277` warning.
- `dotnet test LuuKyCanTin.slnx` → 847 passed, 0 failed, 0 skipped (Domain 92, Application 172, Integration 265, WinForms 318).
- `dotnet ef migrations has-pending-model-changes --project src/Libraries/LuuKyCanTin.Infrastructure` → "No changes have been made to the model since the last migration."

### Completion Notes List

- **Dependency gate:** 2.5 is `done` in both its story file and `stories/README.md`; the gate passes.
- **Translation table** (story text, pre-R.1 → shipped):
  | Story text | Shipped |
  |---|---|
  | `ThongTinDonVi` | `FacilityInfo` (already existed) |
  | Domain `HeThong/` | `Administration/` |
  | `ThongTinDonViService` / `IThongTinDonViService` | `FacilityInfoService` / `IFacilityInfoService` |
  | `LayAsync` | `GetAsync` |
  | `LuuAsync(LuuThongTinDonViRequest)` | `SaveAsync(SaveFacilityInfoRequest, ct)` |
  | `LuuThongTinDonViRequest` | `SaveFacilityInfoRequest` |
  | `ThongTinDonViDto` | `FacilityInfoDto` |
  | `TenCoQuanChuQuan` / `TenDonVi` / `DiaChi` | `ParentAgencyName` / `FacilityName` / `Address` |
  | `DaCauHinh` | `IsConfigured` |
  | `RowVer` | `RowVer` (unchanged) |
  | `IKiemTraQuyen` / `KhongCoQuyenException` | `IPermissionChecker` / `PermissionDeniedException` |
  | `MaQuyen.HT.Sua` / `HT.Xem` | `PermissionCodes.Administration.Update` / `.View` |
  | audit row `Sua` | `AuditAction.Update` |
  | menu "Hệ thống › Thông tin đơn vị" | `ShellNavigation.FacilityInfoKey` in the Hệ thống group, requiring `Administration.View` |
- **T1:** `FacilityInfo` now derives `AuditableEntity` and implements `IAuditable`. The table already carried the audit columns and `RowVer` (R.1 migration), so no migration was needed; `has-pending-model-changes` confirms the model is unchanged. A save now writes one `AuditAction.Update` row through the existing interceptor.
- **T2:** `IAppDbContext.FacilityInfo` was added to the interface (the concrete `AppDbContext` already exposed the DbSet) so the service can load the tracked row; `FacilityInfoStore` stays the read-only path for the print queries. `GetAsync` returns the DTO with `RowVer` and `IsConfigured` (name **and** address non-empty). `SaveAsync` requires `Administration.Update` first, validates, requires the loaded `RowVer`, loads `Id = 1` tracked, sets `OriginalValue` from the request, applies trimmed values (a blank agency normalizes to `null`), and saves. A missing row throws the English developer `InvalidOperationException`; a stale version surfaces as `ConcurrencyConflictException` through the context's translation.
- **T3:** No exact mockup exists for this screen, so it follows the key-02 B edit-dialog pattern hosted in the shell content area like `RoleForm` (card, labels above `InputFrame` fields in a 2-column grid, `FieldError` lines under the fields, one primary `Lưu` + secondary `Làm mới` in a right-aligned footer). Length limits: agency 200, name 200, address 300. Validation failures map to `FacilityInfoField` and the first invalid field is focused. The preview label reflects the printed header live (agency upper-cased when set, then name, then address). Without `Administration.Update` the boxes are read-only and both buttons are hidden. `Navigator.OpenFacilityInfo` hosts the screen; the sidebar item is permission-gated with `Administration.View`.
- **T4:** Validator unit tests, service integration tests (update + one Update audit row with only the changed field's before/after, blank name writes nothing, view-only user denied, stale row version conflicts, `DepositReceiptPrintQuery` returns the new name after a save) and presenter/screen tests (read-only mode, load/save flow, field errors, banner, mnemonics, preview).
- **Out of scope, per the story:** a logo on prints; the first-run checklist (7.6). Old documents intentionally reprint with the current header (design note).

### File List

New:
- `src/Libraries/LuuKyCanTin.Application/Administration/SaveFacilityInfoRequest.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/FacilityHeaderLines.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/FacilityInfoDto.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/IFacilityInfoService.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/FacilityInfoService.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/SaveFacilityInfoRequestValidator.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/FacilityInfoField.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/IFacilityInfoView.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/FacilityInfoPresenter.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/FacilityInfoForm.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/FacilityInfoForm.Designer.cs`
- `tests/LuuKyCanTin.Application.UnitTests/Administration/SaveFacilityInfoRequestValidatorTests.cs`
- `tests/LuuKyCanTin.IntegrationTests/Administration/FacilityInfoServiceTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Administration/FacilityInfoPresenterTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Administration/FacilityInfoScreenTests.cs`

Modified:
- `src/Libraries/LuuKyCanTin.Domain/Administration/FacilityInfo.cs` (implements `IAuditable`)
- `src/Libraries/LuuKyCanTin.Application/Abstractions/IAppDbContext.cs` (exposes `FacilityInfo`)
- `src/Libraries/LuuKyCanTin.Application/ApplicationServiceCollectionExtensions.cs` (validator + service registration)
- `src/Presentation/LuuKyCanTin.WinForms/Shell/INavigator.cs`, `Navigator.cs`, `ShellNavigation.cs` (menu item + screen host)
- `tests/LuuKyCanTin.Application.UnitTests/TestUtilities/InMemoryAppDbContext.cs` (test double)
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/ShellNavigationTests.cs`, `MainPresenterTests.cs`, `ShellScreenTests.cs` (cover the new menu item)

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | README still lists 2.8 as `ready-for-dev` while the story is in review | false | The presentation step syncs the README and status together. |
| 2 | Change Log says "stays in-progress" while the status is review | false | Doc metadata fixed at presentation. |
| 3 | The preview uppercases the agency while the printed template keeps the stored casing | medium — patch | Align the template and the preview on one format (capitals per the story) and cover it through the print query. |
| 4 | `GetAsync` has no permission check | false | 2.5's design note scopes service checks to writes; the menu item carries `Administration.View` and the row is not sensitive. |
| 5 | `IsConfigured` is untested | low — patch | Cover the blank seeded row (false) and a saved row (true). |
| 6 | The blank-agency (null) save path is untested | low — patch | Save an empty agency and assert the stored null and the audit payload. |
| 7 | The missing-row `InvalidOperationException` is untested and escapes the presenter's `async void` load | low — patch | Test the missing row and catch load failures in the presenter. |
| 8 | Save is enabled before the first load; an early click silently does nothing | low — patch | Disable save until loaded (or surface a message). |
| 9 | The view-only denial test never asserts the granted `View` permission exists | low — patch | Assert the seeded role actually holds `Administration.View`. |
| 10 | The service interface doc still says `HT.Sua` | low — patch | Update to `PermissionCodes.Administration.Update`. |
| 11 | `RowVer` is optional in the request but required by the service, which throws a raw `ArgumentException` | low — patch | Make it required or validate it through the request. |
| 12 | `byte[]` `RowVer` makes record equality reference-based in tests | low — reject | Production never relies on record equality; the tests can compare arrays explicitly. |
| 13 | `FacilityInfoField` is not declared `: byte` | low — patch | Matches the enum rule in the naming conventions. |
| 14 | No test proves the post-save reload carries a new row version | low — patch | Second save after a reload must succeed with the returned version. |
| 15 | The real form's field-error wiring and live preview are untested | medium — patch | Mirror the officer-form screen test: `ShowFieldErrors` marks the right frames and focuses the first, and typing updates the preview. |
| 16 | Validator boundary tests cover only the name | low — patch | Add the 200/300 boundary cases for the other fields. |
| 17 | The presenter's load/save `async void` paths let DB errors escape to the global handler | low — patch | Catch and show a message on both paths. |
| 18 | `IsConfigured` uses `IsNullOrEmpty`, so a whitespace-only stored row reports configured | low — patch | Use `IsNullOrWhiteSpace`. |
| 19 | Validator `PropertyName`s are never asserted, so the field mapping can silently break | medium — patch | Assert the emitted property names or feed the real validator into the presenter test. |
| 20 | Trimming at save time is not observed by any test | low — patch | Save padded values and assert the stored row is trimmed. |
| 21 | The new sidebar item's permission gate is not pinned | low — patch | Add `FacilityInfoKey` to the hidden/visible filter test. |
| 22 | The new service/validator DI registrations are not resolved by any test | low — patch | Add them to the registration resolvability theory. |

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
| 2026-10-04 | Implemented T1–T4 on the English base. |
| 2026-10-04 | Reviewed (blind hunter, edge cases, verification gaps) and patched: shared header formatter for preview and print, stricter `IsConfigured`, required row version, presenter error/loading guards, and the missing test coverage; status → done. |
