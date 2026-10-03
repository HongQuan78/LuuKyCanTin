---
story: "2.9"
epic: 2
title: Signatory configuration per print template
status: done
size: M
backlogItems: [HT-04]
frsCovered: [FR6]
nfrsTouched: [NFR5, NFR8]
dependsOn: ["2.1", "2.8"]
baseline_commit: c332a7d271304018a9aab82bd9e522aa6262f417
---

# Story 2.9: Signatory configuration per print template

Status: done

## Story

As an administrator,
I want to configure the signature columns of each print template, with an optional default signer name,
So that printed documents show the right titles and names without retyping them.

## Acceptance Criteria

1. **Given** the `CauHinhKyTen` table (`MaMauIn varchar(30)`, `ThuTu tinyint`, UQ (`MaMauIn`, `ThuTu`), `ChucDanh nvarchar(100)`, `CanBoId` NULL = leave the name blank)
   **When** the migration runs
   **Then** the signers of the 12 templates are seeded left to right exactly as in the process doc:
   - `BIEN_NHAN_THU` (receipt): Người gửi · Người nhận · Lãnh đạo đơn vị.
   - `PHIEU_CHI` (payout): Cán bộ theo dõi tiền lưu ký · Người bị tạm giữ, tạm giam/phạm nhân xác nhận · Cán bộ quản giáo xác nhận · Lãnh đạo đơn vị xác nhận.
   - `BANG_KE_CA_NHAN` (per-detainee statement): Cán bộ căn tin · Cán bộ quản giáo · Người bị tạm giữ, tạm giam/phạm nhân · Thủ trưởng đơn vị.
   - `SO_THEO_DOI` (unit ledger book): Cán bộ căn tin · Chỉ huy phụ trách · Kế toán đơn vị · Thủ trưởng đơn vị.
   - `BANG_KE_NOP` (remittance list): Người nộp · Chỉ huy phụ trách · Thủ trưởng đơn vị.
   - `PHIEU_NHAP` (goods receipt): Người giao · Người nhận · Chỉ huy đội · Lãnh đạo đơn vị.
   - `PHIEU_MUA_HANG` (purchase slip): Người mua hàng · Cán bộ căn tin · Lãnh đạo đơn vị.
   - `BAO_CAO_NXT` (stock movement): Cán bộ bán hàng · Chỉ huy phụ trách · Lãnh đạo đơn vị.
   - `BAO_CAO_DOANH_THU` (revenue): Cán bộ căn tin · Chỉ huy phụ trách · Lãnh đạo đơn vị.
   - `BANG_NIEM_YET_GIA` (posted price list): Cán bộ căn tin · Lãnh đạo đơn vị.
   - `THEO_DOI_MUA_HANG` (purchase history): Cán bộ căn tin · Chỉ huy phụ trách · Lãnh đạo đơn vị.
   - `DANH_MUC_HANG_HOA` (goods catalogue): Cán bộ căn tin · Chỉ huy phụ trách · Lãnh đạo đơn vị.

2. **Given** the signatory screen
   **When** the administrator selects a template, then edits titles, reorders columns or picks a default staff member (active staff only)
   **Then** the change is saved and audited; signer roles that are not staff (sender, detainee, buyer, deliverer) keep `CanBoId` NULL

3. **Given** a template with no signer rows
   **When** it is saved
   **Then** validation requires at least one signer column

## Tasks / Subtasks

- [x] **T1. Template catalogue** (AC: 1)
  - [x] Application `Reporting/TemplateCodes.cs`: one static class with the 12 codes as constants (`TemplateCodes.DepositReceipt = "BIEN_NHAN_THU"`, …), the only place they're spelled, plus `All` with each code's Vietnamese display name for the screen ("Biên nhận thu tiền gửi lưu ký", "Phiếu chi xuất tiền lưu ký", "Bảng kê theo dõi cá nhân", "Sổ theo dõi tiền gửi lưu ký", "Bảng kê nộp tiền gửi lưu ký", "Phiếu nhập hàng", "Phiếu mua hàng", "Báo cáo nhập, xuất hàng hoá", "Báo cáo doanh thu", "Bảng niêm yết giá", "Bảng theo dõi mua hàng", "Danh mục hàng hoá"). Epic 4's print frame and every template class use these constants.
