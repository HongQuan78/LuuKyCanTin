using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Microsoft.Data.SqlClient;
using SP03Search.Data;

namespace SP03Search.Benchmark;

internal sealed record RawQueryResult(
    string Id,
    string Term,
    int Rows,
    double MedianMs,
    double P95Ms,
    long LogicalReads,
    string Plan,
    string Note);

internal sealed record RawPattern(
    string Id,
    string Display,
    string Value,
    string Where,
    string Note,
    SqlDbType ParameterType = SqlDbType.NVarChar);

/// <summary>
/// Runs the candidate query shapes as raw parameterized SQL with SET STATISTICS IO/Xml ON,
/// so every row of the report has real timings, logical reads and the actual plan (seek vs scan).
/// </summary>
internal sealed class RawQueryProbe
{
    private const int Warmups = 3;
    private const int Iterations = 15;

    private readonly string _connectionString;
    private long _readsInWindow;

    public RawQueryProbe(string connectionString) => _connectionString = connectionString;

    public static IReadOnlyList<RawPattern> BuildPatterns() =>
    [
        new("name-prefix-accented", "Nguyễn Văn A%", "Nguyễn Văn A", "HoTen LIKE @t + '%'", "accented prefix seek (collation query)"),
        new("name-prefix-ascii-collation", "nguyen van a%", "nguyen van a", "HoTen LIKE @t + '%'", "same query, ASCII term: collation fails (0 rows)"),
        new("name-prefix-broad", "nguyen%", "nguyen", "HoTen LIKE @t + '%'", "broad prefix; 'Nguyễn' folds, 'Văn' does not"),
        new("name-contains", "%nguyen%", "nguyen", "HoTen LIKE '%' + @t + '%'", "contains"),
        new("name-contains-van", "%van%", "van", "HoTen LIKE '%' + @t + '%'", "contains 'văn' folded: 0 under collation"),
        new("name-contains-1char", "%an%", "an", "HoTen LIKE '%' + @t + '%'", "1-char contains"),
        new("name-wordstart-tuan", "tuan%", "tuan", "(HoTen LIKE @t + '%' OR HoTen LIKE '% ' + @t + '%')", "given name or word start"),
        new("code-prefix", "DT000%", "DT000", "MaSo LIKE @t + '%'", "code prefix, nvarchar param (EF default)"),
        new("code-prefix-narrow", "dt000123%", "dt000123", "MaSo LIKE @t + '%'", "near-exact code, nvarchar param"),
        new("code-prefix-narrow-varchar", "dt000123%", "dt000123", "MaSo LIKE @t + '%'", "near-exact code, varchar param", SqlDbType.VarChar),
        new("code-prefix-varchar", "DT000%", "DT000", "MaSo LIKE @t + '%'", "same, varchar param (no conversion)", SqlDbType.VarChar),
        new("khongdau-prefix", "nguyen van a%", "nguyen van a", "HoTenKhongDau LIKE @t + '%'", "option (a): prefix seek, ASCII term works"),
        new("khongdau-prefix-1char", "n%", "n", "HoTenKhongDau LIKE @t + '%'", "option (a): 1-char prefix (min-length evidence)"),
        new("khongdau-contains-nguyen-van", "%nguyen van%", "nguyen van", "HoTenKhongDau LIKE '%' + @t + '%'", "option (a): contains"),
        new("khongdau-wordstart-tuan", "tuan%", "tuan", "(HoTenKhongDau LIKE @t + '%' OR HoTenKhongDau LIKE '% ' + @t + '%')", "option (a): given name or word start"),
        new("khongdau-contains-do-duc-dat", "%do duc dat%", "do duc dat", "HoTenKhongDau LIKE '%' + @t + '%'", "option (a): full ASCII name chunk"),
        new("code-prefix-1char", "D%", "D", "MaSo LIKE @t + '%'", "1-char code prefix (min-length evidence)"),
    ];

    public async Task<IReadOnlyList<RawQueryResult>> RunAsync(CancellationToken ct)
    {
        var results = new List<RawQueryResult>();
        foreach (var pattern in BuildPatterns())
        {
            ct.ThrowIfCancellationRequested();
            results.Add(await MeasureAsync(pattern, ct));
        }

        return results;
    }

