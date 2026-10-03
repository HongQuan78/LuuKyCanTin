namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The "Event" value in the payload of a sign-in audit row (<c>AuditAction.SignIn</c> holds all of them). The values
/// are codes stored in the append-only audit log, so they keep the spelling existing rows already use.
/// </summary>
public static class SignInEvent
{
    public const string SignIn = "DangNhap";
    public const string FailedSignIn = "DangNhapSai";
    public const string AccountLocked = "KhoaTaiKhoan";
    public const string SignOut = "DangXuat";
    public const string PasswordChanged = "DoiMatKhau";
    public const string LockSession = "KhoaPhien";
    public const string UnlockSession = "MoKhoaPhien";
}
