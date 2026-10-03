using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Administration;

internal sealed class UserStore(AppDbContext db) : IUserStore
{
    // Tracked on purpose: sign-in changes the failure counters and saves the same entity.
    public Task<User?> FindByUserNameAsync(string userName, CancellationToken ct = default) =>
        db.User.FirstOrDefaultAsync(u => u.UserName == userName, ct);

    public Task<User?> FindByIdAsync(int userId, CancellationToken ct = default) =>
        db.User.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task SaveAsync(User user, CancellationToken ct = default) =>
        ((IAppDbContext)db).SaveChangesAsync(ct);

    public Task ReloadAsync(User user, CancellationToken ct = default) =>
        db.Entry(user).ReloadAsync(ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
    {
        db.User.Add(user);
        await ((IAppDbContext)db).SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AccountRow>> GetAllWithRolesAsync(CancellationToken ct = default)
    {
        var users = await db.User.AsNoTracking().OrderBy(u => u.UserName).ToListAsync(ct);
        var officerNames = await db.Officer.AsNoTracking().ToDictionaryAsync(o => o.Id, o => o.FullName, ct);
        var rolesByUser = (await (
                from userRole in db.UserRole.AsNoTracking()
                join role in db.Role.AsNoTracking() on userRole.RoleId equals role.Id
                select new { userRole.UserId, Role = role }).ToListAsync(ct))
            .GroupBy(row => row.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<Role>)group.Select(row => row.Role).OrderBy(role => role.Id).ToList());

        return users.Select(user => new AccountRow(
            user,
            user.OfficerId is { } officerId ? officerNames.GetValueOrDefault(officerId) : null,
            rolesByUser.GetValueOrDefault(user.Id, []))).ToList();
    }

    public Task<bool> HasActiveAccountAsync(int officerId, int? excludedUserId = null, CancellationToken ct = default) =>
        db.User.AnyAsync(
            u => u.OfficerId == officerId && u.IsActive && (excludedUserId == null || u.Id != excludedUserId), ct);

    public Task<string?> GetOfficerFullNameAsync(int officerId, CancellationToken ct = default) =>
        db.Officer.Where(o => o.Id == officerId).Select(o => o.FullName).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<string>> GetPermissionCodesAsync(int userId, CancellationToken ct = default) =>
        await (from user in db.User
               join userRole in db.UserRole on user.Id equals userRole.UserId
               join rolePermission in db.RolePermission on userRole.RoleId equals rolePermission.RoleId
               join permission in db.Permission on rolePermission.PermissionId equals permission.Id
               where user.Id == userId && user.IsActive
               orderby permission.Id
               select permission.Code).Distinct().ToListAsync(ct);
}
