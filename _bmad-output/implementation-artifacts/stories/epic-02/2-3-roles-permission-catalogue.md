---
story: "2.3"
epic: 2
title: Roles and permission catalogue
status: ready-for-dev
size: M
backlogItems: [HT-02]
frsCovered: [FR2]
frsPartial: true
nfrsTouched: [NFR5, NFR6, NFR11]
dependsOn: ["2.2"]
---

# Story 2.3: Roles and permission catalogue

Status: ready-for-dev

## Story

As an administrator,
I want a fixed catalogue of permissions (module × action) and the 6 standard roles seeded,
So that I grant access by role instead of configuring every user by hand.

## Acceptance Criteria

1. **Given** the tables `VaiTro` (`Ma varchar(30)` UQ, `Ten`), `Quyen` (`Ma varchar(50)` UQ, `Ten`, `Module varchar(10)`) and `VaiTroQuyen` (composite PK)
   **When** the migration runs
   **Then** `Quyen` holds one row per module × action in the form `<Module>.<Action>` (for example `LK-C.Duyet`, `BH.Huy`), for modules `HT, DM, LK-T, LK-C, LK-BC, NH, BH, HH-BC` and actions `Xem, Them, Sua, Huy, In, Duyet`

2. **Given** permission codes used in code
   **When** they are referenced
   **Then** they come from one static catalogue class in Application (no string literals scattered in services)
   **And** a unit test fails if a catalogue entry has no seeded `Quyen` row, or if a seeded row has no catalogue entry
   **And** later stories may add special permissions to the catalogue with their own seed rows and default role grants. The registry of special permissions, each seeded by the story that needs it, is: `HT.KhoaSo` (lock/unlock period, 6.8, Kế toán), `LK.LuiNgay` (back-dating, 6.9), `HT.SaoLuu` / `HT.PhucHoi` (backup/restore, 7.1, Quản trị), `HT.CauHinh` (business settings, e.g. 14.6, Quản trị), `HT.QuanTri` (training mode, support bundle, 14.2/14.14, Quản trị)

3. **Given** the seed
   **When** the database is created
   **Then** these 6 roles exist with their default permissions from the feature list:
   - *Quản trị hệ thống*: all of `HT`, all of `DM`.
   - *Cán bộ theo dõi tiền lưu ký*: `LK-T`, `LK-C`, `LK-BC` (all actions except `Duyet`), `DM.Xem`.
   - *Cán bộ căn tin / bán hàng*: `NH`, `BH`, `HH-BC`, `LK-BC.Xem`.
   - *Cán bộ quản giáo*: `LK-BC.Xem` (the purchase-registration permission and the "own detainees only" scope are added in Epic 12).
   - *Chỉ huy phụ trách / Lãnh đạo đơn vị*: `Duyet` on `LK-T` (opening balances, 7.5), `LK-C` (transfers, 5.5) and `NH` (opening stock, 9.6), plus `Xem` on every report module, plus `HT.Xem` (audit-log viewer, 2.10).
   - *Kế toán đơn vị*: `Xem` on `LK-BC` and `HH-BC` (the period-lock permission `HT.KhoaSo` is granted when Story 6.8 seeds it).

4. **Given** the administrator opens Roles
   **When** they tick or untick permissions for a role and save
   **Then** `VaiTroQuyen` is updated and an audit row records the before/after permission list

## Tasks / Subtasks

- [ ] **T1. Permission catalogue** (AC: 1, 2)
  - [ ] Application `HeThong/MaQuyen.cs`: one static class, the **only** place permission codes are spelled. C# identifiers can't contain `-`, so use nested classes per module, e.g. `MaQuyen.HT.Xem = "HT.Xem"`, `MaQuyen.LKC.Duyet = "LK-C.Duyet"`, `MaQuyen.HHBC.In = "HH-BC.In"`.
  - [ ] `MaQuyen.TatCa`: `IReadOnlyList<DinhNghiaQuyen>` with `record DinhNghiaQuyen(int Id, string Ma, string Ten, string Module)`. **Ids are explicit and append-only.** They are the `HasData` keys, so never renumber or reorder. A later special permission (e.g. `HT.KhoaSo`) gets the next free Id.
  - [ ] 8 modules × 6 actions = 48 rows. Vietnamese names like "Xem — Hệ thống", "Duyệt — Giảm tiền lưu ký". Seed every combination even when one makes little sense (`LK-BC.Them`): the AC asks for the full grid, and it keeps the screen regular.
  - [ ] Also create `MaVaiTro` constants for the 6 roles: `QUAN_TRI`, `LUU_KY`, `CAN_TIN`, `QUAN_GIAO`, `LANH_DAO`, `KE_TOAN`.
