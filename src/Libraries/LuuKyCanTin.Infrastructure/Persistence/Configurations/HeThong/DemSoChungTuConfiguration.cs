using LuuKyCanTin.Domain.HeThong;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

internal sealed class DemSoChungTuConfiguration : IEntityTypeConfiguration<DemSoChungTu>
{
    public void Configure(EntityTypeBuilder<DemSoChungTu> builder)
    {
        builder.ToTable("DemSoChungTu");

        builder.HasKey(e => new { e.LoaiChungTu, e.Nam });
        builder.Property(e => e.LoaiChungTu).HasMaxLength(10).IsUnicode(false);
        builder.Property(e => e.Nam).HasColumnType("smallint");
        builder.Property(e => e.TienTo).HasMaxLength(10).IsUnicode(false).IsRequired();
        builder.Property(e => e.SoHienTai).HasDefaultValue(0);

        // Only the receipt counter ships in the skeleton; later document types seed their own. Year rollover is Epic 4.
        builder.HasData(new DemSoChungTu { LoaiChungTu = "BNT", Nam = 2026, TienTo = "BNT", SoHienTai = 0 });
    }
}
