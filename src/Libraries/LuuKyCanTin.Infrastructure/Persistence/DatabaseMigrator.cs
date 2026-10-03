using LuuKyCanTin.Application.Administration;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence;

internal sealed class DatabaseMigrator(AppDbContext db) : IDatabaseMigrator
{
    public async Task<IReadOnlyList<string>> MigrateAsync(CancellationToken ct = default)
    {
        var pending = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();
        await db.Database.MigrateAsync(ct);
        return pending;
    }
}
