namespace LuuKyCanTin.Domain.Administration;

/// <summary>One rule of <see cref="PasswordPolicy"/>, in the order the change-password checklist shows them.</summary>
public enum PasswordRule : byte
{
    MinLength = 1,
    UpperCase = 2,
    LowerCase = 3,
    Digit = 4,
    DifferentFromCurrent = 5,
}
