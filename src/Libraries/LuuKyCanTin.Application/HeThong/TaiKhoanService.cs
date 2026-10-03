using FluentValidation;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.HeThong;

public sealed class TaiKhoanService(
    IAppDbContext db,
    IKiemTraQuyen kiemTraQuyen,
    IGhiNhatKy ghiNhatKy,
    IMatKhauHasher matKhauHasher,
    IClock clock,
    ICurrentUser currentUser,
    IValidator<TaoTaiKhoanRequest> validator,
    KiemTraConQuanTri kiemTraConQuanTri) : ITaiKhoanService
{
    public const string LoiTrungTenDangNhap = "Tên đăng nhập đã tồn tại.";
    public const string LoiCanBoDaCoTaiKhoan = "Cán bộ này đã có tài khoản đang hoạt động.";
    public const string LoiTrungTaiKhoan =
        "Không tạo được tài khoản: tên đăng nhập đã tồn tại hoặc cán bộ đã có tài khoản đang hoạt động.";
    public const string LoiCanBoKhongHopLe = "Cán bộ không tồn tại hoặc đã ngừng công tác.";
    public const string LoiVaiTroKhongHopLe = "Vai trò không hợp lệ.";
    public const string LoiKhongTimThayTaiKhoan = "Không tìm thấy tài khoản.";
    public const string LoiKhongTheTuNgung = "Không thể ngừng hoạt động tài khoản đang đăng nhập.";

    public async Task<IReadOnlyList<TaiKhoanDto>> LayDanhSachAsync(CancellationToken ct = default)
    {
        var now = clock.Now;
        var danhSachNguoiDung = await db.NguoiDung.AsNoTracking()
            .OrderBy(u => u.TenDangNhap)
            .ToListAsync(ct);

        var tenCanBo = await db.CanBo.AsNoTracking().ToDictionaryAsync(c => c.Id, c => c.HoTen, ct);
        var vaiTroTheoNguoiDung = (await (
                from nguoiDungVaiTro in db.NguoiDungVaiTro.AsNoTracking()
                join vaiTro in db.VaiTro.AsNoTracking() on nguoiDungVaiTro.VaiTroId equals vaiTro.Id
                select new { nguoiDungVaiTro.NguoiDungId, vaiTro.Id, vaiTro.Ten }).ToListAsync(ct))
            .GroupBy(v => v.NguoiDungId)
            .ToDictionary(v => v.Key, v => v.OrderBy(x => x.Id).ToList());

        return danhSachNguoiDung.Select(nguoiDung =>
        {
            var vaiTro = vaiTroTheoNguoiDung.GetValueOrDefault(nguoiDung.Id, []);
            return new TaiKhoanDto(
                nguoiDung.Id,
                nguoiDung.TenDangNhap,
                nguoiDung.CanBoId is { } canBoId ? tenCanBo.GetValueOrDefault(canBoId) : null,
                string.Join(", ", vaiTro.Select(v => v.Ten)),
                vaiTro.Select(v => v.Id).ToList(),
                nguoiDung.DangHoatDong,
                nguoiDung.DangBiKhoa(now),
                nguoiDung.PhaiDoiMatKhau);
        }).ToList();
    }

    public async Task<IReadOnlyList<CanBoDto>> LayCanBoDeTaoTaiKhoanAsync(CancellationToken ct = default) =>
        await db.CanBo.AsNoTracking()
            .Where(canBo => canBo.DangCongTac
                && !db.NguoiDung.Any(nguoiDung => nguoiDung.CanBoId == canBo.Id && nguoiDung.DangHoatDong))
            .OrderBy(canBo => canBo.HoTen)
            .ThenBy(canBo => canBo.MaCanBo)
            .Select(canBo => new CanBoDto(
                canBo.Id, canBo.MaCanBo, canBo.HoTen, canBo.ChucVu, canBo.LaQuanGiao, canBo.DangCongTac, canBo.RowVer))
            .ToListAsync(ct);

    public async Task<KetQuaTaoTaiKhoan> TaoAsync(TaoTaiKhoanRequest request, CancellationToken ct = default)
    {
        // Authorization first, before any read or transaction, so a refused call writes nothing at all.
        await kiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua, ct);
        await KiemTraHopLeAsync(request, ct);

        var tenDangNhap = request.TenDangNhap.Trim();
        if (await db.NguoiDung.AnyAsync(nguoiDung => nguoiDung.TenDangNhap == tenDangNhap, ct))
            throw new LoiNghiepVuException(LoiTrungTenDangNhap);
        if (!await db.CanBo.AnyAsync(canBo => canBo.Id == request.CanBoId && canBo.DangCongTac, ct))
            throw new LoiNghiepVuException(LoiCanBoKhongHopLe);
        if (await db.NguoiDung.AnyAsync(nguoiDung => nguoiDung.CanBoId == request.CanBoId && nguoiDung.DangHoatDong, ct))
            throw new LoiNghiepVuException(LoiCanBoDaCoTaiKhoan);

        var vaiTroIds = request.VaiTroIds.Distinct().ToList();
        var vaiTroCoThat = await db.VaiTro.Where(vaiTro => vaiTroIds.Contains(vaiTro.Id)).Select(vaiTro => vaiTro.Id).ToListAsync(ct);
        if (vaiTroCoThat.Count != vaiTroIds.Count)
            throw new LoiNghiepVuException(LoiVaiTroKhongHopLe);

        var matKhauTam = MatKhauTam.Tao();
        var nguoiDungMoi = new NguoiDung
        {
            TenDangNhap = tenDangNhap,
            CanBoId = request.CanBoId,
            MatKhauHash = matKhauHasher.Hash(matKhauTam),
            DangHoatDong = true,
            PhaiDoiMatKhau = true,
        };

        await using var giaoDich = await db.BeginTransactionAsync(ct);
        db.NguoiDung.Add(nguoiDungMoi);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (TrungGiaTriDuyNhatException ex)
        {
            // The filtered unique index or the sign-in name index rejected a concurrent create.
            throw new LoiNghiepVuException(LoiTrungTaiKhoan, ex);
        }

        foreach (var vaiTroId in vaiTroIds)
            db.NguoiDungVaiTro.Add(new NguoiDungVaiTro { NguoiDungId = nguoiDungMoi.Id, VaiTroId = vaiTroId });
        await db.SaveChangesAsync(ct);
        await giaoDich.CommitAsync(ct);

        return new KetQuaTaoTaiKhoan(nguoiDungMoi.Id, matKhauTam);
    }

    public async Task CapNhatVaiTroAsync(int nguoiDungId, IReadOnlyCollection<int> vaiTroIds, CancellationToken ct = default)
    {
        await kiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua, ct);

        var vaiTroMoi = vaiTroIds.Distinct().ToList();
        if (vaiTroMoi.Count == 0)
            throw new LoiNghiepVuException(LoiVaiTroKhongHopLe);

        // Serializable: the guard and the write must be one atomic unit, or two administrators could demote each
        // other at the same moment and both succeed.
        await using var giaoDich = await db.BeginTransactionAsync(MucDoCoLapGiaoDich.TuanTu, ct);

        _ = await db.NguoiDung.SingleOrDefaultAsync(nguoiDung => nguoiDung.Id == nguoiDungId, ct)
            ?? throw new LoiNghiepVuException(LoiKhongTimThayTaiKhoan);

        var vaiTroCoThat = await db.VaiTro.Where(vaiTro => vaiTroMoi.Contains(vaiTro.Id)).Select(vaiTro => vaiTro.Id).ToListAsync(ct);
        if (vaiTroCoThat.Count != vaiTroMoi.Count)
            throw new LoiNghiepVuException(LoiVaiTroKhongHopLe);

        await kiemTraConQuanTri.YeuCauKhiDoiVaiTroAsync(nguoiDungId, vaiTroMoi, ct);

        var hienTai = await db.NguoiDungVaiTro.Where(v => v.NguoiDungId == nguoiDungId).ToListAsync(ct);
        var maTheoId = await db.VaiTro.AsNoTracking().ToDictionaryAsync(vaiTro => vaiTro.Id, vaiTro => vaiTro.Ma, ct);
        var maCu = hienTai.Select(v => maTheoId[v.VaiTroId]).Order(StringComparer.Ordinal).ToList();
        var tapMoi = vaiTroMoi.ToHashSet();

        db.NguoiDungVaiTro.RemoveRange(hienTai.Where(v => !tapMoi.Contains(v.VaiTroId)));
        foreach (var vaiTroId in vaiTroMoi.Where(id => hienTai.All(v => v.VaiTroId != id)))
            db.NguoiDungVaiTro.Add(new NguoiDungVaiTro { NguoiDungId = nguoiDungId, VaiTroId = vaiTroId });

        var maMoi = vaiTroMoi.Select(id => maTheoId[id]).Order(StringComparer.Ordinal).ToList();
        await db.SaveChangesAsync(ct);

        // The join rows are not IAuditable, so the role change is logged explicitly with its before/after values.
        await ghiNhatKy.GhiAsync(
            HanhDong.Sua, "NguoiDung", nguoiDungId,
            new { VaiTro = maCu },
            new { VaiTro = maMoi }, ct);
        await giaoDich.CommitAsync(ct);
    }

    public async Task NgungHoatDongAsync(int nguoiDungId, CancellationToken ct = default)
    {
        await kiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua, ct);
        if (currentUser.NguoiDungId == nguoiDungId)
            throw new LoiNghiepVuException(LoiKhongTheTuNgung);

        await using var giaoDich = await db.BeginTransactionAsync(MucDoCoLapGiaoDich.TuanTu, ct);
        var nguoiDung = await db.NguoiDung.SingleOrDefaultAsync(u => u.Id == nguoiDungId, ct)
            ?? throw new LoiNghiepVuException(LoiKhongTimThayTaiKhoan);

        await kiemTraConQuanTri.YeuCauKhiNgungHoatDongAsync(nguoiDungId, ct);

        nguoiDung.DangHoatDong = false;
        await db.SaveChangesAsync(ct);
        await giaoDich.CommitAsync(ct);
    }

    public async Task KichHoatLaiAsync(int nguoiDungId, CancellationToken ct = default)
    {
        await kiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua, ct);

        await using var giaoDich = await db.BeginTransactionAsync(ct);
        var nguoiDung = await db.NguoiDung.SingleOrDefaultAsync(u => u.Id == nguoiDungId, ct)
            ?? throw new LoiNghiepVuException(LoiKhongTimThayTaiKhoan);

        // The person may have received a newer active account while this one was off.
        if (nguoiDung.CanBoId is { } canBoId
            && await db.NguoiDung.AnyAsync(u => u.CanBoId == canBoId && u.DangHoatDong && u.Id != nguoiDungId, ct))
            throw new LoiNghiepVuException(LoiCanBoDaCoTaiKhoan);

        nguoiDung.DangHoatDong = true;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (TrungGiaTriDuyNhatException ex)
        {
            throw new LoiNghiepVuException(LoiCanBoDaCoTaiKhoan, ex);
        }

        await giaoDich.CommitAsync(ct);
    }

    public async Task MoKhoaAsync(int nguoiDungId, CancellationToken ct = default)
    {
        await kiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua, ct);

        var nguoiDung = await db.NguoiDung.SingleOrDefaultAsync(u => u.Id == nguoiDungId, ct)
            ?? throw new LoiNghiepVuException(LoiKhongTimThayTaiKhoan);

        nguoiDung.GhiNhanDangNhapDung();
        await db.SaveChangesAsync(ct);
    }

    public async Task<string> DatLaiMatKhauAsync(int nguoiDungId, CancellationToken ct = default)
    {
        await kiemTraQuyen.YeuCauAsync(MaQuyen.HT.Sua, ct);

        var nguoiDung = await db.NguoiDung.SingleOrDefaultAsync(u => u.Id == nguoiDungId, ct)
            ?? throw new LoiNghiepVuException(LoiKhongTimThayTaiKhoan);

        var matKhauTam = MatKhauTam.Tao();
        await using var giaoDich = await db.BeginTransactionAsync(ct);
        nguoiDung.MatKhauHash = matKhauHasher.Hash(matKhauTam);
        nguoiDung.PhaiDoiMatKhau = true;
        nguoiDung.GhiNhanDangNhapDung();
        await db.SaveChangesAsync(ct);

        // The interceptor logs the flag's false → true change; this row makes the action itself unmistakable.
        await ghiNhatKy.GhiAsync(
            HanhDong.Sua, "NguoiDung", nguoiDungId,
            new { SuKien = SuKienTaiKhoan.DatLaiMatKhau }, ct);
        await giaoDich.CommitAsync(ct);

        return matKhauTam;
    }

    private async Task KiemTraHopLeAsync(TaoTaiKhoanRequest request, CancellationToken ct)
    {
        var ketQua = await validator.ValidateAsync(request, ct);
        if (!ketQua.IsValid)
            throw new LoiNghiepVuException(string.Join('\n', ketQua.Errors.Select(e => e.ErrorMessage)));
    }
}
