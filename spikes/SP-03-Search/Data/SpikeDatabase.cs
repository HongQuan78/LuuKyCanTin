using System.Data;
using Microsoft.Data.SqlClient;

namespace SP03Search.Data;

internal sealed record SpikeDatabaseInfo(string ServerInfo, IReadOnlyList<string> Indexes);

internal sealed class SpikeDatabase
{
    private readonly SpikeOptions _options;

    public SpikeDatabase(SpikeOptions options) => _options = options;

    public async Task RecreateAsync(CancellationToken ct)
    {
        var scriptPath = Path.Combine(AppContext.BaseDirectory, "Data", "Sql", "create-spike-db.sql");
        if (!File.Exists(scriptPath))
        {
            throw new FileNotFoundException("Spike schema script not found next to the executable.", scriptPath);
        }

        await SqlScriptRunner.ExecuteFileAsync(scriptPath, _options.MasterConnectionString, ct);
    }

    public async Task<bool> ExistsAsync(CancellationToken ct)
    {
        await using var connection = new SqlConnection(_options.MasterConnectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT DB_ID(N'{SpikeOptions.DatabaseName}')";
        var result = await command.ExecuteScalarAsync(ct);
        return result is not null && result is not DBNull;
    }

    public async Task<long> CountAsync(CancellationToken ct)
    {
        if (!await ExistsAsync(ct))
        {
            return 0;
        }

        await using var connection = new SqlConnection(_options.DatabaseConnectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike";
        return Convert.ToInt64(await command.ExecuteScalarAsync(ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task EnsureSeededAsync(int rows, CancellationToken ct)
    {
        if (await CountAsync(ct) == rows)
        {
            return;
        }

        await RecreateAsync(ct);
        await SeedAsync(rows, ct);
    }

    public async Task SeedAsync(int rows, CancellationToken ct)
    {
        var people = VietnameseNames.Generate(rows);

        await using var connection = new SqlConnection(_options.DatabaseConnectionString);
        await connection.OpenAsync(ct);

        await using (var delete = connection.CreateCommand())
        {
            delete.CommandText = "DELETE FROM dbo.DoiTuongSpike; DBCC CHECKIDENT ('dbo.DoiTuongSpike', RESEED, 0);";
            delete.CommandTimeout = 180;
            await delete.ExecuteNonQueryAsync(ct);
        }

        var table = new DataTable();
        table.Columns.Add("MaSo", typeof(string));
        table.Columns.Add("HoTen", typeof(string));
        table.Columns.Add("HoTenKhongDau", typeof(string));
        table.Columns.Add("NamSinh", typeof(short));
        table.Columns.Add("BuongGiam", typeof(string));
        foreach (var person in people)
        {
            table.Rows.Add(person.MaSo, person.HoTen, person.HoTenKhongDau, person.NamSinh, person.BuongGiam);
        }

        using var bulk = new SqlBulkCopy(connection)
        {
            DestinationTableName = "dbo.DoiTuongSpike",
            BatchSize = 5000,
            BulkCopyTimeout = 300,
        };
        bulk.ColumnMappings.Add("MaSo", "MaSo");
        bulk.ColumnMappings.Add("HoTen", "HoTen");
        bulk.ColumnMappings.Add("HoTenKhongDau", "HoTenKhongDau");
        bulk.ColumnMappings.Add("NamSinh", "NamSinh");
        bulk.ColumnMappings.Add("BuongGiam", "BuongGiam");
        await bulk.WriteToServerAsync(table, ct);
    }

    public async Task<SpikeDatabaseInfo> GetInfoAsync(CancellationToken ct)
    {
        await using var connection = new SqlConnection(_options.DatabaseConnectionString);
        await connection.OpenAsync(ct);

        string serverInfo;
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT CONCAT(
                    CAST(SERVERPROPERTY('ProductVersion') AS nvarchar(50)), ' | ',
                    CAST(SERVERPROPERTY('Edition') AS nvarchar(100)), ' | ',
                    CAST(SERVERPROPERTY('Collation') AS nvarchar(100)), ' | db ',
                    CAST(DATABASEPROPERTYEX(DB_NAME(), 'Collation') AS nvarchar(100)))
                """;
            serverInfo = (string)(await command.ExecuteScalarAsync(ct))!;
        }

        var indexes = new List<string>();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT i.name
                FROM sys.indexes AS i
                WHERE i.object_id = OBJECT_ID(N'dbo.DoiTuongSpike') AND i.name IS NOT NULL
                ORDER BY i.name
                """;
            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                indexes.Add(reader.GetString(0));
            }
        }

        return new SpikeDatabaseInfo(serverInfo, indexes);
    }

    public async Task<long> CountRawAsync(string whereClause, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_options.DatabaseConnectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT COUNT_BIG(*) FROM dbo.DoiTuongSpike WHERE {whereClause}";
        command.CommandTimeout = 120;
        return Convert.ToInt64(await command.ExecuteScalarAsync(ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task<IReadOnlyList<int>> IdsRawAsync(string selectSql, CancellationToken ct)
    {
        await using var connection = new SqlConnection(_options.DatabaseConnectionString);
        await connection.OpenAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = selectSql;
        command.CommandTimeout = 120;
        await using var reader = await command.ExecuteReaderAsync(ct);
        var ids = new List<int>();
        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetInt32(0));
        }

        return ids;
    }
}
