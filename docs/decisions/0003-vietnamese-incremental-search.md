# ADR 0003: Vietnamese diacritic-insensitive incremental search

- **Date:** 2026-10-02 (spike SP-03 / Story 1.7 executed on this date)
- **Status:** Proposed — spike validated; [Story 3.2](../../_bmad-output/planning-artifacts/epics/epic-03-detainee-register.md)
  (detainee picker, UX-DR2) adopts the pattern; the product tables are introduced there and in
  Story 3.1
- **References:** Story 1.7 (SP-03), Story 3.2 (detainee picker), Story 1.2 (database collation),
  `epics.md` › NFR10, UX-DR2, FR14; DB design PDF (conventions/collation, `DoiTuong` p.6–7,
  Indexes p.14–15)
- **Evidence:** [`benchmark-10000.txt`](0003/assets/benchmark-10000.txt) ·
  [`benchmark-50000.txt`](0003/assets/benchmark-50000.txt) ·
  [`ui-check-10000.txt`](0003/assets/ui-check-10000.txt). Spike code in `spikes/SP-03-Search`
  (not part of `LuuKyCanTin.slnx`).

## Context

The DB design sets every database to `Vietnamese_CI_AI` "so names can be searched without
diacritics", and Story 3.2's picker must find a detainee by code or name as the user types,
top 50, ~300 ms debounce (NFR10, UX-DR2, FR14). The spike seeded 10,000 and 50,000 realistic
Vietnamese names (including five namesakes "Nguyễn Văn A" and five "Đỗ Đức Đạt") into a
throw-away database created with `CREATE DATABASE ... COLLATE Vietnamese_CI_AI`, mirroring the
Story 1.2 migration. All measurements below are real output of the spike on the dev machine:
SQL Server 2025 LocalDB (`17.0.4025.3`, Express Edition) on an 11th Gen Intel Core i5-11400H,
12 logical CPUs — **not** the unit's workstations and not over the LAN (see Open risks).

## Findings

### 1. The collation alone does not satisfy AC1

`Vietnamese_CI_AI` folds **tone marks** but treats `ă â đ ê ô ơ ư` as **separate letters**, not
accented ASCII. Measured:

| Check | Result |
|---|---|
| `N'ễ' = N'e'`, `N'á' = N'a'`, `N'ã' = N'a'`… | true — tone marks fold |
| `N'ă' = N'a'`, `N'đ' = N'd'`, `N'ê' = N'e'`, `N'ư' = N'u'`… | false — distinct letters |
| ASCII `HoTen LIKE N'nguyen van a%'` on accented data | 0 rows (the same query accented: 6) |
| `HoTen LIKE N'%tuyet%'` = `N'%Tuyết%'` | equal (222 rows at 10k) — tone marks fold in LIKE too |
| `HoTen = N'Nguyễn Văn A'` | 5/5 fixture namesakes found exactly |

`'Văn'` contains `ă`, so `'nguyen van a'` can never match `'Nguyễn Văn A'` through this collation.
`đ`/`Đ` are the same story (`'do duc dat'` ≠ `'Đỗ Đức Đạt'`). `ă`/`đ` are the common case in
Vietnamese names, not an edge. **A stored diacritic-stripped column is required.**

### 2. What else the spike found

- **NFD from Word does not match NFC data.** SQL Server never normalizes Unicode; a raw NFD
  `'Nguyễn Văn A'` pasted from Word matched **0** rows, while the same string normalized to NFC
  matched 5. The app must normalize the term to **NFC** (then trim/collapse whitespace) before
  building the LIKE.
- **Digraph contractions are a correctness rule for the minimum term length.** In the Vietnamese
  collation `ng nh ch th ph kh gi qu tr` behave as single letters: `N'Nguyễn' LIKE N'n%'` = 0,
  `LIKE N'ng%'` = 1; table-wide `HoTen LIKE N'n%'` = 0 while `LIKE N'ng%'` matched 1,241 rows at
  10k. **Names must therefore wait for 2 characters**; a 1-char prefix silently returns nothing.
  Codes (`DT…`) are ASCII and measured fine from 1 character.
- **User-typed wildcards must be escaped.** With `ESCAPE '\'`, a typed `%` and `_` matched 0 rows,
  `[` was literal, and the unescaped `_` matched all 10,000 rows. `EF.Functions.Like(x, pattern,
  "\\")` supports the escape character.
