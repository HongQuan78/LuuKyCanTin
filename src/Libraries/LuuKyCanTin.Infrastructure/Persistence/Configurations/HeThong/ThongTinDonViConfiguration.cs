using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

internal sealed class ThongTinDonViConfiguration : AuditableEntityConfiguration<ThongTinDonVi>
{
    // One reference-data row exists on every install; a fixed timestamp keeps the seed deterministic.
    private static readonly DateTime NgayTaoSeed = new(2026, 1, 1);

    protected override void CauHinhRieng(EntityTypeBuilder<ThongTinDonVi> builder)
    {
        builder.ToTable("ThongTinDonVi", t => t.HasCheckConstraint("CK_ThongTinDonVi_Id", "[Id] = 1"));

        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.TenCoQuanChuQuan).HasMaxLength(200);
        builder.Property(e => e.TenDonVi).HasMaxLength(200).IsRequired();
        builder.Property(e => e.DiaChi).HasMaxLength(300).IsRequired();

        // An empty row for the admin to fill (DB design, seed data). The demo seeder fills it on a dev machine.
        builder.HasData(new ThongTinDonVi
        {
            Id = 1,
            TenCoQuanChuQuan = null,
            TenDonVi = "",
            DiaChi = "",
            NgayTao = NgayTaoSeed,
            NguoiTaoId = 0,
        });
    }
}
