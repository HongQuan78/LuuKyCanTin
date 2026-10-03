using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// Sign-in with lockout and the forced first change (FR1). Returns a result instead of throwing for every
/// credential outcome; only unexpected infrastructure failures escape.
/// </summary>
public sealed class SignInService(
    IUserStore userStore,
    IPasswordHasher passwordHasher,
    ICurrentUserSession session,
    IAuditLogWriter auditLog,
    IClock clock,
    FailedSignInService failedSignIns)
{
    public const string InvalidCredentialsMessage = "Tên đăng nhập hoặc mật khẩu không đúng.";
    public const string AccountLockedMessage = "Tài khoản đã bị khoá, liên hệ quản trị viên.";
    public const string PasswordChangeRequiredMessage = "Tài khoản cần đổi mật khẩu, vui lòng đăng nhập lại.";

    // A parseable PBKDF2 string that can never match a password. Verifying it for an unknown user keeps the
    // response time close to a wrong password, so the form reveals whether an account exists (AC 2).
    private const string DummyPasswordHash =
        "PBKDF2-SHA256$600000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

    public async Task<SignInResult> SignInAsync(string userName, string password, CancellationToken ct = default)
    {
        var user = await userStore.FindByUserNameAsync(userName.Trim(), ct);
        if (user is null)
        {
            passwordHasher.Verify(password, DummyPasswordHash);
            return SignInResult.Fail(SignInStatus.InvalidCredentials, InvalidCredentialsMessage);
        }

        // A locked or inactive account is refused before the password is checked, so nobody can keep testing
        // passwords against it. Both get the locked message, so a deactivated person learns nothing new.
        if (!user.IsActive)
            return SignInResult.Fail(SignInStatus.AccountInactive, AccountLockedMessage);
        if (user.IsLocked(clock.Now))
            return SignInResult.Fail(SignInStatus.AccountLocked, AccountLockedMessage);

        if (!passwordHasher.Verify(password, user.PasswordHash))
        {
            return await failedSignIns.RecordAsync(user, ct)
                ? SignInResult.Fail(SignInStatus.AccountLocked, AccountLockedMessage)
                : SignInResult.Fail(SignInStatus.InvalidCredentials, InvalidCredentialsMessage);
        }

        // The session is set even when a change is forced: the shell stays closed until the change succeeds.
        user.RecordSuccessfulSignIn();
        await userStore.SaveAsync(user, ct);
        // The built-in admin has no officer record; everywhere else the name is loaded, and the fallback is rare.
        var officerFullName = user.OfficerId is { } officerId
            ? await userStore.GetOfficerFullNameAsync(officerId, ct)
            : null;
        // The one permission read of the session: the shell's UI uses this cache, writes re-check the database.
        var permissionCodes = await userStore.GetPermissionCodesAsync(user.Id, ct);
        session.SignIn(user.Id, user.UserName, user.OfficerId, officerFullName ?? user.UserName, permissionCodes);
        await auditLog.WriteAsync(
            AuditAction.SignIn, "User", user.Id,
            new { Event = SignInEvent.SignIn, UserName = user.UserName }, ct);

        return user.MustChangePassword ? SignInResult.PasswordChangeRequired() : SignInResult.Ok();
    }

    /// <summary>
    /// Re-verifies the signed-in user's password to unlock a locked session (story 2.7). It is the sign-in flow
    /// without the session swap: the same account stays signed in. A wrong password counts toward the lockout of
    /// story 2.2, and an inactive or locked account can never unlock. A successful attempt writes the unlock event.
    /// </summary>
    public async Task<SignInResult> ReauthenticateAsync(string password, CancellationToken ct = default)
    {
        if (session.UserId is not { } userId)
            return SignInResult.Fail(SignInStatus.AccountInactive, AccountLockedMessage);

        var user = await userStore.FindByIdAsync(userId, ct);
        if (user is null || !user.IsActive)
            return SignInResult.Fail(SignInStatus.AccountInactive, AccountLockedMessage);
        if (user.IsLocked(clock.Now))
            return SignInResult.Fail(SignInStatus.AccountLocked, AccountLockedMessage);

        if (!passwordHasher.Verify(password, user.PasswordHash))
        {
            return await failedSignIns.RecordAsync(user, ct)
                ? SignInResult.Fail(SignInStatus.AccountLocked, AccountLockedMessage)
                : SignInResult.Fail(SignInStatus.InvalidCredentials, InvalidCredentialsMessage);
        }

        user.RecordSuccessfulSignIn();
        await userStore.SaveAsync(user, ct);
        // Same semantics as sign-in: a forced change blocks the session, so unlocking must return to the login form.
        if (user.MustChangePassword)
            return SignInResult.PasswordChangeRequired();

        await auditLog.WriteAsync(
            AuditAction.SignIn, "User", user.Id, new { Event = SignInEvent.UnlockSession }, ct);

        return SignInResult.Ok();
    }

    /// <summary>
    /// Writes the lock event for the signed-in user. The caller decides when to cover the screen; this only records
    /// why it locked (automatic idle timeout or the manual lock command).
    /// </summary>
    public async Task LockSessionAsync(SessionLockKind kind, CancellationToken ct = default)
    {
        if (session.UserId is not { } userId)
            return;

        await auditLog.WriteAsync(
            AuditAction.SignIn, "User", userId, new { Event = SignInEvent.LockSession, Kind = kind.ToCode() }, ct);
    }

    /// <summary>Writes the sign-out event and clears the session; does nothing when nobody is signed in.</summary>
    public async Task SignOutAsync(CancellationToken ct = default)
    {
        if (session.UserId is not { } userId)
            return;

        await auditLog.WriteAsync(
            AuditAction.SignIn, "User", userId,
            new { Event = SignInEvent.SignOut, UserName = session.UserName }, ct);
        session.SignOut();
    }
}