- **Per-keystroke scope cost is not measurable.** A fresh `DbContext` + pooled open + `SELECT 1`
  cost a median of 0.7–0.9 ms (p95 ~14 ms) — the product's "fresh DI scope per operation"
  (CLAUDE.md) is fine here.

### 3. Chosen query pattern

Two-layer pattern (option (a) in the story):

1. **Store** `DoiTuong.HoTenKhongDau nvarchar(100)`, filled by the Application layer on every
   create/update with `RemoveDiacritics(NormalizeForSearch(hoTen))` (NFC → strip combining marks →
   `đ→d`, `Đ→D`). Never computed in SQL: SQL Server has no built-in diacritic removal, and a CLR
   function would require `clr enabled`.
2. **Search** folds the typed term the same way, escapes LIKE wildcards, and queries the stored
   column. This is what makes `"nguyen van a"`, `"NGUYỄN VĂN A"` and `"Nguyễn Văn A"` return the
   same rows (6 at 10k, 8 at 50k).
3. **Cascade** (Auto): a term of 0–6 letters followed by a digit is treated as a code and searches
   `MaSo` prefix (minimum 1 char); anything else searches the name:
   - prefix seek: `HoTenKhongDau LIKE @t + '%'` — handles "nguyen van a", first-word typing;
   - word start (only if prefix returned nothing): `LIKE @t + '%' OR LIKE '% ' + @t + '%'` — finds
     a given name such as "Tuấn" in the middle;
   - contains (last resort): `LIKE '%' + @t + '%'`.
   All `ORDER BY HoTen, MaSo`, `TOP (50)`, projected to the picker DTO, `AsNoTracking`.
   Story 3.2 adds its required ranking (exact `MaSo` match first) on top.

### 4. Index definitions

`IX_HoTen (HoTen)` from the DB design is kept, but the search index is on the stripped column and
**must be covering**, because the picker projects `HoTen, MaSo, NamSinh, BuongGiam` (Story 3.2
also needs the type/status labels):

```sql
CREATE INDEX IX_DoiTuong_HoTenKhongDau
    ON dbo.DoiTuong (HoTenKhongDau)
    INCLUDE (HoTen, MaSo, NamSinh, BuongGiam);  -- + Story 3.2 label/filter columns
```

Measured difference at 10k / 50k for `HoTenKhongDau LIKE 'nguyen%'`:

| index | median ms | logical reads | plan |
|---|---|---|---|
| `(HoTenKhongDau)` key only | 3.5 / 14.1 | 126 / 619 | Clustered Index Scan |
| `(HoTenKhongDau) INCLUDE (HoTen, MaSo, NamSinh, BuongGiam)` | **1.2 / 2.9** | **12 / 45** | Index Seek |

### 5. Timing (median / p95, TOP 50, warmups 3, iterations 15)

Raw SQL, 10k → 50k:

| pattern | rows 10k/50k | median ms 10k | p95 10k | median ms 50k | p95 50k | plan at 10k |
|---|---|---|---|---|---|---|
| name prefix, accented (`HoTen`) | 6 / 8 | 0.4 | 0.6 | 0.4 | 0.5 | Index Seek |
| name prefix, ASCII (`HoTen`) | 0 / 0 | 0.3 | 0.5 | 0.3 | 1.0 | Index Seek |
| **stripped prefix** (`HoTenKhongDau`) | 6 / 8 | **0.3** | 0.4 | **0.3** | 0.5 | Index Seek |
| word start (`LIKE '% ' + @t`) | 50 | 12.4 | 13.5 | 61.2 | 66.0 | Index Scan |
| contains (`%do duc dat%`) | 5 | 10.2 | 15.5 | 75.9 | 82.2 | Index Seek (covering) |
| code prefix narrow, varchar param | 1 | 0.3 | 0.4 | 0.3 | 0.4 | Seek on `UQ_MaSo` |
| code prefix narrow, nvarchar param | 1 | 3.3 | 5.8 | 14.4 | 18.2 | Scan (implicit convert) |
| code prefix broad `DT000%` | 50 | 3.5 | 5.4 | 15.8 | 18.6 | Index Scan |

EF Core 10, fresh `DbContext` per call, `EF.Functions.Like`, `AsNoTracking`, DTO projection:

