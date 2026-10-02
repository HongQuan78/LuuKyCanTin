using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

internal sealed class VaiTroQuyenConfiguration : IEntityTypeConfiguration<VaiTroQuyen>
{
    public void Configure(EntityTypeBuilder<VaiTroQuyen> builder)
    {
        builder.ToTable("VaiTroQuyen");
        builder.HasKey(v => new { v.VaiTroId, v.QuyenId });

        builder.HasOne<VaiTro>().WithMany().HasForeignKey(v => v.VaiTroId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Quyen>().WithMany().HasForeignKey(v => v.QuyenId).OnDelete(DeleteBehavior.Restrict);

        var idTheoMa = MaQuyen.TatCa.ToDictionary(q => q.Ma, q => q.Id, StringComparer.Ordinal);
        builder.HasData(VaiTroMacDinh.TatCa
            .SelectMany(vaiTro => vaiTro.Quyen.Select(ma => new { VaiTroId = vaiTro.Id, QuyenId = idTheoMa[ma] }))
            .ToArray());
    }
}
