using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Administration;

public static class PasswordRuleDisplayExtensions
{
    /// <summary>The checklist label in the change-password dialog.</summary>
    public static string ToDisplayText(this PasswordRule value) => value switch
    {
        PasswordRule.MinLength => "Ít nhất 8 ký tự",
        PasswordRule.UpperCase => "Có chữ hoa",
        PasswordRule.LowerCase => "Có chữ thường",
        PasswordRule.Digit => "Có chữ số",
        PasswordRule.DifferentFromCurrent => "Khác mật khẩu hiện tại",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown password rule."),
    };
}
