namespace LuuKyCanTin.Application.Administration;

/// <summary>Why a sign-in ended the way it did. Only <see cref="Succeeded"/> opens the shell directly.</summary>
public enum SignInStatus
{
    Succeeded = 1,
    PasswordChangeRequired = 2,
    InvalidCredentials = 3,
    AccountLocked = 4,
    AccountInactive = 5,
}
