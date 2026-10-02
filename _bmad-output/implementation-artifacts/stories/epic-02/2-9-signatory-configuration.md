---
story: "2.9"
epic: 2
title: Signatory configuration per print template
status: ready-for-dev
size: M
backlogItems: [HT-04]
frsCovered: [FR6]
nfrsTouched: [NFR5, NFR8]
dependsOn: ["2.1", "2.8"]
---

# Story 2.9: Signatory configuration per print template

Status: ready-for-dev

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

- [ ] **T1. Template catalogue** (AC: 1)
  - [ ] Application `BaoCao/MaMauIn.cs`: one static class with the 12 codes as constants (`MaMauIn.BienNhanThu = "BIEN_NHAN_THU"`, …), the only place they're spelled, plus `TatCa` with each code's Vietnamese display name for the screen ("Biên nhận thu tiền gửi lưu ký", "Phiếu chi xuất tiền lưu ký", "Bảng kê theo dõi cá nhân", "Sổ theo dõi tiền gửi lưu ký", "Bảng kê nộp tiền gửi lưu ký", "Phiếu nhập hàng", "Phiếu mua hàng", "Báo cáo nhập, xuất hàng hoá", "Báo cáo doanh thu", "Bảng niêm yết giá", "Bảng theo dõi mua hàng", "Danh mục hàng hoá"). Epic 4's print frame and every template class use these constants.
- [ ] **T2. Entity, table and seed** (AC: 1)
  - [ ] Domain `HeThong/CauHinhKyTen.cs`: `Id`, `MaMauIn`, `ThuTu`, `ChucDanh`, `CanBoId?`. Configuration data, **not** `IAuditable` (see T3 for why). Add it to the audit-column base (`AuditableEntity`) only if that's convenient for `NguoiTaoId`; it isn't required.
  - [ ] Configuration: `MaMauIn varchar(30)` NOT NULL; `ThuTu tinyint` NOT NULL, `CHECK (ThuTu >= 1)`; unique `(MaMauIn, ThuTu)`; `ChucDanh nvarchar(100)` NOT NULL; `CanBoId int` NULL FK → `CanBo` (`Restrict`). Add `CHECK (MaMauIn IN (...12 codes...))`, generated from `MaMauIn.TatCa`, so a typo can't create a 13th template.
  - [ ] Seed via `HasData` in migration `AddCauHinhKyTen`, with fixed Ids, exactly the 39 rows of AC 1 in order (`ThuTu` 1..n), all with `CanBoId = NULL`. Copy the titles **character for character** from AC 1, which was checked against the process doc (e.g. "Người bị tạm giữ, tạm giam/phạm nhân xác nhận").
- [ ] **T3. Application service** (AC: 2, 3)
  - [ ] `HeThong/CauHinhKyTenService.cs`: `LayTheoMauInAsync(maMauIn)` returns the ordered rows with the default staff name and that person's `DangCongTac` flag. `LuuAsync(maMauIn, IReadOnlyList<DongKyTen> dong)` where `DongKyTen(string ChucDanh, int? CanBoId)` and the list order is the left-to-right order.
  - [ ] Save flow:
    1. `IKiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua)`.
    2. Validate: the code is in `MaMauIn.TatCa`; ≥ 1 row (AC 3, message "Mẫu in phải có ít nhất một người ký"); each `ChucDanh` required and ≤ 100; each new `CanBoId` refers to an **active** staff member. An existing default whose person has since left may stay, but can't be newly chosen.
    3. In one transaction, **delete the template's rows and insert the new list** with `ThuTu = 1..n`.
    4. Write one audit row.
  - [ ] **Why replace instead of update in place:** reordering would swap `ThuTu` values, and EF's per-row updates hit the `(MaMauIn, ThuTu)` unique index midway. Replacing the set is simple and atomic.
  - [ ] **Audit (AC 2):** the rows are deleted, so they can't be `IAuditable` (the interceptor throws on `Deleted`, Story 1.3). Log explicitly via `IGhiNhatKy` (before/after overload from 2.3): `HanhDong.Sua`, `TenBang = "CauHinhKyTen"`, `BanGhiId = null`, `DuLieuCu`/`DuLieuMoi` = `{ "MaMauIn": "...", "NguoiKy": [ { "ThuTu": 1, "ChucDanh": "...", "CanBoId": null, "HoTen": null }, ... ] }`. Include `HoTen` so the viewer (2.10) is readable without joins.
  - [ ] "Signer roles that are not staff keep `CanBoId` NULL" is a UI default plus documentation, not a hard rule. The system can't know that "Người gửi" isn't staff. Don't attempt title-based validation (KISS).
  - [ ] The read query is also the contract for the print frame (Story 4.4): titles left to right, default name if set. Return the name even if the person has since left. The print frame decides how to show that, and Epic 4 owns it.
- [ ] **T4. WinForms screen** (AC: 2, 3)
  - [ ] `HeThong/`: `ICauHinhKyTenView` + presenter + form. The template list (display names) on the left. On the right, a grid (Thứ tự, Chức danh, Người ký mặc định) with buttons Thêm dòng, Xoá dòng, Lên, Xuống, Lưu, Huỷ thay đổi.
  - [ ] The default-signer column is a combo: "(để trống)" + **active** staff (`LayCanBoDangCongTacAsync`, 2.1). A current default who has left still shows, marked "(đã nghỉ)", and can be kept.
  - [ ] Show a live preview strip of the signature block (titles in columns left to right, name under each) so the admin sees the printed order. Plain labels are fine; it isn't the PDF.
  - [ ] Read-only without `HT.Sua`. Register "Hệ thống › Cấu hình người ký" (`HT.Xem`) in the menu registry.
- [ ] **T5. Tests** (AC: 1–3)
  - [ ] Integration (seed, AC 1): a table-driven test holding the AC 1 list (code → ordered titles) compares it with the DB after migration. **Exact** string equality, Vietnamese diacritics included. Also: 12 distinct codes = `MaMauIn.TatCa`, and every seeded `CanBoId` is null.
  - [ ] Integration (save): reorder the payout's 4 columns → `ThuTu` 1..4 in the new order, no unique-index error, one `Sua` audit row with before/after lists; an empty list → validation error, nothing changed; an inactive staff member as a new default → rejected; keeping an existing inactive default → allowed; a user without `HT.Sua` → `KhongCoQuyenException`.
  - [ ] Presenter: Lên/Xuống reorder the rows; Xoá on the last remaining row is blocked with the AC 3 message before calling the service.

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

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
