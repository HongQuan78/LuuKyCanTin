using LuuKyCanTin.Domain.HeThong;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.HeThong;

internal sealed class NguoiDungVaiTroConfiguration : IEntityTypeConfiguration<NguoiDungVaiTro>
{
    public void Configure(EntityTypeBuilder<NguoiDungVaiTro> builder)
    {
        builder.ToTable("NguoiDungVaiTro");
        builder.HasKey(v => new { v.NguoiDungId, v.VaiTroId });

        builder.HasOne<NguoiDung>().WithMany().HasForeignKey(v => v.NguoiDungId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<VaiTro>().WithMany().HasForeignKey(v => v.VaiTroId).OnDelete(DeleteBehavior.Restrict);

        // The seeded admin's grant is inserted by the AddVaiTroQuyen migration, not by HasData: the admin row is
        // created by a raw migration insert, so EnsureCreated (which only applies model seed data) can't reference it.
    }
}
