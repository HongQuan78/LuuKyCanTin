using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.HeThong;

internal sealed class NguoiDungStore(AppDbContext db) : INguoiDungStore
{
    // Tracked on purpose: sign-in changes the failure counters and saves the same entity.
    public Task<NguoiDung?> TimTheoDangNhapAsync(string tenDangNhap, CancellationToken ct = default) =>
        db.NguoiDung.FirstOrDefaultAsync(u => u.TenDangNhap == tenDangNhap, ct);

    public Task<NguoiDung?> TimTheoIdAsync(int nguoiDungId, CancellationToken ct = default) =>
        db.NguoiDung.FirstOrDefaultAsync(u => u.Id == nguoiDungId, ct);

    public Task<string?> LayHoTenCanBoAsync(int canBoId, CancellationToken ct = default) =>
        db.CanBo.Where(c => c.Id == canBoId).Select(c => c.HoTen).FirstOrDefaultAsync(ct);

    public Task LuuAsync(NguoiDung nguoiDung, CancellationToken ct = default) =>
        ((IAppDbContext)db).SaveChangesAsync(ct);

    public Task TaiLaiAsync(NguoiDung nguoiDung, CancellationToken ct = default) =>
        db.Entry(nguoiDung).ReloadAsync(ct);
}
