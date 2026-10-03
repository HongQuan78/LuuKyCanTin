using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

internal sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermission");
        builder.HasKey(v => new { v.RoleId, v.PermissionId });

        builder.HasOne<Role>().WithMany().HasForeignKey(v => v.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Permission>().WithMany().HasForeignKey(v => v.PermissionId).OnDelete(DeleteBehavior.Restrict);

        var idByCode = PermissionCodes.All.ToDictionary(q => q.Code, q => q.Id, StringComparer.Ordinal);
        builder.HasData(DefaultRoles.All
            .SelectMany(role => role.Permission.Select(code => new { RoleId = role.Id, PermissionId = idByCode[code] }))
            .ToArray());
    }
}