| query | rows | first ms | median ms | p95 ms |
|---|---|---|---|---|
| Auto `"nguyen van a"` (10k / 50k) | 6 / 8 | 1029.5 / 997.2 | **1.3 / 1.1** | 85.0 / 80.7 |
| Auto `"Nguyễn Văn A"` | 6 / 8 | 1.4 / 1.6 | 1.2 / 1.3 | 1.8 / 7.6 |
| Auto `"dt0001"` | 50 | 16.9 / 10.6 | 10.2 / 1.9 | 14.9 / 2.2 |
| word start `"tuan"` | 50 | 41.4 / 84.3 | 15.4 / 64.6 | 24.9 / 67.2 |
| prefix `"nguyen van"` | 38 / 50 | 1.9 / 2.9 | 1.4 / 1.8 | 4.0 / 7.7 |
| contains `"do duc dat"` | 5 | 11.1 / 46.7 | 11.3 / 52.1 | 15.4 / 60.6 |

All queries are far below the 300 ms budget; even the last-resort contains at 50k is ~82 ms.
The curve from 10k to 50k is roughly linear, so a 10× larger table would still fit for
prefix/word-start; contains would be the first to approach the budget.

EF types the code parameter from the mapped `varchar` column, so the code path avoids the implicit
conversion (`ToQueryString()` evidence in the benchmark: `DECLARE @pattern varchar(30)`); a
near-exact varchar comparison seeks `UQ_MaSo` in 0.3 ms (raw table above).

### 6. End-to-end keystroke to grid (WinForms, 300 ms debounce)

`ui-check` drives the real form with simulated typing: 10k rows, grid bound only once per
scenario (11 stale queries cancelled), rows correct:

| scenario | started / cancelled / completed | last keystroke → grid |
|---|---|---|
| `'nguyen van a'` every 60 ms | 12 / 11 / 1 | 800 ms (first query of the process) |
| `'dt000123'` every 40 ms | 8 / 7 / 1 | 411 ms |
| `'Nguyễn Văn A'` every 80 ms | 12 / 11 / 1 | 324 ms |
| `'%100'` typed | 4 / 3 / 1 | 372 ms |

The 800 ms first row is EF query compilation on top of the 300 ms debounce (model warm-up alone
was 624 ms at form load). **Steady state is ~320–410 ms end-to-end.** The spike did not validate
a warm-up query or `EF.CompileAsyncQuery` — warming only the model still left the first keystroke
at ~800 ms; Story 3.2 must try one and verify it.

### 7. Cancellation is real, not cosmetic

`sys.dm_exec_requests` was watched from a second connection while a query ran; the EF token ended
the server command:

| probe | request seen | cancelled | request gone | client return | exception |
|---|---|---|---|---|---|
| `ExecuteSqlRawAsync(WAITFOR 5s, token)` | 18 ms | 160 ms after seen | 193 ms | 194 ms | `SqlException 0` |
| LINQ cross-join `SumAsync` (slow math) | 50 ms | 151 ms after seen | 217 ms | 217 ms | `OperationCanceledException` |

## Decision

**Adopt option (a): a persisted `HoTenKhongDau` column filled by the Application layer, plus a
covering index, plus an Auto query cascade with a 300 ms debounce and cancel-previous.**
"Search by name without diacritics" is implemented in the application, not by the collation;
`Vietnamese_CI_AI` remains the database collation because it still gives case-insensitive,
tone-mark-folding comparison and correct Vietnamese sorting.

Reusable conventions (also for `HangHoa.TenHang` POS search, Epic 10, and Ctrl+K global search,
Epic 13): name the stripped column `<Column>KhongDau`, fill it with the same
`RemoveDiacritics(NormalizeForSearch(...))` helper, index it covering the columns the UI shows.

## What Story 3.2 (detainee picker) must adopt

- [ ] Add `HoTenKhongDau nvarchar(100) NOT NULL`; fill on create/update; unit-test
      `RemoveDiacritics` with `đ/Đ`, NFD input and stray/duplicated spaces. Backfill existing rows
      during Story 3.1/3.2 (add nullable in the migration, backfill via the app, then set NOT NULL;
      or use a data migration). No import path may leave it null.
