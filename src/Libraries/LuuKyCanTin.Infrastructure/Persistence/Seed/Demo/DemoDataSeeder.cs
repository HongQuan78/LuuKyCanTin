using LuuKyCanTin.Application.HeThong;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;

internal sealed class DemoDataSeeder(AppDbContext db) : IDemoDataSeeder
{
    public async Task<DemoSeedDecision> SeedAsync(bool isDevelopment, string? confirmedDatabaseName, CancellationToken cancellationToken = default)
    {
        var decision = DemoSeedPolicy.Decide(isDevelopment, confirmedDatabaseName, db.Database.GetDbConnection().Database);
        if (decision != DemoSeedDecision.Allowed)
            return decision;

        await FillUnitInfoAsync(cancellationToken);
        return decision;
    }

    // The installed row is empty on purpose; on a dev machine the printed header should not be blank.
    // Any field the admin already filled means the row is in use and must not be overwritten.
    private async Task FillUnitInfoAsync(CancellationToken cancellationToken)
    {
        var donVi = await db.ThongTinDonVi.FirstOrDefaultAsync(u => u.Id == 1, cancellationToken);
        if (donVi is null
            || !string.IsNullOrWhiteSpace(donVi.TenCoQuanChuQuan)
            || !string.IsNullOrWhiteSpace(donVi.TenDonVi)
            || !string.IsNullOrWhiteSpace(donVi.DiaChi))
            return;

        donVi.TenCoQuanChuQuan = "CÔNG AN TỈNH …";
        donVi.TenDonVi = "TRẠI TẠM GIAM … (dữ liệu mẫu)";
        donVi.DiaChi = "Xã …, huyện …, tỉnh …";
        await db.SaveChangesAsync(cancellationToken);
    }
}
