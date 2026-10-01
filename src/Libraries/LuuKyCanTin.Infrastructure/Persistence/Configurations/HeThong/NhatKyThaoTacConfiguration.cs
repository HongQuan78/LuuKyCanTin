using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

/// <summary>
/// No FK from NguoiDungId: log rows must outlive any user clean-up, and inserts into the log stay cheap.
/// </summary>
internal sealed class NhatKyThaoTacConfiguration : IEntityTypeConfiguration<NhatKyThaoTac>
{
    public void Configure(EntityTypeBuilder<NhatKyThaoTac> builder)
    {
        builder.ToTable("NhatKyThaoTac");

        builder.Property(e => e.MayTram).HasMaxLength(100);
        builder.Property(e => e.HanhDong).HasConversion<string>().HasMaxLength(20).IsUnicode(false);
        builder.HasEnumCheck(e => e.HanhDong);
        builder.Property(e => e.TenBang).HasMaxLength(50).IsUnicode(false);

        builder.HasIndex(e => new { e.TenBang, e.BanGhiId });
        builder.HasIndex(e => e.ThoiDiem);
    }
}
