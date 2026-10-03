using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.HeThong;

/// <summary>
/// Refuses any change that would leave the system without an active administrator, where "administrator" means
/// an active account holding <see cref="MaQuyen.HT.Sua"/> through one of its roles. The caller opens the
/// transaction (Serializable for the guarded writes), so the check and the change are one atomic unit.
/// </summary>
public sealed class KiemTraConQuanTri(IAppDbContext db)
{
    public const string LoiPhaiConQuanTri =
        "Không thể thực hiện: hệ thống phải còn ít nhất một tài khoản quản trị đang hoạt động";

    /// <summary>An account and its role ids as the guard reads them before a change.</summary>
    public sealed record HienTrangTaiKhoan(int NguoiDungId, bool DangHoatDong, IReadOnlyCollection<int> VaiTroIds);

    /// <summary>
    /// Pure decision: after the change, does at least one active account still hold HT.Sua? Unit-tested without
    /// a database, because the whole guard rests on it.
    /// </summary>
    public static bool ConQuanTri(
        IReadOnlyCollection<HienTrangTaiKhoan> taiKhoan,
        IReadOnlyDictionary<int, IReadOnlyCollection<string>> quyenTheoVaiTro) =>
        taiKhoan.Any(t => t.DangHoatDong && t.VaiTroIds.Any(vaiTroId =>
            quyenTheoVaiTro.TryGetValue(vaiTroId, out var maQuyen)
            && maQuyen.Contains(MaQuyen.HT.Sua, StringComparer.Ordinal)));

    /// <exception cref="LoiNghiepVuException">Deactivating the account would leave no administrator.</exception>
    public async Task YeuCauKhiNgungHoatDongAsync(int nguoiDungId, CancellationToken ct = default)
    {
        var (taiKhoan, quyenTheoVaiTro) = await TaiHienTrangAsync(ct);
        var sau = taiKhoan.Select(t => t.NguoiDungId == nguoiDungId ? t with { DangHoatDong = false } : t).ToList();
        KiemTra(ConQuanTri(sau, quyenTheoVaiTro));
    }

    /// <exception cref="LoiNghiepVuException">The new role set would leave no administrator.</exception>
    public async Task YeuCauKhiDoiVaiTroAsync(int nguoiDungId, IReadOnlyCollection<int> vaiTroMoi, CancellationToken ct = default)
    {
        var (taiKhoan, quyenTheoVaiTro) = await TaiHienTrangAsync(ct);
        var sau = taiKhoan.Select(t => t.NguoiDungId == nguoiDungId ? t with { VaiTroIds = vaiTroMoi } : t).ToList();
        KiemTra(ConQuanTri(sau, quyenTheoVaiTro));
    }

    /// <exception cref="LoiNghiepVuException">The new permission set would leave no administrator.</exception>
    public async Task YeuCauKhiDoiQuyenVaiTroAsync(int vaiTroId, IReadOnlyCollection<string> maQuyenMoi, CancellationToken ct = default)
    {
        var (taiKhoan, quyenTheoVaiTro) = await TaiHienTrangAsync(ct);
        var quyenSau = quyenTheoVaiTro.ToDictionary(
            cap => cap.Key,
            cap => cap.Key == vaiTroId ? maQuyenMoi : cap.Value);
        KiemTra(ConQuanTri(taiKhoan, quyenSau));
    }

    private static void KiemTra(bool conQuanTri)
    {
        if (!conQuanTri)
            throw new LoiNghiepVuException(LoiPhaiConQuanTri);
    }

    private async Task<(
        List<HienTrangTaiKhoan> TaiKhoan,
        Dictionary<int, IReadOnlyCollection<string>> QuyenTheoVaiTro)> TaiHienTrangAsync(CancellationToken ct)
    {
        // Accounts without a role still matter for the "active" count, hence the left join.
        var hang = await (
            from nguoiDung in db.NguoiDung.AsNoTracking()
            join nguoiDungVaiTro in db.NguoiDungVaiTro.AsNoTracking() on nguoiDung.Id equals nguoiDungVaiTro.NguoiDungId into nhom
            from nguoiDungVaiTro in nhom.DefaultIfEmpty()
            select new { nguoiDung.Id, nguoiDung.DangHoatDong, VaiTroId = (int?)nguoiDungVaiTro.VaiTroId }).ToListAsync(ct);

        var taiKhoan = hang
            .GroupBy(h => (h.Id, h.DangHoatDong))
            .Select(nhom => new HienTrangTaiKhoan(
                nhom.Key.Id,
                nhom.Key.DangHoatDong,
                nhom.Where(h => h.VaiTroId.HasValue).Select(h => h.VaiTroId!.Value).ToList()))
            .ToList();

        var quyenTheoVaiTro = (await (
                from vaiTro in db.VaiTro.AsNoTracking()
                join vaiTroQuyen in db.VaiTroQuyen.AsNoTracking() on vaiTro.Id equals vaiTroQuyen.VaiTroId
                join quyen in db.Quyen.AsNoTracking() on vaiTroQuyen.QuyenId equals quyen.Id
                select new { vaiTro.Id, quyen.Ma }).ToListAsync(ct))
            .GroupBy(cap => cap.Id)
            .ToDictionary(cap => cap.Key, cap => (IReadOnlyCollection<string>)cap.Select(q => q.Ma).ToList());

        return (taiKhoan, quyenTheoVaiTro);
    }
}
