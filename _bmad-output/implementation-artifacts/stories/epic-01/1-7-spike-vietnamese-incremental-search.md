---
story: "1.7"
epic: 1
title: "Spike: Vietnamese diacritic-insensitive incremental search"
status: done
type: spike
size: S (time-boxed, 1 day suggested)
backlogItems: [SP-03]
frsCovered: []
nfrsTouched: [NFR10]
uxDrs: [UX-DR2]
dependsOn: ["1.2"]
baseline_commit: 49b2680252cae48c9bfda4c2e59fc9edfac49f87
---

# Story 1.7: Spike: Vietnamese diacritic-insensitive incremental search

Status: done

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

- [x] **T1. Test data** (AC: 1)
  - [ ] `spikes/SP-03-Search/`: a SQL script plus a small console or WinForms app, outside the solution. Create a throw-away DB (LocalDB for the dev run, and the **real SQL Server 2022 Express on target-like hardware** for the measurement) with the default collation `Vietnamese_CI_AI`, matching Story 1.2. — LocalDB dev run **done**; the target-hardware run stays the manual step in ADR 0003 open risk 1.
  - [x] Table `DoiTuongSpike (Id int IDENTITY PK, MaSo varchar(30) UNIQUE, HoTen nvarchar(100), NamSinh smallint, BuongGiam nvarchar(50))` + `INDEX IX_HoTen (HoTen)`.
  - [x] Generate 10,000 realistic names: common surnames (Nguyễn, Trần, Lê, Phạm, Hoàng/Huỳnh, Phan, Vũ/Võ, Đặng, Bùi, Đỗ, Hồ, Ngô, Dương, Lý), middle names (Văn, Thị, Đức, Minh, Ngọc, Hữu, …) and given names. Include deliberate namesakes ("Nguyễn Văn A" ×5) and names with `Đ/đ`.
- [x] **T2. Collation behaviour** (AC: 1)
  - [x] Confirm that `WHERE HoTen LIKE N'nguyen van a%'`, `N'NGUYỄN VĂN A%'` and `N'Nguyễn Văn A%'` return identical rows.
  - [x] **Check `đ`/`Đ` against `d`.** In Vietnamese collations `Đ` is a separate letter, not "D with a diacritic". `Vietnamese_CI_AI` may **not** match "duc" to "Đức". Record the actual result. If it doesn't match, evaluate: (a) a persisted computed column `HoTenKhongDau` (diacritics stripped in the app on save, with `đ→d`) plus an index on it; (b) normalizing the search term only (`d` → `[dđ]` pattern, which breaks index seeks); (c) accepting it and training users. Recommendation if needed: (a), filled by the Application layer on save.
  - [x] Check other traps: double spaces, leading/trailing spaces, NFD-decomposed input pasted from Word (normalize the term to NFC in the app), and `%`/`_`/`[` typed by users (escape them in LIKE).
- [x] **T3. Query patterns and timing** (AC: 2)
  - [x] Compare these, with `SET STATISTICS TIME, IO ON` and the actual execution plan (seek vs scan):
    1. Name prefix: `HoTen LIKE @t + '%'`, which can seek.
    2. Name contains: `HoTen LIKE '%' + @t + '%'`, which scans. At 10k rows that may still be fast enough, so measure it.
    3. Code prefix or exact: `MaSo LIKE @t + '%'`.
    4. Given name / any word: users often know only "Tuấn". Try `HoTen LIKE '% ' + @t + '%' OR HoTen LIKE @t + '%'`.
    5. Combined: if the term looks like a code (digits or a code pattern), search `MaSo` first, otherwise the name.
  - [x] Always `SELECT TOP (50) Id, MaSo, HoTen, NamSinh, BuongGiam … ORDER BY HoTen, MaSo`. These are the columns the picker shows to tell namesakes apart (UX-DR2).
  - [ ] Run through EF Core 10 the way the product will (parameterized `EF.Functions.Like`, `AsNoTracking`, projection to a DTO) as well as raw SQL. Record both timings. Measure **end-to-end from keystroke to grid filled**, not just SQL time, on target-like hardware over the LAN. Record the machine specs. — EF + SQL + WinForms end-to-end **done on LocalDB on the dev machine** (machine specs recorded); the target-hardware/LAN leg is pending (ADR 0003 open risk 1).
