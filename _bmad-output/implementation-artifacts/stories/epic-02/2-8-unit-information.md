---
story: "2.8"
epic: 2
title: Unit information
status: ready-for-dev
size: S
backlogItems: [HT-03]
frsCovered: [FR5]
nfrsTouched: [NFR4, NFR5, NFR8]
dependsOn: ["2.5"]
---

# Story 2.8: Unit information

Status: ready-for-dev

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

- [ ] **T1. Entity and audit** (AC: 1)
  - [ ] `ThongTinDonVi` (Domain `HeThong/`, from 1.8) implements `IAuditable`, so each save writes a `Sua` row with the changed fields' before/after values automatically. If 1.8 didn't derive it from `AuditableEntity`, do it here and add the audit columns + `RowVer` in the migration. Two admins editing at once then get the Story 2.1 conflict message.
  - [ ] Migration (only if the columns change): `UpdateThongTinDonViAudit`. The seeded row `Id = 1` keeps its empty strings, so set the new audit columns' defaults (`NgayTao` = migration time via SQL default, `NguoiTaoId = 0`) so the existing row stays valid.
- [ ] **T2. Application** (AC: 1, 2, 3)
  - [ ] `HeThong/ThongTinDonViService.cs`: `LayAsync()` returns a DTO with `RowVer`; `LuuAsync(LuuThongTinDonViRequest)` does `IKiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua)` → validate → load `Id = 1` → apply → save.
  - [ ] FluentValidation: `TenDonVi` required, ≤ 200; `DiaChi` required, ≤ 300; `TenCoQuanChuQuan` optional, ≤ 200. Trim everything. Whitespace-only counts as empty. Messages per field, e.g. "Tên đơn vị không được để trống".
  - [ ] Add `bool DaCauHinh` (name and address not empty) to the DTO. The first-run checklist (Story 7.6) and the print frame (4.4) can then tell "never filled in" from filled.
  - [ ] Never insert or delete. The service only updates the single seeded row. A missing row is an installation error: throw `InvalidOperationException`.
- [ ] **T3. WinForms screen** (AC: 2, 3)
  - [ ] `HeThong/`: `IThongTinDonViView` + presenter + form. Three text boxes with length limits, and a small preview label showing how the header lines will print (agency in capitals on top if set, then unit name, then address).
  - [ ] Validation errors show next to the field (`ErrorProvider`) and focus the first invalid one.
  - [ ] Read-only when `!HasPermission(HT.Sua)`: boxes read-only, Lưu hidden.
  - [ ] Register "Hệ thống › Thông tin đơn vị" in the 2.5 menu registry with `HT.Xem`.
- [ ] **T4. Tests** (AC: 1–3)
  - [ ] Unit: validator (empty, whitespace, too long, optional agency).
  - [ ] Integration:
    - save → row updated, and one `Sua` row with before/after JSON of the changed fields only;
    - empty name → nothing saved, no audit row;
    - a user with only `HT.Xem` → `KhongCoQuyenException`, no change;
    - stale `RowVer` → conflict error;
    - the Story 1.8 print query (`LayBienNhanThuDeInQuery` or its successor) returns the **new** name after a save. That covers "used by every print from then on".
  - [ ] Presenter: read-only mode for a user without `HT.Sua`.

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

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