- [x] **T2. Entity, table and seed** (AC: 1)
  - [x] Domain `Administration/SignatoryConfiguration.cs`: `Id`, `TemplateCode`, `Ordinal`, `Title`, `OfficerId?`. Configuration data, **not** `IAuditable` (see T3 for why). No audit-column base.
  - [x] Configuration: `TemplateCode varchar(30)` NOT NULL; `Ordinal tinyint` NOT NULL, `CHECK (Ordinal >= 1)`; unique `(TemplateCode, Ordinal)`; `Title nvarchar(100)` NOT NULL; `OfficerId int` NULL FK → `Officer` (`Restrict`). Added `CHECK (TemplateCode IN (...12 codes...))`, generated from `TemplateCodes.All`, so a typo can't create a 13th template.
  - [x] Seed via `HasData` in migration `AddSignatoryConfiguration`, with fixed Ids 1..39, exactly the 39 rows of AC 1 in order (`Ordinal` 1..n), all with `OfficerId = NULL`. Titles copied **character for character** from AC 1, checked against the process doc (e.g. "Người bị tạm giữ, tạm giam/phạm nhân xác nhận").
- [x] **T3. Application service** (AC: 2, 3)
  - [x] `Administration/SignatoryConfigurationService.cs`: `GetByTemplateAsync(templateCode)` returns the ordered rows with the default staff name and that person's active flag. `SaveAsync(templateCode, IReadOnlyList<SignatoryLine> signatories)` where `SignatoryLine(string Title, int? OfficerId)` and the list order is the left-to-right order.
  - [x] Save flow:
    1. `IPermissionChecker.RequireAsync(PermissionCodes.Administration.Update)`.
    2. Validate: the code is in `TemplateCodes.All`; ≥ 1 row (AC 3, message "Mẫu in phải có ít nhất một người ký"); each `Title` required and ≤ 100; each new `OfficerId` refers to an **active** staff member. An existing default whose person has since left may stay, but can't be newly chosen.
    3. In one transaction, **delete the template's rows and insert the new list** with `Ordinal = 1..n`.
    4. Write one audit row.
  - [x] **Why replace instead of update in place:** reordering would swap `Ordinal` values, and EF's per-row updates hit the `(TemplateCode, Ordinal)` unique index midway. Replacing the set is simple and atomic (delete saved first, then insert, in the same transaction).
  - [x] **Audit (AC 2):** the rows are deleted, so they can't be `IAuditable` (the interceptor throws on `Deleted`, Story 1.3). Logged explicitly via `IAuditLogWriter` (before/after overload from 2.3): `AuditAction.Update`, `TableName = "SignatoryConfiguration"`, `RecordId = null`, `OldValues`/`NewValues` = `{ "TemplateCode": "...", "Signatories": [ { "Ordinal": 1, "Title": "...", "OfficerId": null, "FullName": null }, ... ] }`. `FullName` included so the viewer (2.10) is readable without joins.
  - [x] "Signer roles that are not staff keep `OfficerId` NULL" is a UI default plus documentation, not a hard rule. No title-based validation (KISS).
  - [x] The read query is also the contract for the print frame (Story 4.4): titles left to right, default name if set. Returns the name even if the person has since left. The print frame decides how to show that, and Epic 4 owns it.
- [x] **T4. WinForms screen** (AC: 2, 3)
  - [x] `Administration/`: `ISignatoryConfigurationView` + presenter + form. The template list (display names) on the left. On the right, a grid (Thứ tự, Chức danh, Người ký mặc định) with buttons Thêm dòng, Xoá dòng, Lên, Xuống, Lưu, Huỷ thay đổi.
  - [x] The default-signer column is a combo: "(để trống)" + **active** staff (`IOfficerService.GetActiveOfficersAsync`, 2.1). A current default who has left still shows, marked "(đã nghỉ)", and can be kept.
  - [x] Live preview strip of the signature block (titles in columns left to right, name under each) so the admin sees the printed order. Plain labels; it isn't the PDF.
  - [x] Read-only without `Administration.Update`. Registered "Hệ thống › Cấu hình người ký" (`Administration.View`) in the menu registry.
- [x] **T5. Tests** (AC: 1–3)
  - [x] Integration (seed, AC 1): a table-driven test holding the AC 1 list (code → ordered titles) compares it with the DB after migration. **Exact** string equality, Vietnamese diacritics included. Also: 12 distinct codes = `TemplateCodes.All`, and every seeded `OfficerId` is null.
  - [x] Integration (save): reorder the payout's 4 columns → `Ordinal` 1..4 in the new order, no unique-index error, one `Update` audit row with before/after lists; an empty list → validation error, nothing changed; an inactive staff member as a new default → rejected; keeping an existing inactive default → allowed; a user without `Administration.Update` → `PermissionDeniedException`.
  - [x] Presenter: Lên/Xuống reorder the rows; Xoá on the last remaining row is blocked with the AC 3 message before calling the service.