- [x] **T4. Debounce and cancelling stale queries** (AC: 2, 3)
  - [x] In a small WinForms test form: a `TextBox` + `DataGridView`. On `TextChanged`, cancel the previous `CancellationTokenSource`, create a new one, `await Task.Delay(300, ct)`, then `await query.ToListAsync(ct)`. Ignore `OperationCanceledException` and bind only if the token is still current.
  - [x] Confirm that cancelling actually cancels the SQL command (EF passes the token to `SqlCommand`, so check with SQL Profiler / Extended Events or by timing). Make sure fast typists don't pile up queries.
  - [x] Each search uses a **new DI scope / DbContext** (CLAUDE.md: Forms never hold a `DbContext`). Note whether the per-keystroke scope cost is measurable.
  - [x] Minimum term length: decide whether 1 character is useful or should wait for 2. Recommendation: search codes from 1 character and names from 2.
- [x] **T5. Outcome note** (AC: 3)
  - [x] `docs/decisions/0003-vietnamese-incremental-search.md`: collation findings (including `đ`), chosen query pattern(s), index definitions (`DoiTuong (HoTen)` from the DB design, plus anything added, e.g. `INCLUDE (MaSo, NamSinh, BuongGiam)` to avoid lookups), the timing table, the debounce and cancellation pattern as a code snippet, and the minimum term length. Mark which items Story 3.2 (detainee picker, UX-DR2) must adopt.

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

deepseek-v4.1-flash (opencode)

### Debug Log References

- The spike source from an earlier interrupted run was missing from the working tree (only
  gitignored `artifacts/` and `obj/`/`bin/` output remained); the project was rebuilt from the
  story. `MSSQLLocalDB` (SQL Server 2025 LocalDB 17.0.4025.3, "Express Edition") was the server
  for all runs; `--connection` exists for the real SQL Express/LAN measurement.
- First rebuilt run: `ef-auto-name-accented` returned 0 rows — the Auto path searched the stored
  ASCII column but did not fold the typed term. Fixed in `SearchService.SearchAsync`
  (`RemoveDiacritics(NormalizeForSearch(rawTerm))`); after that ASCII, accented and NFD terms all
  return the same rows.
- Cancellation probe: `COUNT(*)` over the table cross join is rewritten by the optimizer
  (4 ms raw), so it could not be cancelled. The LINQ probe now runs
  `SelectMany(...).SumAsync(Math.Sqrt(...))` (~27 s raw when run uncancelled) and both probes show
  the request ending in `sys.dm_exec_requests` with `OperationCanceledException` / `SqlException 0`.
- EF parameter typing captured with `ToQueryString()`: `DECLARE @pattern varchar(30)` — EF derives
  varchar from the mapped `varchar(30)` column, so the product code search avoids the implicit
  conversion (raw nvarchar comparison scans: 14 ms at 50k vs 0.3 ms varchar; near-exact varchar
  comparison seeks `UQ_MaSo`).
- UI check: the first scenario's keystroke → grid is ~807 ms because EF compiles the first query;
  steady state is ~330–410 ms (300 ms debounce + query + bind). Warm-up at load is mandatory for
  Story 3.2.

### Completion Notes List

- **AC1.** Tone marks fold under `Vietnamese_CI_AI` (`'%tuyet%'` = `'%Tuyết%'`), but `ă â đ ê ô
  ơ ư` are separate letters: ASCII `'nguyen van a%'` on `HoTen` = 0 rows. Option (a) — persisted
  `HoTenKhongDau` filled by the app (`đ→d`) with a covering index — returns identical ids for
  ASCII, accented and NFD terms (6/6 at 10k, 8/8 at 50k). NFD input from Word does not match NFC
  data and must be normalized; user-typed `% _ [` are escaped with `ESCAPE '\'`.
