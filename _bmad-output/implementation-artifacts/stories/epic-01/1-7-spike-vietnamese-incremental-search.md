---
story: "1.7"
epic: 1
title: "Spike: Vietnamese diacritic-insensitive incremental search"
status: ready-for-dev
type: spike
size: S (time-boxed, 1 day suggested)
backlogItems: [SP-03]
frsCovered: []
nfrsTouched: [NFR10]
uxDrs: [UX-DR2]
dependsOn: ["1.2"]
---

# Story 1.7: Spike: Vietnamese diacritic-insensitive incremental search

Status: ready-for-dev

## Story

As a custodial officer,
I want to type part of a name without diacritics and see matches immediately,
So that I can find a detainee among thousands in a second or two.

## Acceptance Criteria

1. **Given** a test table with 10,000 Vietnamese names under collation `Vietnamese_CI_AI` and an index on the name
   **When** searching "nguyen van a" or "NGUYỄN VĂN A"
   **Then** both return the same rows as "Nguyễn Văn A"

2. **Given** incremental search (results refresh while typing, with a ~300 ms debounce)
   **When** the user types successive characters
   **Then** each query returns the top 50 matches in under 300 ms on target hardware (the measurement is recorded)

3. **Given** the spike result
   **When** it is reviewed
   **Then** a note records the chosen query pattern (prefix vs contains, code vs name), the index definitions, and how stale queries are cancelled, for use in Story 3.2

## Tasks / Subtasks

- [ ] **T1. Test data** (AC: 1)
  - [ ] `spikes/SP-03-Search/`: a SQL script plus a small console or WinForms app, outside the solution. Create a throw-away DB (LocalDB for the dev run, and the **real SQL Server 2022 Express on target-like hardware** for the measurement) with the default collation `Vietnamese_CI_AI`, matching Story 1.2.
  - [ ] Table `DoiTuongSpike (Id int IDENTITY PK, MaSo varchar(30) UNIQUE, HoTen nvarchar(100), NamSinh smallint, BuongGiam nvarchar(50))` + `INDEX IX_HoTen (HoTen)`.
  - [ ] Generate 10,000 realistic names: common surnames (Nguyễn, Trần, Lê, Phạm, Hoàng/Huỳnh, Phan, Vũ/Võ, Đặng, Bùi, Đỗ, Hồ, Ngô, Dương, Lý), middle names (Văn, Thị, Đức, Minh, Ngọc, Hữu, …) and given names. Include deliberate namesakes ("Nguyễn Văn A" ×5) and names with `Đ/đ`.
- [ ] **T2. Collation behaviour** (AC: 1)
  - [ ] Confirm that `WHERE HoTen LIKE N'nguyen van a%'`, `N'NGUYỄN VĂN A%'` and `N'Nguyễn Văn A%'` return identical rows.
  - [ ] **Check `đ`/`Đ` against `d`.** In Vietnamese collations `Đ` is a separate letter, not "D with a diacritic". `Vietnamese_CI_AI` may **not** match "duc" to "Đức". Record the actual result. If it doesn't match, evaluate: (a) a persisted computed column `HoTenKhongDau` (diacritics stripped in the app on save, with `đ→d`) plus an index on it; (b) normalizing the search term only (`d` → `[dđ]` pattern, which breaks index seeks); (c) accepting it and training users. Recommendation if needed: (a), filled by the Application layer on save.
  - [ ] Check other traps: double spaces, leading/trailing spaces, NFD-decomposed input pasted from Word (normalize the term to NFC in the app), and `%`/`_`/`[` typed by users (escape them in LIKE).
- [ ] **T3. Query patterns and timing** (AC: 2)
  - [ ] Compare these, with `SET STATISTICS TIME, IO ON` and the actual execution plan (seek vs scan):
    1. Name prefix: `HoTen LIKE @t + '%'`, which can seek.
    2. Name contains: `HoTen LIKE '%' + @t + '%'`, which scans. At 10k rows that may still be fast enough, so measure it.
    3. Code prefix or exact: `MaSo LIKE @t + '%'`.
    4. Given name / any word: users often know only "Tuấn". Try `HoTen LIKE '% ' + @t + '%' OR HoTen LIKE @t + '%'`.
    5. Combined: if the term looks like a code (digits or a code pattern), search `MaSo` first, otherwise the name.
  - [ ] Always `SELECT TOP (50) Id, MaSo, HoTen, NamSinh, BuongGiam … ORDER BY HoTen, MaSo`. These are the columns the picker shows to tell namesakes apart (UX-DR2).
  - [ ] Run through EF Core 10 the way the product will (parameterized `EF.Functions.Like`, `AsNoTracking`, projection to a DTO) as well as raw SQL. Record both timings. Measure **end-to-end from keystroke to grid filled**, not just SQL time, on target-like hardware over the LAN. Record the machine specs.
- [ ] **T4. Debounce and cancelling stale queries** (AC: 2, 3)
  - [ ] In a small WinForms test form: a `TextBox` + `DataGridView`. On `TextChanged`, cancel the previous `CancellationTokenSource`, create a new one, `await Task.Delay(300, ct)`, then `await query.ToListAsync(ct)`. Ignore `OperationCanceledException` and bind only if the token is still current.
  - [ ] Confirm that cancelling actually cancels the SQL command (EF passes the token to `SqlCommand`, so check with SQL Profiler / Extended Events or by timing). Make sure fast typists don't pile up queries.
  - [ ] Each search uses a **new DI scope / DbContext** (CLAUDE.md: Forms never hold a `DbContext`). Note whether the per-keystroke scope cost is measurable.
  - [ ] Minimum term length: decide whether 1 character is useful or should wait for 2. Recommendation: search codes from 1 character and names from 2.
- [ ] **T5. Outcome note** (AC: 3)
  - [ ] `docs/decisions/0003-vietnamese-incremental-search.md`: collation findings (including `đ`), chosen query pattern(s), index definitions (`DoiTuong (HoTen)` from the DB design, plus anything added, e.g. `INCLUDE (MaSo, NamSinh, BuongGiam)` to avoid lookups), the timing table, the debounce and cancellation pattern as a code snippet, and the minimum term length. Mark which items Story 3.2 (detainee picker, UX-DR2) must adopt.

## Dev Notes

### Why a spike, and what "done" means

- Time-boxed research (A10). **The deliverable is the decision note.** Spike code lives in `spikes/`, outside `LuuKyCanTin.slnx`, so the product source stays clean. Story 3.2 builds the real picker in the product.

### Context

- The DB design already defines `DoiTuong.HoTen nvarchar(100) NOT NULL, index` and the collation `Vietnamese_CI_AI` "so names can be searched without diacritics". This spike validates that assumption before Epic 3 depends on it.
- The same pattern applies later to goods (`HangHoa (TenHang)`, POS search in Epic 10) and to global search (Ctrl+K, Epic 13). Keep the note generic enough to reuse.
- Target scale: "thousands of detainees" (NFR10). Measure with 10,000 rows and note how the curve looks at 50,000 too, since history accumulates (released detainees stay in the table).

### Depends on 1.2 because

- The spike must use the same collation setup as the real migration (`UseCollation("Vietnamese_CI_AI")` at DB level). Create the spike DB the same way, or simply run Story 1.2's migrations and add the spike table on top.

### References

- Epic 1 › Story 1.7; `epics.md` › NFR10, UX-DR2, FR14
- DB design PDF: conventions (collation), `DoiTuong` (p.6–7), Indexes (p.14–15)
- Story 3.2 (detainee picker) consumes this outcome

## Dev Agent Record

### Agent Model Used

### Debug Log References

### Completion Notes List

### File List

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
