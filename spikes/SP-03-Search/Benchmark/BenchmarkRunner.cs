using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using SP03Search.Data;
using SP03Search.Search;

namespace SP03Search.Benchmark;

internal sealed class BenchmarkRunner
{
    private const int EfIterations = 15;

    private readonly SpikeOptions _options;
    private readonly SpikeDatabase _database;
    private readonly SpikeDbContextFactory _factory;
    private readonly SearchService _search;
    private readonly RawQueryProbe _rawProbe;
    private readonly CancellationProbe _cancellationProbe;
    private readonly StringBuilder _report = new();

    private int _failures;
    private int _namesakes;
    private int _accentedPrefixRows;

    public BenchmarkRunner(SpikeOptions options)
    {
        _options = options;
        _database = new SpikeDatabase(options);
        _factory = new SpikeDbContextFactory(options.DatabaseConnectionString);
        _search = new SearchService(_factory);
        _rawProbe = new RawQueryProbe(options.DatabaseConnectionString);
        _cancellationProbe = new CancellationProbe(_factory, options.DatabaseConnectionString);
    }

    public int Failures => _failures;

    public async Task<string> RunAsync(int rows, CancellationToken ct)
    {
        var info = await _database.GetInfoAsync(ct);
        var rowCount = await _database.CountAsync(ct);
        WriteHeader(rows, rowCount, info);

        await CollationChecksAsync(rowCount, ct);

        var raw = await _rawProbe.RunAsync(ct);
        WriteRawTable(raw);

        await IndexComparisonAsync(ct);
        await EfSectionAsync(ct);
        await ScopeCostSectionAsync(ct);

        var cancellation = await _cancellationProbe.RunAsync(ct);
        WriteCancellationTable(cancellation);

        var debounce = await DebounceSectionAsync(ct);
        WriteDebounceTable(debounce);

        WriteKeyFindings(rows, raw, debounce);
        return _report.ToString();
    }