- **AC2.** Raw SQL and EF Core 10 medians are all ≤ ~80 ms at 50k (prefix seek 0.3 ms, word start
  62 ms, contains 76 ms); the 300 ms budget has large headroom. End-to-end in the real WinForms
  form: one visible bind per scenario (11 stale queries cancelled per fast-typed term), steady
  ~330–410 ms including the debounce.
- **AC3.** ADR `0003-vietnamese-incremental-search.md` records collation findings (including `đ`),
  the query cascade, index DDL (`IX_DoiTuong_HoTenKhongDau` covering), both timing tables, the
  debounce/cancellation snippet, minimum term lengths (names 2 — a correctness rule because of
  digraph contractions; codes 1) and the checklist Story 3.2 must adopt.
- **Cancellation is real:** the EF token ends the server command (watched via
  `sys.dm_exec_requests`); per-keystroke fresh-`DbContext` cost is ~0.8 ms median, so the
  "fresh DI scope per operation" rule is not a bottleneck.
- **Gap (recorded in the ADR, open risk 1):** all numbers are LocalDB 17 on the dev machine
  (i5-11400H, 12 logical CPUs), not the unit's workstations over the LAN. Re-run
  `--benchmark`/`--ui-check` with `--connection` on target hardware before NFR10 is signed off.
  The first-query EF compilation (~0.6–1.1 s) is also flagged for warm-up in Story 3.2.
- Evidence committed under `docs/decisions/0003/assets/` (benchmark at 10k and 50k, UI check);
  live output stays in the gitignored `spikes/SP-03-Search/artifacts/`.

### File List

- `spikes/SP-03-Search/SP-03-Search.csproj` (new)
- `spikes/SP-03-Search/README.md` (new)
- `spikes/SP-03-Search/Program.cs` (new)
- `spikes/SP-03-Search/MainForm.cs` (new)
- `spikes/SP-03-Search/SpikeOptions.cs` (new)
- `spikes/SP-03-Search/ParentConsole.cs` (new)
- `spikes/SP-03-Search/Data/Sql/create-spike-db.sql` (new)
- `spikes/SP-03-Search/Data/DoiTuongSpike.cs`, `SpikeDatabase.cs`, `SpikeDbContext.cs`, `SpikeDbContextFactory.cs`, `SqlScriptRunner.cs`, `VietnameseNames.cs`, `VietnameseText.cs` (new)
- `spikes/SP-03-Search/Search/SearchService.cs`, `SearchResult.cs`, `SearchStrategy.cs`, `IncrementalSearch.cs` (new)
- `spikes/SP-03-Search/Benchmark/BenchmarkRunner.cs`, `RawQueryProbe.cs`, `CancellationProbe.cs`, `Timing.cs` (new)
- `docs/decisions/0003-vietnamese-incremental-search.md` (new)
- `docs/decisions/0003/assets/benchmark-10000.txt`, `benchmark-50000.txt`, `ui-check-10000.txt` (new)
- `_bmad-output/implementation-artifacts/stories/epic-01/1-7-spike-vietnamese-incremental-search.md` (task checkboxes, Dev Agent Record, Change Log)

## Change Log

