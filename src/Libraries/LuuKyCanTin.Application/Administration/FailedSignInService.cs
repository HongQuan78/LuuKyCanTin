using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// Records one failed password attempt: counts it toward lockout and writes the audit event. Shared by sign-in
/// and by change-password, because a wrong current password counts as a failed attempt too.
/// </summary>
public sealed class FailedSignInService(
    IUserStore userStore,
    IClock clock,
    SignInOptions options,
    IAuditLogWriter auditLog)
{
    /// <returns>True when this attempt just locked the account.</returns>
    public async Task<bool> RecordAsync(User user, CancellationToken ct = default)
    {
        var justLocked = user.RecordFailedSignIn(clock.Now, options.LockoutPeriod);
        try
        {
            await userStore.SaveAsync(user, ct);
        }
        catch (ConcurrencyConflictException)
        {
            // Two workstations failed the same account at once. Reload and re-apply this attempt once, so it is
            // never lost; a second conflict still reaches the caller.
            await userStore.ReloadAsync(user, ct);
            justLocked = user.RecordFailedSignIn(clock.Now, options.LockoutPeriod);
            await userStore.SaveAsync(user, ct);
        }

        var signInEvent = justLocked ? SignInEvent.AccountLocked : SignInEvent.FailedSignIn;
        await auditLog.WriteAsync(
            AuditAction.SignIn, "User", user.Id,
            new { Event = signInEvent, UserName = user.UserName }, ct);

        return justLocked;
    }
}
