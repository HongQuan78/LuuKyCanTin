# SP-03: Vietnamese diacritic-insensitive incremental search (Story 1.7)

Throw-away spike. Not part of `LuuKyCanTin.slnx` and never referenced by `src/`.
The deliverable is the decision note: [`docs/decisions/0003-vietnamese-incremental-search.md`](../../docs/decisions/0003-vietnamese-incremental-search.md).

## What it does

- Creates a throw-away database with the product's `Vietnamese_CI_AI` database collation and a
  `DoiTuongSpike` table (mirrors `DoiTuong`), then seeds 10,000 (or 50,000) deterministic
  Vietnamese names, including namesakes and `Đ/đ`.
- Checks the collation behaviour: tone marks fold, `ă â đ ê ô ơ ư` do not, NFD input fails, `LIKE`
  wildcards must be escaped.
- Measures the candidate raw SQL shapes with `SET STATISTICS IO, XML ON` (seek vs scan, logical
  reads, median/p95) and the same queries through EF Core 10 (fresh `DbContext` per call,
  `AsNoTracking`, DTO projection).
- Proves the `CancellationToken` reaches `SqlCommand` by watching `sys.dm_exec_requests`, and runs
  the 300 ms debounce + cancel-previous pattern headlessly.
- Drives the WinForms form with simulated keystrokes and measures keystroke → grid filled.

## Run

Default server is LocalDB (`(localdb)\MSSQLLocalDB`). Use `--connection` for SQL Server Express on
the target-like machine.

```powershell
dotnet run --project spikes/SP-03-Search -- --setup --rows 10000
dotnet run --project spikes/SP-03-Search -- --benchmark --rows 10000 --out spikes/SP-03-Search/artifacts
dotnet run --project spikes/SP-03-Search -- --ui-check --rows 10000 --out spikes/SP-03-Search/artifacts
# curve at the higher row count
dotnet run --project spikes/SP-03-Search -- --benchmark --rows 50000 --out spikes/SP-03-Search/artifacts
dotnet run --project spikes/SP-03-Search -- --ui
```

`--setup` drops `LuuKyCanTin_Spike_Search` if it exists. Never point it at production.
