using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Administration;

/// <summary>
/// Answers from the database on every call, never from a cache: a revoked permission or a deactivated account
/// stops working at the next check.
/// </summary>
internal sealed class PermissionChecker(AppDbContext db, ICurrentUser currentUser) : IPermissionChecker
{
    public async Task RequireAsync(string permissionCode, CancellationToken ct = default)
    {
        if (currentUser.UserId is not { } userId)
            throw new PermissionDeniedException(permissionCode);

        var hasPermission = await (from user in db.User
                             join userRole in db.UserRole on user.Id equals userRole.UserId
                             join rolePermission in db.RolePermission on userRole.RoleId equals rolePermission.RoleId
                             join permission in db.Permission on rolePermission.PermissionId equals permission.Id
                             where user.Id == userId
                                 && user.IsActive
                                 && permission.Code == permissionCode
                             select permission.Id).AnyAsync(ct);

        if (!hasPermission)
            throw new PermissionDeniedException(permissionCode);
    }
}
