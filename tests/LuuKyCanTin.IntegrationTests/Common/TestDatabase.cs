using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.IntegrationTests.Common;

/// <summary>
/// A uniquely named LocalDB database, dropped on dispose. Unique names let test classes run in parallel:
/// ALTER DATABASE COLLATE needs exclusive access to its database.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    public string Name { get; } = $"LuuKyCanTin_Test_{Guid.NewGuid():N}";

    public string ConnectionString => new SqlConnectionStringBuilder(LocalDbFactAttribute.ConnectionString)
    {
        InitialCatalog = Name,
    }.ConnectionString;

    public DbContextOptions<AppDbContext> Options => new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer(ConnectionString)
        .Options;

    public AppDbContext CreateDbContext() => new(Options);

    public async Task MigrateAsync()
    {
        await using var db = CreateDbContext();
        await db.Database.MigrateAsync();
    }

    /// <summary>Creates the database the way an admin would before the first migration: empty, server default collation.</summary>
    public async Task CreateEmptyAsync()
    {
        await using var connection = new SqlConnection(LocalDbFactAttribute.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand($"CREATE DATABASE [{Name}]", connection);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<object?> ScalarAsync(string sql)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        return await command.ExecuteScalarAsync();
    }

    public async Task ExecuteAsync(string sql)
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // Pooled connections would keep the database in use and block the drop.
        SqlConnection.ClearAllPools();
        await using var db = CreateDbContext();
        await db.Database.EnsureDeletedAsync();
    }
}
