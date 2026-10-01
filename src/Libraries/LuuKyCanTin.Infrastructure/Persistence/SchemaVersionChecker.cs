using System.Data.Common;
using LuuKyCanTin.Application.HeThong;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LuuKyCanTin.Infrastructure.Persistence;

internal sealed class SchemaVersionChecker(AppDbContext db, ILogger<SchemaVersionChecker> logger) : ISchemaVersionChecker
{
    public async Task<SchemaVersionCheckResult> CheckAsync(CancellationToken cancellationToken = default)
    {
        var expected = db.Database.GetMigrations().Last();

        IEnumerable<string> applied;
        try
        {
            // A missing database or history table yields an empty list (a mismatch), not an exception.
            applied = await db.Database.GetAppliedMigrationsAsync(cancellationToken);
        }
        catch (DbException ex)
        {
            logger.LogError(ex, "Cannot connect to the database to check its schema version");
            return SchemaVersionCheckResult.ConnectionFailed(expected);
        }

        return SchemaVersionCheckResult.Compare(expected, applied.LastOrDefault());
    }
}
