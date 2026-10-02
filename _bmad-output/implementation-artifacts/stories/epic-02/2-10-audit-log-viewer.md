---
story: "2.10"
epic: 2
title: Audit-log viewer
status: ready-for-dev
size: M
backlogItems: [HT-07]
frsCovered: [FR11]
nfrsTouched: [NFR5, NFR10]
dependsOn: ["1.3", "2.5"]
---

# Story 2.10: Audit-log viewer

Status: ready-for-dev

## Story

As a unit leader,
I want to look up who added, changed, cancelled, printed or approved what, and when, with the old and new values,
So that I can investigate any discrepancy.

## Acceptance Criteria

1. **Given** a user with `HT.Xem`
   **When** they open the audit log
   **Then** they can filter by date range (default today), user, workstation, action (`Them, Sua, Huy, In, Duyet, DangNhap`), table and record id, and results are paged (newest first)

2. **Given** a selected audit row
   **When** the user opens its detail
   **Then** a side-by-side view shows each changed field with old → new values parsed from `DuLieuCu` / `DuLieuMoi`

3. **Given** the audit-log screen
   **When** it is used
   **Then** it offers no edit or delete action anywhere, and the service exposes only read methods

4. **Given** 1 million audit rows
   **When** filtering by a one-day range
   **Then** results appear in under 2 seconds (an index on `ThoiDiem` plus `TenBang, BanGhiId` is added in this story's migration)

## Tasks / Subtasks

- [ ] **T1. Query (SQL, not LINQ)** (AC: 1, 4)
  - [ ] Application `HeThong/NhatKyThaoTacQuery` (interface in Application, implemented in Infrastructure with EF `SqlQuery` or Dapper, per CLAUDE.md "complex reports as SQL"): `TimAsync(LocNhatKy loc, int trang, int coTrang, CancellationToken ct)` returns `(IReadOnlyList<DongNhatKy> Dong, int TongSo)`; `LayChiTietAsync(long id)` returns the row including both JSON columns.
  - [ ] `LocNhatKy`: `TuNgay`, `DenNgay` (`DateOnly`, inclusive; translate to `ThoiDiem >= @tu AND ThoiDiem < @den + 1 day` so the index is used), optional `NguoiDungId`, `MayTram`, `HanhDong`, `TenBang`, `BanGhiId`.
  - [ ] List columns: `ThoiDiem`, user (`TenDangNhap` + staff `HoTen` via `LEFT JOIN NguoiDung`/`CanBo`; there's no FK by design, Story 1.3, so a missing user shows its id), `MayTram`, `HanhDong`, `TenBang`, `BanGhiId`, and a short summary (the explicit event's `SuKien` for `DangNhap` rows, otherwise the changed field names). Don't select the JSON columns in the list; load them only for the detail.
  - [ ] `ORDER BY ThoiDiem DESC, Id DESC` + `OFFSET/FETCH`, page size 100. Return the total count for the pager (a `COUNT(*)` over a one-day range is cheap).
  - [ ] Validate the range: `TuNgay <= DenNgay`, at most 366 days ("Khoảng thời gian tối đa một năm"), so nobody can accidentally scan the whole table.
  - [ ] Filter lists for the screen: accounts (including inactive), distinct `TenBang` and distinct `MayTram` values.
- [ ] **T2. Indexes** (AC: 4)
  - [ ] Story 1.3 already created `IX_NhatKyThaoTac_ThoiDiem` and `IX_NhatKyThaoTac_TenBang_BanGhiId` (`NhatKyThaoTacConfiguration`). **Don't duplicate them.** Add a test that reads `sys.indexes` and asserts both exist, to satisfy the AC's index clause.
  - [ ] Run the T4 performance test first. Only if a common filter combination is slow (e.g. user + month), add one index in a migration `AddNhatKyThaoTacIndexes`, likely `(NguoiDungId, ThoiDiem)`, and record the measurement in the Completion Notes. No speculative indexes: every index slows inserts on an append-only hot table.
- [ ] **T3. Detail view: old → new** (AC: 2)
  - [ ] Application `HeThong/SoSanhNhatKy.cs` (pure, unit-tested): parse `DuLieuCu`/`DuLieuMoi` with `System.Text.Json` (`JsonDocument`) into `IReadOnlyList<(string Truong, string? GiaTriCu, string? GiaTriMoi)>` over the **union** of property names:
    - `Them`: old is empty.
    - `Sua`/`Huy`: both sides (the interceptor stores only changed columns).
    - Explicit events (`DangNhap`, `In`, `Duyet`): show the payload as fields.
    - Nested arrays (role lists from 2.3/2.4, signer lists from 2.9): show them compactly, one item per line, and for a list of strings mark added/removed items (`+ LK-C.Duyet`, `− DM.Sua`).
  - [ ] Malformed JSON never crashes the screen: show the raw text with a note.
  - [ ] Format values for reading: `true/false` → "Có/Không"; ISO dates → `dd/MM/yyyy HH:mm`; leave numbers and enum codes as stored. Field names stay as column names, which is exactly what investigators need to match against the DB.
- [ ] **T4. Performance test** (AC: 4)
  - [ ] `IntegrationTests/HeThong/NhatKyThaoTacPerformanceTests.cs`, `[SqlServerFact]` + `[Trait("Category", "Slow")]`: bulk-insert 1,000,000 rows spread over 365 days (one set-based `INSERT … SELECT` from a numbers CTE, not EF; use `SqlBulkCopy` only if that's too slow), with realistic `TenBang`/`HanhDong` mixes and ~1 KB JSON on 10% of rows. Then run `TimAsync` for one day (first page + count) and assert < 2 s, measured with `Stopwatch` after one warm-up call. Record the timing.
  - [ ] Bypass `AuditInterceptor` for the seed (raw SQL), which is fine in a test. Use a throw-away DB from the fixture.
  - [ ] CI runs on LocalDB. If the test is too slow for CI, keep it in the suite but filter `Category!=Slow` in CI, and say so in the Completion Notes. AC 4 is then verified locally against the Docker SQL Server.
- [ ] **T5. WinForms screen** (AC: 1, 2, 3)
  - [ ] `HeThong/`: `INhatKyThaoTacView` + presenter + form. A filter bar (date range with default today–today, user combo, workstation combo, action combo with Vietnamese labels "Thêm/Sửa/Huỷ/In/Duyệt/Đăng nhập", table combo, record id box, Tìm button), a read-only grid with pager (Trang x/y, Trước, Sau), and a detail panel below or on double-click, with a 3-column grid (Trường, Giá trị cũ, Giá trị mới) where changed values are highlighted.
  - [ ] **No** edit, delete or "clear log" control anywhere, and the grid is `ReadOnly = true`. Right-click export is not in the AC; leave it to UX-DR3 (the shared voucher grid, Epic 6).
  - [ ] Register "Hệ thống › Nhật ký thao tác" (`HT.Xem`) in the menu registry.
- [ ] **T6. Read-only guarantee** (AC: 3)
  - [ ] Architecture test: the query interface has only `Lay*`/`Tim*` methods, returning data, and Infrastructure's implementation never calls `SaveChanges`, `Add`, `Update` or `Remove`. A source scan of that one file is enough. Story 1.3's interceptor already blocks EF updates and deletes of `NhatKyThaoTac`. Epic 7 adds the DB-level `DENY UPDATE, DELETE`.
- [ ] **T7. Tests** (AC: 1–3)
  - [ ] Unit: `SoSanhNhatKy` for `Them`, `Sua`, `Huy`, an explicit `DangNhap` payload, a role-list diff, malformed JSON, and Vietnamese text preserved (the interceptor writes with `UnsafeRelaxedJsonEscaping`).
  - [ ] Integration: each filter alone and combined; date-range edges (a row at 23:59:59 is included, the next day's 00:00:00 isn't); newest first; paging totals; a user without `HT.Xem` can't open it (menu hidden, 2.5). The query itself is read-only and needs no write check.
  - [ ] Presenter: default range is today (from `IClock`); an invalid range shows the validation message.

## Dev Notes

### Current codebase state (after 2.5, ideally after 2.9)

- `NhatKyThaoTac` + both indexes (1.3); events from sign-in, lockout, sign-out, lock/unlock (2.2, 2.7); role and account audit rows with before/after lists (2.3, 2.4); signer lists (2.9). Build this story after the others if possible, so the diff view can be tested against every payload shape the epic produces.

### Design notes

- **Who sees it:** `HT.Xem`, granted to *Quản trị hệ thống* and, per alignment A22 (Story 2.3), to *Chỉ huy phụ trách / Lãnh đạo đơn vị*, the story's user.
- **The log isn't sorted by `Id` alone.** `ThoiDiem` comes from each workstation's `IClock`, and clocks can drift. Sort by `ThoiDiem DESC, Id DESC` and filter by `ThoiDiem`, the business meaning of "when". Note the drift in the Completion Notes as an operations point (NTP on the LAN, Epic 7).
- `HanhDong` is stored as text (Story 1.3), so filter with the enum name as a string parameter.

### Gotchas

- `SqlQuery<T>` composed with `Skip/Take` wraps the SQL in a sub-select. Fine for a `SELECT`, but write `OFFSET/FETCH` explicitly in the SQL to keep the plan predictable, and materialize with `ToListAsync()`.
- `DateOnly` parameters → convert to `DateTime` at 00:00 local before binding. `ThoiDiem` is `datetime2(0)` local time.
- 1M rows with JSON make a ~1 GB test DB. Make sure the fixture drops it, and don't run it in parallel with other slow tests.

### Out of scope

- Export to Excel/PDF (UX-DR3/FR46, Epic 6). Hash-chain tamper detection (Epic 14). The cancelled-documents report (Epic 6, FR40).

### References

- Epic 2 › Story 2.10; `epics.md` › FR11, NFR5, Backlog Alignment A22
- DB design PDF: `NhatKyThaoTac` (p.6)
- Story 1.3 (interceptor, JSON format, existing indexes) and its Completion Notes
- `CLAUDE.md`: complex reports as SQL; the audit log is append-only

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-02 | Story file created from Epic 2 |
