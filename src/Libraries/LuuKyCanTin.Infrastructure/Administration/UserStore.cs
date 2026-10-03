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
}
