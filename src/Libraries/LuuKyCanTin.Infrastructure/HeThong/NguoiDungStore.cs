using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.HeThong;

internal sealed class NguoiDungStore(AppDbContext db) : INguoiDungStore
{
    public Task<NguoiDung?> TimTheoDangNhapAsync(string tenDangNhap, CancellationToken ct = default) =>
        db.NguoiDung.AsNoTracking().FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap, ct);
}