## Dev Notes

### Current codebase state (after 2.8)

- `CanBo` + `LayCanBoDangCongTacAsync` (2.1), `IGhiNhatKy` before/after (2.3), `IKiemTraQuyen` and the menu registry (2.5), the unit info screen (2.8).

### Design notes

- **Seeded once, then owned by the unit.** `HasData` puts the defaults in the migration (NEN-02: fresh install = upgrade). Later edits are data. A future migration must never re-`HasData` these rows with new values, or EF would overwrite the unit's edits. If a template's defaults ever change, write a data migration that only touches unedited rows.
- The process doc writes "Cán bộ căng tin" on the price list. AC 1 normalizes it to "Cán bộ căn tin", the spelling used everywhere else. Keep the AC spelling.
- Epic 7's first-run checklist (7.6) checks "at least one signer per template". AC 3 makes that true by construction after any save, and the seed makes it true from day one.

### Gotchas

- `HasData` with deletes: EF diffs seed data on every `migrations add`. Because these rows are seeded with fixed Ids and later replaced at runtime with new Ids, nothing in the model changes, so later migrations don't touch them. Confirm by adding an empty migration in the test run and checking it's empty.
- `MaMauIn` is `varchar(30)`. The longest code today is `BAO_CAO_DOANH_THU` (17 chars), but have the constants test assert ≤ 30 so a future template can't overflow.

### Out of scope

- Rendering the signature block (Story 4.4 print frame). Per-document signer overrides at print time (not in the spec).

### References

- Epic 2 › Story 2.9; `epics.md` › FR6, FR42
- DB design PDF: `CauHinhKyTen` (p.5); seed "Chức danh ký của 12 mẫu in theo đúng quy trình" (p.15)
- Process doc `document/QUY TRÌNH TIẾP NHẬN TIỀN GỬI LƯU KÝ.docx`: signer lists in A.I.5, A.II.6, A.III.1.3, A.III.2.3, A.III.3.3, B.I, B.II, B.III–VII

## Dev Agent Record

### Agent Model Used

opencode / deepseek-v4.1-flash

### Debug Log References

- `dotnet build LuuKyCanTin.slnx` → 0 errors; only the pre-existing SP-01 QuestPDF WebView2 `MSB3277` warning.
- `dotnet test LuuKyCanTin.slnx` → 896 passed, 0 failed, 0 skipped (Domain 92, Application 177, Integration 280, WinForms 347).
- `dotnet ef migrations has-pending-model-changes --project src/Libraries/LuuKyCanTin.Infrastructure` → "No changes have been made to the model since the last migration."
- `HasData` gotcha check: added a temporary `SeedDiffCheck` migration after `AddSignatoryConfiguration`, confirmed it is empty, then removed it, so later `migrations add` runs don't re-seed the 39 rows.

### Completion Notes List