    public async Task<RawQueryResult> MeasureAsync(RawPattern pattern, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        connection.InfoMessage += OnInfoMessage;
        await using (var statistics = connection.CreateCommand())
        {
            statistics.CommandText = "SET STATISTICS IO ON;";
            await statistics.ExecuteNonQueryAsync(ct);
        }

        var samples = new List<double>(Iterations);
        var rowCount = 0;
        for (var i = -Warmups; i < Iterations; i++)
        {
            using var command = connection.CreateCommand();
            command.CommandText = SelectSql(pattern.Where);
            Bind(command, pattern);
            var stopwatch = Stopwatch.StartNew();
            using var reader = await command.ExecuteReaderAsync(ct);
            var rows = 0;
            while (await reader.ReadAsync(ct))
            {
                rows++;
            }

            stopwatch.Stop();
            rowCount = rows;
            if (i >= 0)
            {
                samples.Add(stopwatch.Elapsed.TotalMilliseconds);
            }
        }

        var (plan, reads) = await CapturePlanAsync(connection, pattern, ct);
        connection.InfoMessage -= OnInfoMessage;
        return new RawQueryResult(
            pattern.Id,
            pattern.Display,
            rowCount,
            Timing.Median(samples),
            Timing.P95(samples),
            reads,
            plan,
            pattern.Note);
    }

    public async Task RecreateKhongDauIndexAsync(bool covering, CancellationToken ct)
    {
        var include = covering ? " INCLUDE (HoTen, MaSo, NamSinh, BuongGiam)" : string.Empty;
        var sql =
            "DROP INDEX IF EXISTS IX_HoTenKhongDau ON dbo.DoiTuongSpike; " +
            $"CREATE INDEX IX_HoTenKhongDau ON dbo.DoiTuongSpike (HoTenKhongDau){include};";
        await SqlScriptRunner.ExecuteAsync(sql, _connectionString, ct);
    }

    private static string SelectSql(string where) =>
        "SELECT TOP (50) Id, MaSo, HoTen, NamSinh, BuongGiam " +
        "FROM dbo.DoiTuongSpike WHERE " + where + " ORDER BY HoTen, MaSo";

    private static void Bind(SqlCommand command, RawPattern pattern)
    {
        command.CommandTimeout = 120;
        command.Parameters.Add("@t", pattern.ParameterType, 100).Value = pattern.Value;
    }

    private void OnInfoMessage(object sender, SqlInfoMessageEventArgs e)
    {
        foreach (var line in e.Message.Split('\n'))
        {
            var match = Regex.Match(line, @"logical reads\s+(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                Interlocked.Add(ref _readsInWindow, long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
            }
        }
    }

    private async Task<(string Plan, long LogicalReads)> CapturePlanAsync(
        SqlConnection connection,
        RawPattern pattern,
        CancellationToken ct)
    {
        Interlocked.Exchange(ref _readsInWindow, 0);
        using var command = connection.CreateCommand();
        command.CommandTimeout = 120;
        command.CommandText = "SET STATISTICS XML ON;\n" + SelectSql(pattern.Where) + ";\nSET STATISTICS XML OFF;";
        Bind(command, pattern);

        string? planXml = null;
        await using (var reader = await command.ExecuteReaderAsync(ct))
        {
            do
            {
                if (reader.FieldCount == 1
                    && reader.GetName(0).Contains("Showplan", StringComparison.OrdinalIgnoreCase))
                {
                    if (await reader.ReadAsync(ct))
                    {
                        planXml = reader.GetString(0);
                    }
                }
                else
                {
                    while (await reader.ReadAsync(ct))
                    {
                    }
                }
            }
            while (await reader.NextResultAsync(ct));
        }

        await Task.Delay(100, ct);
        return (DescribePlan(planXml), Interlocked.Read(ref _readsInWindow));
    }

    private static string DescribePlan(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return "(no plan captured)";
        }

        var ns = XNamespace.Get("http://schemas.microsoft.com/sqlserver/2004/07/showplan");
        var document = XDocument.Parse(xml);
        var parts = new List<string>();
        foreach (var relOp in document.Descendants(ns + "RelOp"))
        {
            var indexOp = relOp.Element(ns + "IndexScan") ?? relOp.Element(ns + "IndexSeek");
            if (indexOp is null)
            {
                continue;
            }

            var physicalOp = (string?)relOp.Attribute("PhysicalOp") ?? "?";
            var indexName = (string?)indexOp.Element(ns + "Object")?.Attribute("Index") ?? "(heap)";
            var label = $"{physicalOp} {indexName}";
            if (!parts.Contains(label))
            {
                parts.Add(label);
            }
        }

        if (parts.Count == 0)
        {
            var root = document.Descendants(ns + "RelOp").FirstOrDefault();
            return (string?)root?.Attribute("PhysicalOp") ?? "(unknown)";
        }

        return string.Join(" + ", parts);
    }
}
