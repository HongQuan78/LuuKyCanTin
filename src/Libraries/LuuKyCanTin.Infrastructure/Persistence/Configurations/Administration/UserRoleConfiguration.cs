using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole");
        builder.HasKey(v => new { v.UserId, v.RoleId });

        builder.HasOne<User>().WithMany().HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Role>().WithMany().HasForeignKey(v => v.RoleId).OnDelete(DeleteBehavior.Restrict);

        // The seeded admin's grant is inserted by the AddVaiTroQuyen migration, not by HasData: the admin row is
        // created by a raw migration insert, so EnsureCreated (which only applies model seed data) can't reference it.
    }
}
