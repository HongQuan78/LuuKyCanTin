using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.DanhMuc;

internal sealed class CanBoConfiguration : AuditableEntityConfiguration<CanBo>
{
    protected override void CauHinhRieng(EntityTypeBuilder<CanBo> builder)
    {
        builder.ToTable("CanBo");

        builder.Property(e => e.MaCanBo).HasMaxLength(LuuCanBoRequestValidator.DoDaiMaCanBo).IsUnicode(false);
        builder.HasIndex(e => e.MaCanBo).IsUnique();

        builder.Property(e => e.HoTen).HasMaxLength(LuuCanBoRequestValidator.DoDaiHoTen).UseCollation(AppDbContext.CollationTimKiem);
        builder.HasIndex(e => e.HoTen).HasDatabaseName("IX_CanBo_HoTen");

        builder.Property(e => e.ChucVu).HasMaxLength(LuuCanBoRequestValidator.DoDaiChucVu);

        builder.Property(e => e.LaQuanGiao).HasDefaultValue(false);
        // EF leaves a property equal to its sentinel out of the INSERT. With the default sentinel (false), adding
        // someone who has already left would silently store the database default, true.
        builder.Property(e => e.DangCongTac).HasDefaultValue(true).HasSentinel(true);
    }
}
