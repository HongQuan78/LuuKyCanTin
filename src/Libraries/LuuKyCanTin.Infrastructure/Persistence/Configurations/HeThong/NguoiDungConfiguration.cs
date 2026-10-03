using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

internal sealed class NguoiDungConfiguration : AuditableEntityConfiguration<NguoiDung>
{
    protected override void CauHinhRieng(EntityTypeBuilder<NguoiDung> builder)
    {
        builder.ToTable("NguoiDung");

        builder.Property(e => e.TenDangNhap).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.TenDangNhap).IsUnique();

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