| Date | Change |
|---|---|
| 2026-10-01 | Story file created from Epic 1 |
| 2026-10-02 | SP-03 executed: spike project under `spikes/SP-03-Search`, LocalDB measurements at 10k/50k, cancellation probe, WinForms UI check, ADR 0003 + committed evidence. Status → review (deepseek-v4.1-flash) |
| 2026-10-02 | Review fixes applied: `Check` gates on the EF/debounce benchmark sections, ui-check fails a never-bound scenario, debounce term guard, option-value guard, ADR corrections (warm-up, `WaitForIdleAsync`, `\` escaping) and timing tables refreshed to the re-recorded evidence; T1/T3 target-hardware markers unchecked (deepseek-v4.1-flash) |

## Review Triage Log

| # | Finding | Verdict | Evidence / Resolution |
|---|---|---|---|
| 1 | Story status bookkeeping disagrees with itself (frontmatter/body/change log; ADR "Proposed" vs "Adopt") | false | The frontmatter is the workflow-managed state and the body `Status:` line is left stale in the earlier story files too (1.6 body says `ready-for-dev` with frontmatter `review`); "Proposed" is the normal ADR lifecycle before the implementing story adopts it. No reader is misled by a requirement. |
| 2 | ADR never quotes NFR10; "300 ms debounce" vs "300 ms budget" ambiguous | false | Story AC2 defines the 300 ms as per-query latency, and the ADR separates the query tables (§5) from the end-to-end debounce table (§6), so the two are not conflated. |
| 3 | `Timing.P95` with 15 iterations equals the sample maximum | false | Nearest-rank p95 of n=15 is by definition the 15th sample; the report states iterations=15. Not a mislabel, just an unrobust estimate, which is acceptable for a spike's pass/fail margin (≤80 ms vs 300 ms). |
| 4 | `--benchmark` EF/debounce sections never assert; a `RemoveDiacritics` regression still exits 0 | medium — patch | Verified: every `Check(...)` lives in `CollationChecksAsync`; `EfSectionAsync`/`DebounceSectionAsync` only write rows. Deleting the term fold at `SearchService.cs:26` (the exact regression in the Dev Notes) passes the gate. Patched with assertions. |
| 5 | `--ui-check` validates neither rows nor elapsed time; a never-bound search is written as a normal row | medium — patch | Verified: `RunScenarioAsync` turns a 10 s no-bind wait into `ElapsedMs = -1` and appends a normal row; `Failures` only increments in the outer catch (`MainForm.cs:192-196`). Patched: a scenario fails when `Completed != 1`, `Rows < 0` or `ElapsedMs < 0`. |
| 6 | `--ui-check` returns 0 although the search path or debounce broke | medium — patch | Same defect as #5 (verification-gap layer's independent statement); covered by the same patch, which propagates to the process exit code. |
| 7 | Warm-up mitigation unvalidated (only `context.Model` touched; first keystroke still ~807 ms) | low — patch | Verified: `SpikeDbContextFactory.WarmUp` only reads `Model`. ADR corrected to state the spike did not validate a warm-up query and Story 3.2 must verify the recipe (open risk 2 already flags the cost). |
| 8 | `HoTenKhongDau` nullable/unenforced in the spike schema | false | The spike column is nullable on purpose (option-a candidate); the ADR's Story 3.2 checklist already mandates `NOT NULL` and backfill. The product schema does not exist yet. |
| 9 | `--rows` below fixture index 122 silently misses namesakes | low — reject | Fixture `Assign` skips out-of-range indices, but a missing fixture makes the hard-coded `Check`s fail loudly (exit non-zero); the documented default is 10,000 and tiny row counts are debug-only. |
| 10 | Cascade short-circuits on the first non-empty stage | false | Prefix → word-start → contains is the documented design; a fallback only runs when the previous stage returned nothing. Story 3.2's exact-code-first ranking sits on top. |
| 11 | Concurrency across LAN workstations not measured/risk-listed | low — reject | Real gap, but open risk 1 already bounds all numbers to LocalDB on the dev machine; measuring concurrent keystrokes is a product-side task with no bearing on the spike's LocalDB conclusions, and the fix is not a direct correction. |
| 12 | Reproducibility metadata incomplete (compat level, LocalDB 17 vs SQL Server 2022) | false | ADR records version/edition/collation, machine specs and the target server (SQL Server 2022 Express) as the required rerun; the simple index seek/scan predicates do not hinge on the compat level. |
| 13 | `RawQueryProbe` reads scraped from English INFO text with a fixed 100 ms wait; plan parser only lists seek/scan | low — reject | Code facts confirmed (IO message regex, `Task.Delay(100)`, `DescribePlan` seeks/scans only), but the committed runs were coherent and reproducible on this machine; reworking IO capture (plan-XML attributes) is more than a direct correction and the tool is throwaway. |
| 14 | ADR says `WaitForIdleAsync()` is what the UI check uses (nothing calls it) | low — patch | Verified: the only references are its definition; the drivers use their own `Bound` event + `TaskCompletionSource`. ADR sentence corrected. |
| 15 | Zero/one-character name UX not specified for the picker | false | The ADR records the correctness rule (names from 2, codes from 1); what the picker displays at 0–1 characters is Story 3.2 / UX-DR2's job, outside a search-mechanics spike. |
| 16 | Code-like Auto term with no code match never falls back to names | false | Vietnamese names cannot contain digits, so a term with a digit cannot be a valid name prefix; the dead end is the intended behavior. |
| 17 | ADR checklist omits escaping `\` though the tested helper escapes it | low — patch | Verified: `EscapeLike` escapes `\`, `%`, `_`, `[`; the ADR lists only `% _ [`. A 3.2 dev following it literally could mis-escape a typed backslash. Checklist corrected. |
| 18 | Authorization/audit/PII absent from the 3.2 checklist | false | Out of the spike's intent (AC3 asks for query pattern, indexes, cancellation); permissions/logging belong to the product stories (CLAUDE.md applies there). |
| 19 | Helpers live only in the throwaway spike with no unit tests | false | Explicit spike scope: the deliverable is the decision note; Story 3.2 builds and tests the real picker in the product. |
| 20 | Scale conclusion extrapolated from two points | false | The ADR already says the curve is roughly linear and that contains is the first path to approach the budget; no unsupported claim was made. |
| 21 | Cancellation counters conflate debounce-cancel and lost-version-race | false | Counters count "cancelled", which is accurate for both; SQL-side cancellation is proven separately by the `sys.dm_exec_requests` probe. |
| 22 | Evidence `[PASS]` labels conflate confirmed limitations with AC passes | false | Each `Check` name states the asserted proposition (e.g. "collation alone ... does NOT match") and the detail explains it; adjacent lines confirm option (a). No reader is misled. |
| 23 | Interrupted `--setup` (DB exists, table missing) aborts benchmark | low — reject | `--setup` is documented as destructive and re-runnable; recovering by rerunning it is the obvious step. Fix adds a branch for a debug-only interruption. |
| 24 | `RawQueryProbe` logical-reads timing window misattribution | low — reject | Same root cause as #13; no wrong number observed in the committed evidence. |
| 25 | `IncrementalSearch` stale-bind race outside the UI thread | low — reject | The version check plus `ThrowIfCancellationRequested` closes all but a single-instruction window; the product pattern runs on the UI thread with no interleaving, and the headless runs never showed it. Fix adds a lock. |
| 26 | `BenchmarkRunner.DebounceSectionAsync` binds an intermediate prefix result | low — patch | Verified: unlike `MainForm`'s driver, the benchmark's `ResultsBound` ignores the bound term (`ResultsBound = (_, rows) => …`), so a prefix that wins a typing stall sets `boundRows`/`finished` early. Patched with the term-equality guard. |
| 27 | Valueless option consumes the next flag (`--out --benchmark`) | low — patch | Verified: `SpikeOptions.Next` accepts any following token. Patched: reject a following `--option` as a missing value. |
| 28 | T1/T3 marked done although the target-hardware leg was not performed | low — patch | Verified against the Dev Agent Record: all runs are LocalDB on the dev machine. Task markers corrected to unchecked with the pending manual step named (matches ADR 0003 open risk 1). |
| 29 | T3 says `SET STATISTICS TIME, IO ON` but only IO/XML are set and timings are client wall clock | false | The AC asks for query latency; client wall clock is the right measure for the user-visible number and the actual plan was captured via XML. The literal `STATISTICS TIME` omission does not change the measured quantity. |
