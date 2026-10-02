using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using LuuKyCanTin.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

internal sealed class VaiTroConfiguration : AuditableEntityConfiguration<VaiTro>
{
    protected override void ConfigureEntity(EntityTypeBuilder<VaiTro> builder)
    {
        builder.ToTable("VaiTro");

        builder.Property(v => v.Ma).HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.HasIndex(v => v.Ma).IsUnique();
        builder.Property(v => v.Ten).HasMaxLength(100).IsRequired();

        builder.HasData(VaiTroMacDinh.TatCa
            .Select(v => new VaiTro
            {
                Id = v.Id,
                Ma = v.Ma,
                Ten = v.Ten,
                NgayTao = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Unspecified),
                NguoiTaoId = 0,
            })
            .ToArray());
    }
}
