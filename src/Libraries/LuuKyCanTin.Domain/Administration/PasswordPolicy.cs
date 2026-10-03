namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// The password-strength rules (FR1): at least 8 characters with an upper-case letter, a lower-case letter and a
/// digit. A pure function with no dependencies, so the rules are unit-tested without a UI.
/// </summary>
public static class PasswordPolicy
{
    public const int MinLength = 8;

    public const string TooShortMessage = "Mật khẩu phải có ít nhất 8 ký tự";
    public const string MissingUpperCaseMessage = "Mật khẩu phải có ít nhất một chữ in hoa";
    public const string MissingLowerCaseMessage = "Mật khẩu phải có ít nhất một chữ thường";
    public const string MissingDigitMessage = "Mật khẩu phải có ít nhất một chữ số";
    public const string SameAsCurrentMessage = "Mật khẩu mới phải khác mật khẩu hiện tại";
    public const string ConfirmationMismatchMessage = "Xác nhận mật khẩu không khớp";

    /// <summary>Returns every rule the new password breaks; an empty list means it is accepted.</summary>
    /// <remarks>Uses <see cref="char.IsUpper"/> and <see cref="char.IsLower"/>, so Đ and ă count as letters.</remarks>
    public static IReadOnlyList<string> Validate(string? newPassword)
    {
        var violations = new List<string>();
        if (newPassword is null || newPassword.Length < MinLength)
            violations.Add(TooShortMessage);
        if (newPassword is null || !newPassword.Any(char.IsUpper))
            violations.Add(MissingUpperCaseMessage);
        if (newPassword is null || !newPassword.Any(char.IsLower))
            violations.Add(MissingLowerCaseMessage);
        if (newPassword is null || !newPassword.Any(char.IsDigit))
            violations.Add(MissingDigitMessage);

        return violations;
    }
}
