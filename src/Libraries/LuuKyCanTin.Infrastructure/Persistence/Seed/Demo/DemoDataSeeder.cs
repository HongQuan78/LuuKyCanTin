using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;

internal sealed class DemoDataSeeder(AppDbContext db, IMatKhauHasher matKhauHasher) : IDemoDataSeeder
{
    /// <summary>Password of every demo account. Development only; the admin still changes its own at first sign-in.</summary>
    public const string MatKhauDemo = "Demo@2026";

    private sealed record TaiKhoanDemo(
        string TenDangNhap, string MaCanBo, string HoTen, string ChucVu, string MaVaiTro, bool LaQuanGiao);

    // One account per standard role, so a tester can sign in and see each role's permissions.
    private static readonly IReadOnlyList<TaiKhoanDemo> TaiKhoanMacDinh =
    [
        new("luuky", "CB-LK", "Nguyễn Thị Lưu Ký", "Cán bộ theo dõi tiền lưu ký", MaVaiTro.LuuKy, false),
        new("cantin", "CB-CT", "Trần Văn Căn Tin", "Cán bộ căn tin", MaVaiTro.CanTin, false),
        new("quangiao", "CB-QG", "Lê Văn Quản Giáo", "Cán bộ quản giáo", MaVaiTro.QuanGiao, true),
        new("lanhdao", "CB-LD", "Phạm Văn Lãnh Đạo", "Chỉ huy phụ trách", MaVaiTro.LanhDao, false),
        new("ketoan", "CB-KT", "Hoàng Thị Kế Toán", "Kế toán đơn vị", MaVaiTro.KeToan, false),
    ];

    public async Task<DemoSeedDecision> NapDuLieuMauAsync(
        bool laMoiTruongPhatTrien, string? tenCoSoDuLieuXacNhan, CancellationToken ct = default)
    {
        var decision = DemoSeedPolicy.KiemTra(laMoiTruongPhatTrien, tenCoSoDuLieuXacNhan, db.Database.GetDbConnection().Database);
        if (decision != DemoSeedDecision.Allowed)
            return decision;

        await FillUnitInfoAsync(ct);
        await FillTaiKhoanDemoAsync(ct);
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

    // Idempotent: an account or staff code that exists is left alone, so re-running --seed-demo changes nothing.
    private async Task FillTaiKhoanDemoAsync(CancellationToken cancellationToken)
    {
        foreach (var taiKhoan in TaiKhoanMacDinh)
        {
            if (await db.NguoiDung.AnyAsync(u => u.TenDangNhap == taiKhoan.TenDangNhap, cancellationToken))
                continue;

            if (!await db.CanBo.AnyAsync(c => c.MaCanBo == taiKhoan.MaCanBo, cancellationToken))
            {
                db.CanBo.Add(new CanBo(taiKhoan.MaCanBo, taiKhoan.HoTen, taiKhoan.ChucVu, taiKhoan.LaQuanGiao));
                await db.SaveChangesAsync(cancellationToken);
            }

            var nguoiDung = new NguoiDung
            {
                TenDangNhap = taiKhoan.TenDangNhap,
                MatKhauHash = matKhauHasher.Hash(MatKhauDemo),
                DangHoatDong = true,
                PhaiDoiMatKhau = false,
            };
            db.NguoiDung.Add(nguoiDung);
            await db.SaveChangesAsync(cancellationToken);

            var vaiTroId = await db.VaiTro
                .Where(v => v.Ma == taiKhoan.MaVaiTro)
                .Select(v => v.Id)
                .SingleAsync(cancellationToken);
            db.NguoiDungVaiTro.Add(new NguoiDungVaiTro { NguoiDungId = nguoiDung.Id, VaiTroId = vaiTroId });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
