using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.HeThong;

internal sealed class ThongTinDonViStore(AppDbContext db) : IThongTinDonViStore
{
    public Task<ThongTinDonVi?> LayAsync(CancellationToken ct = default) =>
        db.ThongTinDonVi.AsNoTracking().FirstOrDefaultAsync(u => u.Id == 1, ct);
}