- [ ] **T2. Domain + persistence** (AC: 1, 3)
  - [ ] Domain `HeThong/VaiTro.cs` (`Ma`, `Ten`, collection of granted permission ids or a `VaiTroQuyen` join entity), `Quyen.cs` (`Ma`, `Ten`, `Module`). `Quyen` is reference data, not `AuditableEntity`. It changes only through migrations. `VaiTro` is `AuditableEntity`, because its permission set is edited.
  - [ ] Configurations: `VaiTro.Ma varchar(30)` UQ, `Ten nvarchar(100)`; `Quyen.Ma varchar(50)` UQ, `Ten nvarchar(150)`, `Module varchar(10)`; `VaiTroQuyen (VaiTroId, QuyenId)` composite PK with FKs (`Restrict`); `NguoiDungVaiTro (NguoiDungId, VaiTroId)` composite PK with FKs. Create `NguoiDungVaiTro` now so that `IKiemTraQuyen` (T4) has something to query. Story 2.4 builds the screen that fills it.
  - [ ] Seed in the migration via `HasData`: `Quyen` from `MaQuyen.TatCa`; the 6 `VaiTro` rows with fixed Ids; `VaiTroQuyen` per AC 3 (an Infrastructure `Seed/VaiTroMacDinh.cs` maps role code → permission codes, built from `MaQuyen` constants, never literals). Grant the seeded `admin` the *Quản trị hệ thống* role (`NguoiDungVaiTro`) in the same migration, or nobody can administer.
  - [ ] Migration `AddVaiTroQuyen`.
- [ ] **T3. Role editing** (AC: 4)
  - [ ] Application `HeThong/VaiTroService.cs`: `LayDanhSachAsync()`, `LayQuyenCuaVaiTroAsync(vaiTroId)`, `CapNhatQuyenAsync(vaiTroId, IReadOnlyCollection<string> maQuyen, byte[] rowVer)`.
  - [ ] Reject unknown codes (only `MaQuyen.TatCa`). Diff old vs new, then insert/delete `VaiTroQuyen` rows and touch the `VaiTro` row so its `RowVer` changes. Two admins editing the same role then conflict cleanly (Story 2.1 T4 error).
  - [ ] **Audit**: `VaiTroQuyen` is a join table with a composite key and rows are deleted, so the interceptor can't log it. The Story 1.3 interceptor throws on a `Deleted` `IAuditable`, so don't mark it. Write one explicit row via `IGhiNhatKy`: `HanhDong.Sua`, `TenBang = "VaiTro"`, `BanGhiId = vaiTroId`, `DuLieuCu = { "Quyen": [sorted old codes] }`, `DuLieuMoi = { "Quyen": [sorted new codes] }`.
  - [ ] `IGhiNhatKy.GhiAsync` takes only the new data today. Add an overload or optional parameter `object? duLieuCu`. `NhatKyFactory` already builds both columns, so it's a small change. Stories 2.4 and 2.9 need it too.
  - [ ] The whole update (rows + audit) in one transaction.
  - [ ] **Last-admin guard:** removing `HT.Sua` from a role can leave the system without an administrator. Story 2.4 builds that guard and applies it to this method too. Leave a pointer comment here.
- [ ] **T4. Permission check port** (enables 2.4 and 2.5)
  - [ ] Domain `HeThong/KhongCoQuyenException.cs` (message "Bạn không có quyền thực hiện thao tác này", carries the required code). Story 2.5's AC calls it a domain exception.
  - [ ] Application `Abstractions/IKiemTraQuyen.cs`: `Task YeuCauAsync(string maQuyen, CancellationToken ct)` throws `KhongCoQuyenException` when the current user lacks it. Infrastructure implementation: one query against the **DB** (not a cache) joining `NguoiDung` (must be `DangHoatDong = 1`) → `NguoiDungVaiTro` → `VaiTroQuyen` → `Quyen.Ma`, so a revoked permission or a deactivated account stops working immediately (Story 2.5 AC 4). No signed-in user → throw.
  - [ ] `VaiTroService.CapNhatQuyenAsync` calls `YeuCauAsync(MaQuyen.HT.Sua)` first, before any transaction. Story 2.5 adds the UI side, the menu and the bypass test for every service.
