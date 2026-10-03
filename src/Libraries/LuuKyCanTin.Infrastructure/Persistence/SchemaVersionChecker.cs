using System.Data.Common;
using LuuKyCanTin.Application.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LuuKyCanTin.Infrastructure.Persistence;

internal sealed class SchemaVersionChecker(AppDbContext db, ILogger<SchemaVersionChecker> logger) : ISchemaVersionChecker
{
    public async Task<SchemaVersionCheckResult> CheckAsync(CancellationToken ct = default)
    {
        var buildMigrations = db.Database.GetMigrations().ToList();

        IReadOnlyList<string> appliedMigrations;
        try
        {
            // A missing database or history table yields an empty list (a mismatch), not an exception.
            appliedMigrations = (await db.Database.GetAppliedMigrationsAsync(ct)).ToList();
        }
        // A malformed connection string (bad keyword or value) is thrown by SqlClient as ArgumentException.
        catch (Exception ex) when (ex is DbException or ArgumentException)
        {
            logger.LogError(ex, "Cannot connect to the database to check its schema version");
            return SchemaVersionCheckResult.CreateConnectionFailed(buildMigrations[^1]);
        }

        return SchemaVersionCheckResult.Create(buildMigrations, appliedMigrations);
    }
}
