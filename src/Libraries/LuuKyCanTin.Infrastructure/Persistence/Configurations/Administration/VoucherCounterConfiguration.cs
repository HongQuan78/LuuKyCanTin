using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

internal sealed class VoucherCounterConfiguration : IEntityTypeConfiguration<VoucherCounter>
{
    public void Configure(EntityTypeBuilder<VoucherCounter> builder)
    {
        builder.ToTable("VoucherCounter");

        builder.HasKey(e => new { e.VoucherTypeCode, e.Year });
        builder.Property(e => e.VoucherTypeCode).HasMaxLength(10).IsUnicode(false);
        builder.Property(e => e.Year).HasColumnType("smallint");
        builder.Property(e => e.Prefix).HasMaxLength(10).IsUnicode(false).IsRequired();
        builder.Property(e => e.CurrentNumber).HasDefaultValue(0);

        // Only the receipt counter ships in the skeleton; later document types seed their own. Year rollover is Epic 4.
        builder.HasData(new VoucherCounter { VoucherTypeCode = "BNT", Year = 2026, Prefix = "BNT", CurrentNumber = 0 });
    }
}
