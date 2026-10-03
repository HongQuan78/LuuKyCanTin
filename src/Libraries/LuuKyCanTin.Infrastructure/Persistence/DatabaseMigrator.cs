using LuuKyCanTin.Application.HeThong;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence;

internal sealed class DatabaseMigrator(AppDbContext db) : IDatabaseMigrator
{
    public async Task<IReadOnlyList<string>> ApDungMigrationAsync(CancellationToken ct = default)
    {
        var danhSachChuaApDung = (await db.Database.GetPendingMigrationsAsync(ct)).ToList();
        await db.Database.MigrateAsync(ct);
        return danhSachChuaApDung;
    }
}
