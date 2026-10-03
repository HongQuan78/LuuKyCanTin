using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Administration;

/// <summary>Changes the password of the signed-in user (FR1). The current password is always required.</summary>
public sealed class ChangePasswordService(
    IUserStore userStore,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser,
    IAuditLogWriter auditLog,
    FailedSignInService failedSignIns)
{
    public const string NotSignedInMessage = "Bạn chưa đăng nhập.";
    public const string AccountNotFoundMessage = "Không tìm thấy tài khoản.";

    /// <exception cref="BusinessRuleException">A wrong current password, a weak new password or a mismatch.</exception>
    public async Task ChangePasswordAsync(string currentPassword, string newPassword, string confirmation, CancellationToken ct = default)
    {
        if (currentUser.UserId is not { } userId)
            throw new BusinessRuleException(NotSignedInMessage);

        var user = await userStore.FindByIdAsync(userId, ct)
            ?? throw new BusinessRuleException(AccountNotFoundMessage);

        if (!passwordHasher.Verify(currentPassword, user.PasswordHash))
        {
            // A wrong current password counts toward lockout, exactly as a wrong password at sign-in.
            var justLocked = await failedSignIns.RecordAsync(user, ct);
            throw new BusinessRuleException(justLocked ? SignInService.AccountLockedMessage : SignInService.InvalidCredentialsMessage);
        }

        var violations = PasswordPolicy.Validate(newPassword);
        if (violations.Count > 0)
            throw new BusinessRuleException(string.Join('\n', violations));
        if (string.Equals(newPassword, currentPassword, StringComparison.Ordinal))
            throw new BusinessRuleException(PasswordPolicy.SameAsCurrentMessage);
        if (!string.Equals(newPassword, confirmation, StringComparison.Ordinal))
            throw new BusinessRuleException(PasswordPolicy.ConfirmationMismatchMessage);

        user.ChangePassword(passwordHasher.Hash(newPassword));
        await userStore.SaveAsync(user, ct);
        await auditLog.WriteAsync(
            AuditAction.SignIn, "User", user.Id,
            new { Event = SignInEvent.PasswordChanged, UserName = user.UserName }, ct);
    }
}
