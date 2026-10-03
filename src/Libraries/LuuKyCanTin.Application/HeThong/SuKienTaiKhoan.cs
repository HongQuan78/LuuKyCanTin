namespace LuuKyCanTin.Application.HeThong;

/// <summary>The "SuKien" value in the payload of an account-administration audit row.</summary>
public static class SuKienTaiKhoan
{
    /// <summary>
    /// A reset changes only the excluded hash plus the forced-change flag, so it also gets an explicit event row
    /// that names the action, and the audit viewer shows it unmistakably.
    /// </summary>
    public const string DatLaiMatKhau = "DatLaiMatKhau";
}
