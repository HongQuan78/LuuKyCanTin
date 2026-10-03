using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

internal sealed class NguoiDungConfiguration : AuditableEntityConfiguration<NguoiDung>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NguoiDung> builder)
    {
        // Only the built-in admin may exist without a staff record; every real person works under their own account.
        builder.ToTable("NguoiDung", t => t.HasCheckConstraint(
            "CK_NguoiDung_CanBoId", "[CanBoId] IS NOT NULL OR [TenDangNhap] = 'admin'"));

        builder.Property(e => e.TenDangNhap).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.TenDangNhap).IsUnique();

        builder.HasOne<CanBo>().WithMany().HasForeignKey(e => e.CanBoId).OnDelete(DeleteBehavior.Restrict);

        // One active account per person (NEN-09); inactive history is kept, so the filter allows it.
        builder.HasIndex(e => e.CanBoId)
            .IsUnique()
            .HasFilter("[DangHoatDong] = 1 AND [CanBoId] IS NOT NULL")
            .HasDatabaseName("UX_NguoiDung_CanBoId_DangHoatDong");

        // The hash is ASCII base64; KhongGhiNhatKy keeps it out of the audit-log JSON.
        builder.Property(e => e.MatKhauHash).HasMaxLength(200).IsUnicode(false).IsRequired();

        // The sentinel is the default itself, so a true value lets the database default apply and an explicit
        // false is still written (without it EF would treat false as "unset" only if it were the sentinel).
        builder.Property(e => e.DangHoatDong).HasDefaultValue(true).HasSentinel(true);
        builder.Property(e => e.PhaiDoiMatKhau).HasDefaultValue(false);
        builder.Property(e => e.SoLanSai).HasDefaultValue((byte)0);
        builder.Property(e => e.KhoaDen).HasColumnType("datetime2(0)");
    }
}