    private void WriteHeader(int rows, long rowCount, SpikeDatabaseInfo info)
    {
        Append("=== SP-03 — Vietnamese diacritic-insensitive incremental search (Story 1.7) ===");
        Append($"Run at      : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Append($"Machine     : {Environment.MachineName} ({Environment.ProcessorCount} logical CPUs)");
        Append($"Processor   : {ProcessorName()}");
        Append($"OS          : {RuntimeInformation.OSDescription}");
        Append($"Rows        : {rows:N0}");
        Append(string.Empty);
        Append("--- Environment ---");
        Append($"Table rows  : {rowCount:N0}");
        Append($"SQL Server  : {info.ServerInfo}");
        Append($"Indexes     : {string.Join(", ", info.Indexes)}");
        Append(string.Empty);
    }

    private async Task CollationChecksAsync(long rowCount, CancellationToken ct)
    {
        Append("--- AC1: collation behaviour under Vietnamese_CI_AI (database default) ---");

        var folds = await ScalarIntAsync(
            "SELECT IIF(N'ễ' = N'e' AND N'ệ' = N'e' AND N'á' = N'a' AND N'à' = N'a' AND N'ã' = N'a' AND N'ả' = N'a' AND N'ạ' = N'a', 1, 0)",
            ct) == 1;
        Append("folds to ASCII : ễ=e, ệ=e, á=a, à=a, ã=a, ả=a, ạ=a");
        Check(folds, "tone marks fold (ễ ệ á à ã ả ạ = e/a)", "_AI ignores tone marks");

        var distinct = await ScalarIntAsync(
            "SELECT IIF(N'ă' <> N'a' AND N'â' <> N'a' AND N'đ' <> N'd' AND N'ê' <> N'e' AND N'ô' <> N'o' AND N'ơ' <> N'o' AND N'ư' <> N'u', 1, 0)",
            ct) == 1;
        Append("distinct letter: ă≠a, â≠a, đ≠d, ê≠e, ô≠o, ơ≠o, ư≠u");
        Check(distinct, "letters ă â đ ê ô ơ ư stay distinct from ASCII", "Vietnamese alphabet: separate letters, not D+accent; option (a) required for them");

        _accentedPrefixRows = (int)await CountWhereAsync("HoTen LIKE N'Nguyễn Văn A%'", ct);
        var upperPrefixRows = (int)await CountWhereAsync("HoTen LIKE N'NGUYỄN VĂN A%'", ct);
        Check(
            _accentedPrefixRows == upperPrefixRows && _accentedPrefixRows > 0,
            "accented terms, any case: 'Nguyễn Văn A%' = 'NGUYỄN VĂN A%'",
            $"{_accentedPrefixRows} rows each");

        var asciiOnHoTen = await CountWhereAsync("HoTen LIKE N'nguyen van a%'", ct);
        Check(
            asciiOnHoTen == 0,
            "collation alone: ASCII 'nguyen van a%' does NOT match HoTen",
            $"0 rows (vs {_accentedPrefixRows} accented) — 'Văn' uses ă, a distinct letter; AC1 needs option (a)");

        _namesakes = (int)await CountWhereAsync("HoTen = N'Nguyễn Văn A'", ct);
        Check(_namesakes == 5, "exact accented namesakes", $"{_namesakes}/5 'Nguyễn Văn A'");

        var tuyetFolded = await CountWhereAsync("HoTen LIKE N'%tuyet%'", ct);
        var tuyetAccented = await CountWhereAsync("HoTen LIKE N'%Tuyết%'", ct);
        Check(
            tuyetFolded == tuyetAccented && tuyetFolded > 0,
            "tone marks fold in LIKE too: '%tuyet%' = '%Tuyết%'",
            $"{tuyetFolded} rows");

        var accentedIds = await _database.IdsRawAsync(
            "SELECT Id FROM dbo.DoiTuongSpike WHERE HoTen LIKE N'Nguyễn Văn A%' ORDER BY Id", ct);
        var foldedIds = await _database.IdsRawAsync(
            "SELECT Id FROM dbo.DoiTuongSpike WHERE HoTenKhongDau LIKE N'nguyen van a%' ORDER BY Id", ct);
        Check(
            accentedIds.Count > 0 && accentedIds.SequenceEqual(foldedIds),
            "option (a): 'nguyen van a%' on HoTenKhongDau = 'Nguyễn Văn A%' on HoTen",
            $"{foldedIds.Count} rows, same ids");

        var doNamesakes = await CountWhereAsync("HoTen = N'Đỗ Đức Đạt'", ct);
        var doFolded = await CountWhereAsync("HoTenKhongDau = N'do duc dat'", ct);
        Check(
            doNamesakes == 5 && doFolded == doNamesakes,
            "option (a): 'do duc dat' on HoTenKhongDau matches 'Đỗ Đức Đạt'",
            $"namesakes {doNamesakes}/5; folded {doFolded}");

        var nfdName = VietnameseText.ToNfd("Nguyễn Văn A");
        var nfdRaw = await ScalarIntAsync(
            "SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike WHERE HoTen = @p",
            ct,
            command => command.Parameters.Add("@p", SqlDbType.NVarChar, 100).Value = nfdName);
        var nfcExact = await ScalarIntAsync(
            "SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike WHERE HoTen = @p",
            ct,
            command => command.Parameters.Add("@p", SqlDbType.NVarChar, 100).Value = VietnameseText.NormalizeForSearch(nfdName));
        Check(
            nfdRaw == 0 && nfcExact == 5,
            "NFD (Word paste): raw NFD does NOT match NFC data; NFC-normalized term does",
            $"raw NFD exact {nfdRaw}; normalized exact {nfcExact}; normalized prefix {_accentedPrefixRows} rows");

        var doubleSpace = await ScalarIntAsync(
            "SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike WHERE HoTen = N'Nguyễn  Văn A'", ct);
        var normalizedSpace = await ScalarIntAsync(
            "SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike WHERE HoTen = @p",
            ct,
            command => command.Parameters.Add("@p", SqlDbType.NVarChar, 100).Value =
                VietnameseText.NormalizeForSearch("  Nguyễn   Văn A  "));
        Check(
            doubleSpace == 0 && normalizedSpace == 5,
            "double/leading/trailing spaces: NormalizeForSearch trims and collapses",
            $"raw double-space {doubleSpace}; normalize+trim {normalizedSpace}");

        var digraphExpression = await ScalarIntAsync(
            "SELECT IIF(N'Nguyễn' LIKE N'n%', 0, 1) + IIF(N'Nguyễn' LIKE N'ng%', 1, 0)", ct);
        var oneCharStart = await CountWhereAsync("HoTen LIKE N'n%'", ct);
        var twoCharStart = await CountWhereAsync("HoTen LIKE N'ng%'", ct);
        Check(
            digraphExpression == 2 && oneCharStart == 0 && twoCharStart > 0,
            "digraph contractions ng/nh/ch/th/ph/kh/gi/qu/tr",
            $"1-char prefix + % cannot match them ('Nguyễn' LIKE 'n%' = 0, LIKE 'ng%' = 1); table 'n%' = {oneCharStart}, 'ng%' = {twoCharStart}; names need 2 characters");

        var literalPercent = await ScalarIntAsync(
            @"SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike WHERE HoTen LIKE N'%\%%' ESCAPE N'\'", ct);
        var literalUnderscore = await ScalarIntAsync(
            @"SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike WHERE HoTen LIKE N'%\_%' ESCAPE N'\'", ct);
        var unescapedUnderscore = await CountWhereAsync("HoTen LIKE N'%_%'", ct);
        var literalBracket = await ScalarIntAsync(
            @"SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike WHERE HoTen LIKE N'%\[n]%' ESCAPE N'\'", ct);
        Check(
            literalPercent == 0 && literalUnderscore == 0 && unescapedUnderscore == rowCount && literalBracket == 0,
            "typed wildcards %, _ are escaped",
            $"literal '%' {literalPercent}, literal '_' {literalUnderscore}, unescaped '_' {unescapedUnderscore}/{rowCount}, literal '\\[n]' {literalBracket}");

        Append($"  table rows at {rowCount:N0}: {rowCount:N0}");
        Append(string.Empty);
    }

    private void WriteRawTable(IReadOnlyList<RawQueryResult> results)
    {
        Append("--- Query patterns (raw SQL, parameterized, TOP 50, warmups 3, iterations 15; logical reads from the actual plan) ---");
        Append("| id | term | rows | median ms | p95 ms | logical reads | plan | note |");
        Append("|---|---|---|---|---|---|---|---|");
        foreach (var result in results)
        {
            Append(
                $"| {result.Id} | {result.Term} | {result.Rows} | {result.MedianMs:0.0} | {result.P95Ms:0.0} | " +
                $"{result.LogicalReads} | {result.Plan} | {result.Note} |");
        }

        Append(string.Empty);
    }

    private async Task IndexComparisonAsync(CancellationToken ct)
    {
        Append("--- Index comparison on the searched column (HoTenKhongDau LIKE 'nguyen%') ---");
        Append("| index | median ms | p95 ms | logical reads | plan |");
        Append("|---|---|---|---|---|");

        foreach (var covering in new[] { false, true })
        {
            await _rawProbe.RecreateKhongDauIndexAsync(covering, ct);
            var pattern = new RawPattern(
                covering ? "covering" : "key-only",
                covering ? "INCLUDE (HoTen, MaSo, NamSinh, BuongGiam)" : "key only",
                "nguyen",
                "HoTenKhongDau LIKE @t + '%'",
                string.Empty);
            var result = await _rawProbe.MeasureAsync(pattern, ct);
            var label = covering
                ? "IX_HoTenKhongDau INCLUDE (HoTen, MaSo, NamSinh, BuongGiam)"
                : "IX_HoTenKhongDau (key only)";
            Append($"| {label} | {result.MedianMs:0.0} | {result.P95Ms:0.0} | {result.LogicalReads} | {result.Plan} |");
        }

        Append("(the covering index is left in place for the remaining measurements)");
        Append(string.Empty);
    }

    private async Task EfSectionAsync(CancellationToken ct)
    {
        var scenarios = new (string Id, string Term, SearchStrategy Strategy)[]
        {
            ("ef-auto-name-ascii", "nguyen van a", SearchStrategy.Auto),
            ("ef-auto-name-accented", "Nguyễn Văn A", SearchStrategy.Auto),
            ("ef-auto-code", "dt0001", SearchStrategy.Auto),
            ("ef-name-wordstart", "tuan", SearchStrategy.TenKhongDauTuBatKy),
            ("ef-khongdau-prefix", "nguyen van", SearchStrategy.TenKhongDauPrefix),
            ("ef-khongdau-contains", "do duc dat", SearchStrategy.TenKhongDauChua),
        };

        Append("--- EF Core 10 (fresh DbContext per call, AsNoTracking, DTO projection, wall clock) ---");
        Append("| id | term | strategy | rows | first ms | median ms | p95 ms |");
        Append("|---|---|---|---|---|---|---|");

        var results = new Dictionary<string, IReadOnlyList<SearchResult>>();
        foreach (var (id, term, strategy) in scenarios)
        {
            var first = await TimeOnceAsync(term, strategy, ct);
            var samples = new List<double>(EfIterations);
            for (var i = 0; i < EfIterations; i++)
            {
                samples.Add((await TimeOnceAsync(term, strategy, ct)).Ms);
            }

            results[id] = first.Rows;
            var median = Timing.Median(samples);
            var p95 = Timing.P95(samples);
            Append($"| {id} | {term} | {strategy} | {first.Rows.Count} | {first.Ms:0.0} | {median:0.0} | {p95:0.0} |");
        }

        var ascii = results["ef-auto-name-ascii"];
        var accented = results["ef-auto-name-accented"];
        Check(
            ascii.Count > 0 && ascii.SequenceEqual(accented),
            "EF Auto: ASCII and accented terms return the same non-empty rows",
            $"'nguyen van a' = 'Nguyễn Văn A' — {ascii.Count} rows, same ids");

        var code = results["ef-auto-code"];
        Check(
            code.Count > 0 && code.All(r => r.MaSo.StartsWith("DT0001", StringComparison.OrdinalIgnoreCase)),
            "EF Auto: a code-like term takes the MaSo prefix path",
            $"'dt0001' — {code.Count} rows, all MaSo start with DT0001");

        var wordStart = results["ef-name-wordstart"];
        Check(
            wordStart.Count > 0 && wordStart.All(r => VietnameseText.RemoveDiacritics(r.HoTen).Contains("tuan", StringComparison.OrdinalIgnoreCase)),
            "EF TenKhongDauTuBatKy: finds a given name at a word start",
            $"'tuan' — {wordStart.Count} rows all contain 'tuan'");

        var prefix = results["ef-khongdau-prefix"];
        Check(
            prefix.Count > 0 && prefix.All(r => VietnameseText.RemoveDiacritics(r.HoTen).StartsWith("nguyen van", StringComparison.OrdinalIgnoreCase)),
            "EF TenKhongDauPrefix: prefix stage",
            $"'nguyen van' — {prefix.Count} rows all start with 'nguyen van'");

        var contains = results["ef-khongdau-contains"];
        Check(
            contains.Count == 5 && contains.All(r => VietnameseText.RemoveDiacritics(r.HoTen).Contains("do duc dat", StringComparison.OrdinalIgnoreCase)),
            "EF TenKhongDauChua: contains stage finds the five namesakes",
            $"'do duc dat' — {contains.Count} rows");

        Append("EF code-search SQL (parameter declaration shows the provider type):");
        foreach (var line in _search.ExplainCodeSearch("dt0001").Split('\n'))
        {
            Append("  " + line.TrimEnd());
        }

        Append(string.Empty);
    }

    private async Task ScopeCostSectionAsync(CancellationToken ct)
    {
        var samples = new List<double>(EfIterations);
        for (var i = 0; i < EfIterations; i++)
        {
            var stopwatch = Stopwatch.StartNew();
            await using var context = _factory.CreateDbContext();
            await context.Database.OpenConnectionAsync(ct);
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(ct);
            stopwatch.Stop();
            samples.Add(stopwatch.Elapsed.TotalMilliseconds);
        }

        Append(
            $"per-keystroke scope cost (new DbContext + pooled open + SELECT 1): median {Timing.Median(samples):0.0} ms, p95 {Timing.P95(samples):0.0} ms");
        Append(string.Empty);
    }

    private void WriteCancellationTable(IReadOnlyList<CancellationResult> results)
    {
        Append("--- Cancellation: does the EF token reach SqlCommand? (sys.dm_exec_requests watch) ---");
        Append("| probe | request seen | cancelled | request gone | client returned | exception |");
        Append("|---|---|---|---|---|---|");
        foreach (var result in results)
        {
            Append(
                $"| {result.Probe} | {result.RequestSeen} | {result.CancelledAfter} | {result.RequestGone} | " +
                $"{result.ClientReturned} | {result.Exception} |");
        }

        Append(string.Empty);
    }

    private async Task<IReadOnlyList<DebounceRow>> DebounceSectionAsync(CancellationToken ct)
    {
        var scenarios = new (string Label, string Term, int IntervalMs)[]
        {
            ("fast typist · name 'nguyen van a'", "nguyen van a", 60),
            ("fast typist · code 'dt000123'", "dt000123", 40),
            ("NFD paste from Word · 'Nguyễn Văn A'", VietnameseText.ToNfd("Nguyễn Văn A"), 80),
            ("wildcard '%100' typed", "%100", 60),
        };

        var results = new List<DebounceRow>();
        foreach (var (label, term, intervalMs) in scenarios)
        {
            var lastKeystroke = 0L;
            var boundRows = -1;
            var finished = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var search = new IncrementalSearch((value, token) => QueryRowsAsync(value, token))
            {
                Debounce = TimeSpan.FromMilliseconds(300),
                ResultsBound = (boundTerm, rows) =>
                {
                    if (!string.Equals(boundTerm, term, StringComparison.Ordinal))
                    {
                        return;
                    }

                    boundRows = rows.Count;
                    finished.TrySetResult();
                },
                Failed = (_, _) => finished.TrySetResult(),
            };

            for (var length = 1; length <= term.Length; length++)
            {
                if (length == term.Length)
                {
                    lastKeystroke = Stopwatch.GetTimestamp();
                }

                search.OnTextChanged(term[..length]);
                if (length < term.Length)
                {
                    await Task.Delay(intervalMs, ct);
                }
            }

            var winner = await Task.WhenAny(finished.Task, Task.Delay(TimeSpan.FromSeconds(10), ct));
            var elapsed = winner == finished.Task && lastKeystroke != 0
                ? Stopwatch.GetElapsedTime(lastKeystroke).TotalMilliseconds
                : -1;
            results.Add(new DebounceRow(
                label,
                term.Length,
                search.Counters.Started,
                search.Counters.Cancelled,
                search.Counters.Completed,
                search.Counters.Failed,
                boundRows,
                elapsed));
        }

        return results;
    }

    private void WriteDebounceTable(IReadOnlyList<DebounceRow> results)
    {
        Append("--- Debounce + stale-query cancellation (headless, same IncrementalSearch the form uses) ---");
        Append("| scenario | chars | started | cancelled | completed | failed | rows | last keystroke → results |");
        Append("|---|---|---|---|---|---|---|---|");
        foreach (var result in results)
        {
            var elapsed = result.LastToResultsMs >= 0 ? $"{result.LastToResultsMs:0} ms" : "timeout";
            Append(
                $"| {result.Label} | {result.Chars} | {result.Started} | {result.Cancelled} | " +
                $"{result.Completed} | {result.Failed} | {result.Rows} | {elapsed} |");
            Check(
                result.Completed == 1 && result.Failed == 0 && result.Rows >= 0 && result.LastToResultsMs >= 0,
                $"debounce scenario {result.Label}",
                $"completed {result.Completed}, cancelled {result.Cancelled}, rows {result.Rows}, last keystroke → results {elapsed}");
        }

        Append(string.Empty);
    }

    private void WriteKeyFindings(
        int rows,
        IReadOnlyList<RawQueryResult> raw,
        IReadOnlyList<DebounceRow> debounce)
    {
        var prefix = raw.First(r => r.Id == "khongdau-prefix");
        var wordStart = raw.First(r => r.Id == "khongdau-wordstart-tuan");
        var contains = raw.First(r => r.Id == "khongdau-contains-do-duc-dat");
        var codeNarrowNvarchar = raw.First(r => r.Id == "code-prefix-narrow");
        var codeNarrowVarchar = raw.First(r => r.Id == "code-prefix-narrow-varchar");
        var slowestDebounce = debounce.Count == 0 ? "n/a" : $"{debounce.Max(d => d.LastToResultsMs):0} ms";

        Append($"--- Key findings at {rows:N0} rows ---");
        Append("1. Vietnamese_CI_AI folds tone marks (á = a) but NOT the letters ă â đ ê ô ơ ư: in the");
        Append("   Vietnamese alphabet they are separate letters, so 'nguyen van a' never matches");
        Append("   'Nguyễn Văn A' through the collation alone. ă/đ names are the common case, not an edge.");
        Append("2. The same collation treats ng/nh/ch/th/ph/kh/gi/qu/tr as contractions: a 1-character");
        Append("   prefix + wildcard returns 0 for 'Nguyễn'/'Châu'/'Thảo'/'Phạm'/'Khánh'/'Nhung'/'Giang'");
        Append("   ('Nguyễn' LIKE 'n%' = 0, LIKE 'ng%' = 1). Names from 2 characters is therefore a");
        Append("   CORRECTNESS rule, not only UX. Codes measured fine from 1 character.");
        Append("3. NFD text pasted from Word does not match NFC data for ă ('Va'+breve ≠ 'Văn'); the app");
        Append("   must normalize the search term to NFC (NormalizeForSearch).");
        Append("4. Option (a) — a persisted HoTenKhongDau (diacritics stripped, đ→d, filled on save) with");
        Append("   an index — satisfies AC1: the ASCII and accented queries return identical rows and the");
        Append($"   prefix query keeps an index seek (median {prefix.MedianMs:0.0} ms). Recommendation for Story 3.1/3.2.");
        Append("5. Codes: MaSo prefix; a near-exact code seeks UQ_DoiTuongSpike_MaSo when the parameter is");
        Append($"   varchar (median {codeNarrowVarchar.MedianMs:0.0} ms) and scans when it is nvarchar (EF default, median {codeNarrowNvarchar.MedianMs:0.0} ms).");
        Append("   Names: HoTenKhongDau word-start (prefix seek for the first word,");
        Append($"   OR '% ' form scans at median {wordStart.MedianMs:0.0} ms); contains scans at median {contains.MedianMs:0.0} ms and is");
        Append("   the last fallback.");
        Append("6. Delegate the query to EF Core with the CancellationToken; cancellation aborts the SQL");
        Append("   command (dm_exec_requests shows the request ending). Debounce 300 ms with");
        Append($"   cancel-previous keeps a fast typist at one visible query (slowest measured {slowestDebounce}).");
        Append(string.Empty);
    }

    private async Task<(double Ms, IReadOnlyList<SearchResult> Rows)> TimeOnceAsync(
        string term,
        SearchStrategy strategy,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var outcome = await _search.SearchAsync(term, strategy, ct);
        stopwatch.Stop();
        return (stopwatch.Elapsed.TotalMilliseconds, outcome.Rows);
    }

    private async Task<IReadOnlyList<SearchResult>> QueryRowsAsync(string term, CancellationToken ct)
    {
        var outcome = await _search.SearchAsync(term, SearchStrategy.Auto, ct);
        return outcome.Rows;
    }

    private Task<long> CountWhereAsync(string where, CancellationToken ct) => _database.CountRawAsync(where, ct);

    private async Task<long> ScalarIntAsync(
        string sql,
        CancellationToken ct,
        Action<SqlCommand>? bind = null)
    {
        await using var connection = new SqlConnection(_options.DatabaseConnectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.CommandTimeout = 120;
        bind?.Invoke(command);
        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    private void Check(bool passed, string name, string detail)
    {
        Append($"[{(passed ? "PASS" : "FAIL")}] {name} — {detail}");
        if (!passed)
        {
            _failures++;
        }
    }

    private void Append(string line) => _report.AppendLine(line);

    private static string ProcessorName()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
        return key?.GetValue("ProcessorNameString")?.ToString()?.Trim() ?? "unknown";
    }

    private sealed record DebounceRow(
        string Label,
        int Chars,
        int Started,
        int Cancelled,
        int Completed,
        int Failed,
        int Rows,
        double LastToResultsMs);
}