- **Dependency gate:** 2.1 is `review` (developed; review pending) and 2.8 is `done`; the story files and `stories/README.md` agree, so the gate passes.
- **Translation table** (story text, pre-R.1 → shipped):
  | Story text | Shipped |
  |---|---|
  | Domain `HeThong/CauHinhKyTen.cs` | Domain `Administration/SignatoryConfiguration.cs` |
  | Application `BaoCao/MaMauIn.cs` | Application `Reporting/TemplateCodes.cs` |
  | `MaMauIn` | `TemplateCode` |
  | `MaMauIn.TatCa` | `TemplateCodes.All` (with each code's Vietnamese display name) |
  | `ThuTu` | `Ordinal` |
  | `ChucDanh` | `Title` |
  | `CanBoId` | `OfficerId` |
  | `DongKyTen` | `SignatoryLine` |
  | `CauHinhKyTenService` / `ICauHinhKyTenService` | `SignatoryConfigurationService` / `ISignatoryConfigurationService` |
  | `LayTheoMauInAsync` | `GetByTemplateAsync` |
  | `LuuAsync` | `SaveAsync` |
  | `DangCongTac` | active flag on `SignatoryRowDto.IsOfficerActive` |
  | `IKiemTraQuyen` / `KhongCoQuyenException` | `IPermissionChecker` / `PermissionDeniedException` |
  | `MaQuyen.HT.Sua` / `HT.Xem` | `PermissionCodes.Administration.Update` / `.View` |
  | `LayCanBoDangCongTacAsync` | `IOfficerService.GetActiveOfficersAsync` |
  | audit `HanhDong.Sua` | `AuditAction.Update` |
  | audit `TenBang = "CauHinhKyTen"` | table `SignatoryConfiguration`; the `LegacyAuditNames.Tables` entry `CauHinhKyTen` → `SignatoryConfiguration` exists only because the existing invariant test requires every current table to have a legacy name — this table is new, so no pre-R.1 audit rows can hold the old name |
  | payload `MaMauIn` / `NguoiKy` / `HoTen` | payload `TemplateCode` / `Signatories` / `FullName` (with `Ordinal`, `Title`, `OfficerId`) |
  | menu "Hệ thống › Cấu hình người ký" | `ShellNavigation.SignatoryConfigurationKey` in the Hệ thống group, requiring `Administration.View` |
- **T1:** `TemplateCodes` is the only place the 12 codes are spelled. `DepositReceiptModel.PrintTemplate` now references `TemplateCodes.DepositReceipt` instead of the literal. Unit tests pin the 12 constants, distinct codes, non-empty display names and the `varchar(30)` bound.
- **T2:** `SignatoryConfiguration` is deliberately not `IAuditable` and has no audit columns (a save replaces the row set, so the interceptor would see only deletes and adds). The table carries both check constraints, the unique `(TemplateCode, Ordinal)` index and the `Restrict` FK to `Officer`. `AddSignatoryConfiguration` seeds ids 1..39 in AC 1 order with `OfficerId = NULL`. Titles are asserted character for character by an integration test.
- **T3:** `SaveAsync` checks `Administration.Update` before any read, validates in memory, then inside one transaction deletes the template's rows (`SaveChanges`), inserts the new list with `Ordinal = 1..n` (`SaveChanges`), writes one explicit audit row and commits. Two saves inside the transaction guarantee delete-before-insert, so the unique index is never hit midway. The audit payload holds the ordered before/after lists including `FullName`. `GetByTemplateAsync` left-joins `Officer`, so a blank line materializes as null and a retired default still returns its name with `IsOfficerActive = false`.
- **T4:** No mockup exists for this screen, so it follows the closest prototype, the role screen's master-detail pattern (left list card, right card with grid and footer), plus a plain-label preview strip. The presenter owns add/remove/reorder/save; the view only binds and reads the grid. The combo offers "(để trống)" + active staff + any retired default already used, marked "(đã nghỉ)". Without `Administration.Update` the grid is read-only and all write buttons are disabled. Sidebar mnemonic `g` keeps the sidebar's mnemonic-uniqueness test green.
- **T5:** 8 integration tests (2 seed + 6 save/read), 11 presenter tests and 4 screen tests; `ShellNavigationTests`/`ShellNavigationFilterTests` cover the new menu item and its `Administration.View` gate.
- **Notes / risks:** `GetByTemplateAsync` has no permission check, consistent with the other read queries (the menu carries `Administration.View`; writes re-check). The 39 seeded rows have fixed ids and are replaced at runtime with new identity ids, so later migrations never touch them (verified with the temporary empty migration). Per-document signer overrides at print time and rendering the block remain Story 4.4.

### File List

New:
- `src/Libraries/LuuKyCanTin.Application/Reporting/TemplateCodes.cs`
- `src/Libraries/LuuKyCanTin.Domain/Administration/SignatoryConfiguration.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/SignatoryLine.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/SignatoryRowDto.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/ISignatoryConfigurationService.cs`
- `src/Libraries/LuuKyCanTin.Application/Administration/SignatoryConfigurationService.cs`
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Configurations/Administration/SignatoryConfigurationConfiguration.cs`
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Migrations/20261003191123_AddSignatoryConfiguration.cs` (+ `.Designer.cs`)
- `src/Presentation/LuuKyCanTin.WinForms/Administration/ISignatoryConfigurationView.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/SignatoryConfigurationPresenter.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/SignatoryConfigurationForm.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Administration/SignatoryConfigurationForm.Designer.cs`
- `tests/LuuKyCanTin.Application.UnitTests/Reporting/TemplateCodesTests.cs`
- `tests/LuuKyCanTin.IntegrationTests/Administration/SignatoryConfigurationSeedTests.cs`
- `tests/LuuKyCanTin.IntegrationTests/Administration/SignatoryConfigurationServiceTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Administration/SignatoryConfigurationPresenterTests.cs`
- `tests/LuuKyCanTin.WinForms.UnitTests/Administration/SignatoryConfigurationScreenTests.cs`

Modified:
- `src/Libraries/LuuKyCanTin.Application/Reporting/DepositReceiptModel.cs` (uses `TemplateCodes.DepositReceipt`)
- `src/Libraries/LuuKyCanTin.Application/Abstractions/IAppDbContext.cs` (exposes `SignatoryConfiguration`)
- `src/Libraries/LuuKyCanTin.Application/ApplicationServiceCollectionExtensions.cs` (scoped service registration)
- `src/Libraries/LuuKyCanTin.Application/Administration/LegacyAuditNames.cs` (`CauHinhKyTen` → `SignatoryConfiguration`)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/AppDbContext.cs` (`DbSet<SignatoryConfiguration>`)
- `src/Libraries/LuuKyCanTin.Infrastructure/Persistence/Migrations/AppDbContextModelSnapshot.cs`
- `src/Presentation/LuuKyCanTin.WinForms/Shell/INavigator.cs`, `Navigator.cs`, `ShellNavigation.cs` (menu item + screen host)
- `tests/LuuKyCanTin.Application.UnitTests/TestUtilities/InMemoryAppDbContext.cs` (test double)
- `tests/LuuKyCanTin.WinForms.UnitTests/Shell/ShellNavigationTests.cs`, `ShellNavigationFilterTests.cs` (cover the new menu item)

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | `(byte)(i + 1)` wraps at 256 signer lines with an opaque DB error | low — patch | Cap the list with a business message and test it. |
| 2 | The presenter reports success after a reload that failed and was swallowed | medium — patch | Make the reload return success and show the confirmation only then. |
| 3 | No concurrency token; two admins saving one template silently overwrite | low — reject | Replace-set is the story's explicit design and there is no natural row token for a replaced set; last-write-wins on configuration is acceptable. |
| 4 | `GetByTemplateAsync` returns empty for an unknown code, the future print contract | low — patch | Throw `InvalidOperationException` for a code outside the catalogue, like other programming-error cases. |
| 5 | The service XML doc still says `HT.Sua` and omits two exception cases | low — patch | Update the contract docs. |
| 6 | The configuration comment claims "any byte fails the lower bound" | low — patch | Comment only; correct it. |
| 7 | Story status vs Change Log contradiction | false | Fixed at presentation. |
| 8 | The `LegacyAuditNames` note misstates why the entry exists | low — patch (docs) | Correct the Dev Agent Record wording. |
| 9 | Blank/over-length title and unknown-officer branches are untested | medium — patch | Add the three service tests. |
| 10 | The template-code CHECK and unique ordinal index are untested | low — patch | Add raw-insert rejection tests for a 13th code and a duplicate ordinal. |
| 11 | The presenter's `Loaded`, template switch, discard and failure banners are untested | medium — patch | Add the presenter tests following the sibling patterns. |
| 12 | Template display names are not pinned exactly | low — patch | Assert the 12 Vietnamese strings in catalogue order. |
| 13 | The preview strip and the retired-officer choice are untested | low — patch | Add the screen assertions. |
| 14 | The preview updates only on committed cell edits, not while typing | low — patch | Commit the edit on dirty-state change so the strip follows typing. |
| 15 | A retired default disappears from the combo after its line is replaced | low — patch | Keep officers referenced by the original snapshot in the choice list. |
| 16 | Switching template or discarding leaves a stale banner | low — patch | Clear the message on both paths. |
| 17 | `HasData` rows are runtime-editable, so a future seed diff could revert unit edits | low — defer | Inherent to `HasData`; the story documents the data-migration policy. Recorded in `deferred-work.md`. |
| 18 | `_rows` is dead presenter state | low — patch | Remove it. |
| 19 | A template switch during an in-flight load can bind stale rows and save under the new code | medium — patch | Compare the selected code before applying the load result, and clear the grid plus disable Save when a load fails. |
| 20 | The real `Navigator.OpenSignatoryConfiguration` is not exercised by any test | low — patch | Add the signatory case to the module-screen hosting theory. |

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
| 2026-10-04 | Implemented T1–T5 on the English base. |
| 2026-10-04 | Reviewed (blind hunter, edge cases, verification gaps) and patched: row cap, unknown-template read error, stale-load guards, failure banners, retired-officer choices, and the missing test coverage; status → done. |
