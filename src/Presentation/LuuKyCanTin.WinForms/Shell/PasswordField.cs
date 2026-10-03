namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The three password boxes of the change-password dialog, so an error lands under the field it concerns.</summary>
public enum PasswordField : byte
{
    Current = 1,
    New = 2,
    Confirmation = 3,
}
