using System.Data.Common;
using LuuKyCanTin.Application.HeThong;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LuuKyCanTin.Infrastructure.Persistence;

internal sealed class SchemaVersionChecker(AppDbContext db, ILogger<SchemaVersionChecker> logger) : ISchemaVersionChecker
{
    public async Task<SchemaVersionCheckResult> KiemTraAsync(CancellationToken ct = default)
    {
        var danhSachMigrationCuaBan = db.Database.GetMigrations().ToList();

        IReadOnlyList<string> danhSachMigrationDaApDung;
        try
        {
            // A missing database or history table yields an empty list (a mismatch), not an exception.
            danhSachMigrationDaApDung = (await db.Database.GetAppliedMigrationsAsync(ct)).ToList();
        }
        // A malformed connection string (bad keyword or value) is thrown by SqlClient as ArgumentException.
        catch (Exception ex) when (ex is DbException or ArgumentException)
        {
            logger.LogError(ex, "Cannot connect to the database to check its schema version");
            return SchemaVersionCheckResult.TaoLoiKetNoi(danhSachMigrationCuaBan[^1]);
        }

        return SchemaVersionCheckResult.Tao(danhSachMigrationCuaBan, danhSachMigrationDaApDung);
    }
}
