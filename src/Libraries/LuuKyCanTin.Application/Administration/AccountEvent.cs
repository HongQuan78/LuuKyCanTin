namespace LuuKyCanTin.Application.Administration;

/// <summary>The "Event" value in the payload of an account-administration audit row.</summary>
public static class AccountEvent
{
    /// <summary>
    /// A reset changes only the excluded hash plus the forced-change flag, so it also gets an explicit event row
    /// that names the action, and the audit viewer shows it unmistakably. The value is stored data and stays
    /// Vietnamese.
    /// </summary>
    public const string ResetPassword = "DatLaiMatKhau";

    /// <summary>An unlock that changed no column still gets a row naming the action.</summary>
    public const string Unlock = "MoKhoa";
}
