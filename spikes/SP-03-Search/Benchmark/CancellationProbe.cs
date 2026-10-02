using System.Diagnostics;
using System.Globalization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SP03Search.Data;

namespace SP03Search.Benchmark;

internal sealed record CancellationResult(
    string Probe,
    string RequestSeen,
    string CancelledAfter,
    string RequestGone,
    string ClientReturned,
    string Exception);

/// <summary>
/// Proves that the CancellationToken handed to EF reaches SqlCommand: a second connection
/// watches sys.dm_exec_requests for the query's session id while the token is cancelled.
/// </summary>
internal sealed class CancellationProbe
{
    private readonly SpikeDbContextFactory _factory;
    private readonly string _connectionString;

    public CancellationProbe(SpikeDbContextFactory factory, string connectionString)
    {
        _factory = factory;
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<CancellationResult>> RunAsync(CancellationToken ct)
    {
        var results = new List<CancellationResult>
        {
            await ProbeRawAsync(ct),
            await ProbeLinqAsync(ct),
        };
        return results;
    }

    private async Task<CancellationResult> ProbeRawAsync(CancellationToken ct)
    {
        await using var context = _factory.CreateDbContext();
        await context.Database.OpenConnectionAsync(ct);
        var spid = await GetSpidAsync(context, ct);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var stopwatch = Stopwatch.StartNew();
        var queryTask = context.Database.ExecuteSqlRawAsync("WAITFOR DELAY '00:00:05';", cts.Token);
        var watch = await WatchAndCancelAsync(spid, cts, stopwatch, ct);
        var exception = await ObserveAsync(queryTask);
        stopwatch.Stop();

        return new CancellationResult(
            "EF ExecuteSqlRawAsync(WAITFOR 5s) + token",
            watch.Seen,
            watch.CancelledAfter,
            watch.Gone,
            $"{stopwatch.ElapsedMilliseconds} ms",
            exception);
    }

    private async Task<CancellationResult> ProbeLinqAsync(CancellationToken ct)
    {
        await using var context = _factory.CreateDbContext();
        await context.Database.OpenConnectionAsync(ct);
        var spid = await GetSpidAsync(context, ct);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var stopwatch = Stopwatch.StartNew();
        var queryTask = context.DoiTuongSpike
            .SelectMany(a => context.DoiTuongSpike, (a, b) => Math.Sqrt((double)a.Id * b.Id))
            .SumAsync(cts.Token);
        var watch = await WatchAndCancelAsync(spid, cts, stopwatch, ct);
        var exception = await ObserveAsync(queryTask);
        stopwatch.Stop();

        return new CancellationResult(
            "EF LINQ cross-join SumAsync(math) + token",
            watch.Seen,
            watch.CancelledAfter,
            watch.Gone,
            $"{stopwatch.ElapsedMilliseconds} ms",
            exception);
    }

    private static async Task<short> GetSpidAsync(SpikeDbContext context, CancellationToken ct)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT @@SPID";
        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt16(result, CultureInfo.InvariantCulture);
    }

    private async Task<(string Seen, string CancelledAfter, string Gone)> WatchAndCancelAsync(
        short spid,
        CancellationTokenSource cts,
        Stopwatch stopwatch,
        CancellationToken ct)
    {
        await using var watcher = new SqlConnection(_connectionString);
        await watcher.OpenAsync(ct);

        long? seenAt = null;
        long? cancelledAt = null;
        while (stopwatch.ElapsedMilliseconds < 5000)
        {
            var active = await IsActiveAsync(watcher, spid, ct);
            if (active && seenAt is null)
            {
                seenAt = stopwatch.ElapsedMilliseconds;
            }

            if (seenAt is not null && cancelledAt is null && stopwatch.ElapsedMilliseconds - seenAt >= 150)
            {
                cts.Cancel();
                cancelledAt = stopwatch.ElapsedMilliseconds;
            }

            if (seenAt is not null && !active)
            {
                return (
                    $"yes, {seenAt} ms",
                    cancelledAt is null ? "not cancelled" : $"{cancelledAt - seenAt} ms after seen",
                    $"yes, {stopwatch.ElapsedMilliseconds} ms");
            }

            await Task.Delay(10, ct);
        }

        return (
            seenAt is null ? "not seen" : $"yes, {seenAt} ms",
            cancelledAt is null ? "not cancelled" : $"{cancelledAt - seenAt} ms after seen",
            "not gone within 5 s");
    }

    private static async Task<bool> IsActiveAsync(SqlConnection watcher, short spid, CancellationToken ct)
    {
        await using var command = watcher.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sys.dm_exec_requests WHERE session_id = @spid";
        command.Parameters.AddWithValue("@spid", spid);
        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result, CultureInfo.InvariantCulture) > 0;
    }

    private static async Task<string> ObserveAsync(Task queryTask)
    {
        try
        {
            await queryTask;
            return "none";
        }
        catch (OperationCanceledException)
        {
            return "OperationCanceledException";
        }
        catch (SqlException ex)
        {
            return $"SqlException {ex.Number}";
        }
        catch (Exception ex)
        {
            return ex.GetType().Name;
        }
    }
}