- [ ] Normalize + fold the typed term: NFC, trim/collapse whitespace, strip diacritics,
      escape `\`, `%`, `_`, `[`, then `EF.Functions.Like(column, pattern, "\\")`.
- [ ] Minimum term length: **names 2 characters, codes 1**; Auto = code-like if it starts with
      0–6 letters followed by a digit.
- [ ] Query cascade prefix → word start → contains; `TOP (50)`, `ORDER BY HoTen, MaSo`; then
      Story 3.2's exact-code-first ranking on top.
- [ ] Covering index `IX_DoiTuong_HoTenKhongDau (HoTenKhongDau) INCLUDE (HoTen, MaSo, NamSinh,
      BuongGiam, …)`; keep the DB design's `IX_HoTen (HoTen)` unless a later review drops it.
- [ ] The debounce/cancellation snippet below, one fresh DI scope/DbContext per search, bind only
      the run whose version is current.
- [ ] Address first-query EF compilation: the spike did not validate a warm-up query or
      `EF.CompileAsyncQuery` (warming only the model left the first keystroke at ~800 ms). Story
      3.2 must test one and confirm the first keystroke is within budget.

## Debounce and cancellation pattern

The spike's `Search/IncrementalSearch.cs`, reduced to the essential shape:

```csharp
private CancellationTokenSource? _pending;
private long _version;

public void OnTextChanged(string term)          // TextBox.TextChanged
{
    _pending?.Cancel();                          // stale query is cancelled now
    var cts = _pending = new CancellationTokenSource();
    var version = ++_version;
    _ = SearchAsync(term, version, cts);
}

private async Task SearchAsync(string term, long version, CancellationTokenSource cts)
{
    try
    {
        await Task.Delay(300, cts.Token);        // debounce; token aborts the wait
        var rows = await query
            .OrderBy(x => x.HoTen).ThenBy(x => x.MaSo)
            .Take(50)
            .Select(x => new PickerRow(x.Id, x.MaSo, x.HoTen, x.NamSinh, x.BuongGiam))
            .ToListAsync(cts.Token);             // token reaches SqlCommand (proven above)
        if (version == _version)                 // bind only the current run
        {
            _grid.DataSource = rows;
        }
    }
    catch (OperationCanceledException)
    {
        // a newer keystroke won; nothing to do
    }
    finally
    {
        cts.Dispose();
    }
}
```

`WaitForIdleAsync()` (await the latest task) is an optional test signal for Story 3.2; the spike's
drivers do not call it — they subscribe to a `Bound` event completed by a `TaskCompletionSource`
instead.

## Consequences

- Every consumer of the same pattern pays one extra `nvarchar(100)` column and one covering index
  per searched table; reads dominate this app, so the write cost is acceptable.
- Names and codes share one search box without ambiguity (Auto detection); users never need to
  type diacritics or know the `MaSo` format.
- Stale results cannot flash into the grid: the token is checked before binding, and the SQL side
  really stops (measured).
- Prefix/word-start keep most keystrokes on seeks; contains is only a fallback.

## Open risks / still to verify

1. **Target hardware and LAN not measured.** All numbers are LocalDB on the dev machine. Run
   `spikes/SP-03-Search` with `--connection` against the unit's SQL Server 2022 Express and a
   workstation, and re-record `--benchmark`/`--ui-check`, before signing NFR10 off. There is large
   headroom (worst measured query ~80 ms at 50k), so the risk is low.
2. **First-query compilation** could break the "immediate" feel if the picker is opened cold. The
   spike did not validate a warm-up/compiled query (it warmed the EF model only), so Story 3.2
   must verify one.
3. **History grows** (released detainees stay). The 10k→50k curve is linear; re-check at the real
   row count. If needed, the active-only default for money forms (Story 3.2) reduces the scanned
   set, but a filter does not change the plan of the fallback scans.
4. **No test on the installed `Vietnamese_CI_AI` database yet** — this ADR is validated on a
   throw-away DB with the same collation; Story 1.2's migration sets it for the product.
5. The spike ranks by `HoTen, MaSo` only; Story 3.2's "exact code first" ranking is not yet
   measured.

## Alternatives considered

1. **Normalize the search term only, pattern `d → [dđ]` on `HoTen`** (option (b) in the story):
   turns every prefix seek into a scan and gets worse with `ă/â/ê/ô/ơ/ư`; rejected.
2. **Accept tone-insensitive matching and train users to type Vietnamese letters** (option (c)):
   contradicts FR14/UX-DR2; rejected.
3. **A SQL computed column**: SQL Server cannot strip diacritics natively (CLR required); a
   correctly-filled app-side column is simpler and testable; chosen.
4. **Full-text search (`CONTAINS`)**: heavier setup (catalog/population), weak for `đ`/prefix and
   incremental input at this scale; rejected (KISS/YAGNI).
5. **A different collation** (`Latin1_General_100_CI_AI` etc.): the `ă â đ ê ô ơ ư` letter problem
   remains; no collation folds letters that are legally distinct; rejected.
