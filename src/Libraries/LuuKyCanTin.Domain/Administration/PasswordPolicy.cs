namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// The password-strength rules (FR1): at least 8 characters with an upper-case letter, a lower-case letter and a
/// digit, and different from the current password. A pure function with no dependencies, so the rules are
/// unit-tested without a UI; the change-password checklist and the service both go through <see cref="Evaluate"/>.
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

    /// <summary>Checks every rule, in display order, so a checklist can tick each one as the user types.</summary>
    /// <remarks>Uses <see cref="char.IsUpper"/> and <see cref="char.IsLower"/>, so Đ and ă count as letters.</remarks>
    public static IReadOnlyList<PasswordRuleResult> Evaluate(string? newPassword, string? currentPassword) =>
    [
        new(PasswordRule.MinLength, newPassword is { Length: >= MinLength }),
        new(PasswordRule.UpperCase, newPassword is not null && newPassword.Any(char.IsUpper)),
        new(PasswordRule.LowerCase, newPassword is not null && newPassword.Any(char.IsLower)),
        new(PasswordRule.Digit, newPassword is not null && newPassword.Any(char.IsDigit)),
        new(PasswordRule.DifferentFromCurrent, !string.Equals(newPassword ?? "", currentPassword ?? "", StringComparison.Ordinal)),
    ];

    /// <summary>Returns every strength rule the new password breaks; an empty list means it is accepted.</summary>
    public static IReadOnlyList<string> Validate(string? newPassword) =>
        Evaluate(newPassword, currentPassword: null)
            .Where(r => !r.IsSatisfied && r.Rule != PasswordRule.DifferentFromCurrent)
            .Select(r => ToViolationMessage(r.Rule))
            .ToList();

    public static string ToViolationMessage(PasswordRule rule) => rule switch
    {
        PasswordRule.MinLength => TooShortMessage,
        PasswordRule.UpperCase => MissingUpperCaseMessage,
        PasswordRule.LowerCase => MissingLowerCaseMessage,
        PasswordRule.Digit => MissingDigitMessage,
        PasswordRule.DifferentFromCurrent => SameAsCurrentMessage,
        _ => throw new ArgumentOutOfRangeException(nameof(rule), rule, "Unknown password rule."),
    };
}
