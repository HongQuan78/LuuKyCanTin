using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using LuuKyCanTin.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

internal sealed class RoleConfiguration : AuditableEntityConfiguration<Role>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role");

        builder.Property(v => v.Code).HasMaxLength(30).IsUnicode(false).IsRequired();
        builder.HasIndex(v => v.Code).IsUnique();
        builder.Property(v => v.Name).HasMaxLength(100).IsRequired();

        builder.HasData(DefaultRoles.All
            .Select(v => new Role
            {
                Id = v.Id,
                Code = v.Code,
                Name = v.Name,
                CreatedAt = new DateTime(2026, 10, 2, 0, 0, 0, DateTimeKind.Unspecified),
                CreatedById = 0,
            })
            .ToArray());
    }
}
