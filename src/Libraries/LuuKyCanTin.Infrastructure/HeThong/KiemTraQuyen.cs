using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.HeThong;

/// <summary>
/// Answers from the database on every call, never from a cache: a revoked permission or a deactivated account
/// stops working at the next check.
/// </summary>
internal sealed class KiemTraQuyen(AppDbContext db, ICurrentUser currentUser) : IKiemTraQuyen
{
    public async Task YeuCauAsync(string maQuyen, CancellationToken ct = default)
    {
        if (currentUser.NguoiDungId is not { } nguoiDungId)
            throw new KhongCoQuyenException(maQuyen);

        var coQuyen = await (from nguoiDung in db.NguoiDung
                             join nguoiDungVaiTro in db.NguoiDungVaiTro on nguoiDung.Id equals nguoiDungVaiTro.NguoiDungId
                             join vaiTroQuyen in db.VaiTroQuyen on nguoiDungVaiTro.VaiTroId equals vaiTroQuyen.VaiTroId
                             join quyen in db.Quyen on vaiTroQuyen.QuyenId equals quyen.Id
                             where nguoiDung.Id == nguoiDungId
                                 && nguoiDung.DangHoatDong
                                 && quyen.Ma == maQuyen
                             select quyen.Id).AnyAsync(ct);

        if (!coQuyen)
            throw new KhongCoQuyenException(maQuyen);
    }
}
