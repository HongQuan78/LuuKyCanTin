using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.LuuKy;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.LuuKy;

internal sealed class ChungTuLuuKyConfiguration : AuditableEntityConfiguration<ChungTuLuuKy>
{
    protected override void CauHinhRieng(EntityTypeBuilder<ChungTuLuuKy> builder)
    {
        builder.ToTable("ChungTuLuuKy", t =>
        {
            t.HasCheckConstraint("CK_ChungTuLuuKy_NghiepVu_LoaiPhieu", "[NghiepVu] / 10 = [LoaiPhieu]");
            t.HasCheckConstraint("CK_ChungTuLuuKy_ChuyenKhoan", "[HinhThuc] <> 2 OR [SoTaiKhoanNguoiGui] IS NOT NULL");
            t.HasCheckConstraint("CK_ChungTuLuuKy_SoTien", "[SoTien] > 0");
            t.HasCheckConstraint(
                "CK_ChungTuLuuKy_Huy",
                "[TrangThai] <> 3 OR ([LyDoHuy] IS NOT NULL AND [NgayHuy] IS NOT NULL AND [NguoiHuyId] IS NOT NULL)");
        });

        builder.Property(e => e.SoChungTu).HasMaxLength(20).IsUnicode(false).IsRequired();
        builder.HasIndex(e => e.SoChungTu).IsUnique();

        builder.Property(e => e.HoTenDoiTuong).HasMaxLength(100).IsRequired();
        builder.Property(e => e.NguoiGuiHoTen).HasMaxLength(100);
        builder.Property(e => e.QuanHe).HasMaxLength(50);
        builder.Property(e => e.SoPhieuGoc).HasMaxLength(30).IsUnicode(false);
        builder.Property(e => e.SoTaiKhoanNguoiGui).HasMaxLength(30).IsUnicode(false);
        builder.Property(e => e.NoiDung).HasMaxLength(500);
        builder.Property(e => e.SoTienBangChu).HasMaxLength(300).IsRequired();
        builder.Property(e => e.LyDoHuy).HasMaxLength(300);
        builder.Property(e => e.NgayHuy).HasColumnType("datetime2(0)");
        builder.Property(e => e.SoLanIn).HasDefaultValue((short)0);

        builder.HasEnumCheck(e => e.LoaiPhieu);
        builder.HasEnumCheck(e => e.NghiepVu);
        builder.HasEnumCheck(e => e.HinhThuc);
        builder.HasEnumCheck(e => e.TrangThai);
        builder.HasEnumCheck(e => e.LoaiDoiTuong);

        builder.HasOne<DoiTuong>().WithMany().HasForeignKey(e => e.DoiTuongId).OnDelete(DeleteBehavior.Restrict);

        // Statement scanning (per detainee, by date) and the posted-documents-by-date reports (Epic 6).
        builder.HasIndex(e => new { e.DoiTuongId, e.NgayChungTu, e.Id })
            .IncludeProperties(e => new { e.LoaiPhieu, e.SoTien, e.TrangThai });
        builder.HasIndex(e => e.NgayChungTu).HasFilter("[TrangThai] = 2");
    }
}