- [ ] **T5. WinForms "Vai trò" screen** (AC: 4)
  - [ ] `HeThong/`: `IVaiTroView` + presenter + form. A role list on the left. On the right, a grid with rows = modules (Vietnamese names) and columns = the 6 actions, as checkboxes. Special permissions (none yet) go in a separate "Quyền đặc biệt" list below, so they show up automatically when later stories add them. Buttons: Lưu, Huỷ thay đổi.
  - [ ] Role names and codes are read-only. Adding or deleting roles is out of scope (6 fixed roles, KISS).
- [ ] **T6. Tests** (AC: 1–4)
  - [ ] Unit (Application): `MaQuyen.TatCa` has 48 standard entries, unique codes, unique Ids, and every code matches `^[A-Z-]+\.[A-Za-z]+$`. Each constant appears in `TatCa`: use reflection over the nested classes.
  - [ ] Integration: after migration, the `Quyen` rows equal `MaQuyen.TatCa` exactly (both directions, AC 2); each role's grants equal the AC 3 table; `admin` has `QUAN_TRI`; `CapNhatQuyenAsync` writes one `Sua` row with sorted before/after lists; a stale `RowVer` conflicts; a user without `HT.Sua` gets `KhongCoQuyenException` and nothing changes.
  - [ ] Integration test of `IKiemTraQuyen`: an inactive user with the right role is refused, and a user with no role is refused.

## Dev Notes

### Current codebase state (after 2.2)

- `NguoiDung` with lockout and forced change, `DangNhapService`, the sign-out loop, `IGhiNhatKy` (new-data only).

### Design notes

- **Codes are the contract, Ids are seed keys.** Code references `MaQuyen.*` constants. The DB `Quyen.Id` matters only for `HasData`. That's why the catalogue carries explicit Ids.
- **Special-permission registry (alignment A18).** Each later story adds its entry to `MaQuyen`, its `Quyen` seed row and its default grant in its own migration. The catalogue ↔ seed test keeps them in sync.
- **Leadership gets `HT.Xem`** (alignment A22, added with this story file). FR11/HT-07 makes the audit log a leadership tool (Story 2.10's user is a unit leader), but the feature-list role table gave leadership no `HT` permission. With `HT.Xem` they can also open the HT screens read-only (unit info, signers, accounts), which is harmless because every write is re-checked.
- **Why `IKiemTraQuyen` lands here, not in 2.5:** Story 2.4's AC 5 needs a service-level check, and 2.4 comes before 2.5. The port is tiny and needs only the tables this story creates. Story 2.5 still owns the full UI side and the bypass test across all services.

### Gotchas

- `HasData` for many-to-many join rows: configure `VaiTroQuyen` as an explicit entity (`HasKey(x => new { x.VaiTroId, x.QuyenId })`) and seed it as anonymous objects. Skip-navigation seeding is awkward.
- Don't make `VaiTroQuyen` `IAuditable`. Deleted entries would hit the interceptor's "never hard-deleted" throw (Story 1.3 T6).
- The audit lists must be **sorted**. Otherwise two identical sets look different in the diff view (2.10).

### Out of scope

- Assigning roles to users (2.4). The permission-driven menu and `ICurrentUser.HasPermission` (2.5). The "own detainees only" warden scope (Epic 12).

### References

- Epic 2 › Story 2.3; `epics.md` › FR2, Backlog Alignment A18, A22
- DB design PDF: `VaiTro`, `Quyen`, `NguoiDungVaiTro`, `VaiTroQuyen` (p.4–5); seed "6 vai trò … quyền theo mã chức năng" (p.15)
- Feature list PDF: user roles and main permissions (p.2)

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2; leadership role gains `HT.Xem` (alignment A22) |
