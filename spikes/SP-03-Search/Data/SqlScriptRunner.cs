using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace SP03Search.Data;

internal static class SqlScriptRunner
{
    private static readonly Regex BatchSeparator = new(
        @"^[ \t]*GO[ \t]*(--.*)?$",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    public static async Task ExecuteFileAsync(string path, string connectionString, CancellationToken ct)
    {
        var script = await File.ReadAllTextAsync(path, ct);
        await ExecuteAsync(script, connectionString, ct);
    }

    public static async Task ExecuteAsync(string script, string connectionString, CancellationToken ct)
    {
        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(ct);

        foreach (var batch in BatchSeparator.Split(script))
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandText = batch;
            command.CommandTimeout = 180;
            await command.ExecuteNonQueryAsync(ct);
        }
    }
}
