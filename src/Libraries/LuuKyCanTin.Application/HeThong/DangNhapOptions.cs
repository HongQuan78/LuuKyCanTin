namespace LuuKyCanTin.Application.HeThong;

/// <summary>
/// Sign-in settings bound from the "DangNhap" section of appsettings.json. Workstations share one database, so
/// every machine has to use the same lock period.
/// </summary>
public sealed class DangNhapOptions
{
    public const string SectionName = "DangNhap";

    /// <summary>Minutes an account stays locked after 5 failed attempts. 0 means only an administrator can unlock it.</summary>
    public int ThoiGianKhoaPhut { get; set; } = 15;

    public TimeSpan? ThoiGianKhoa => ThoiGianKhoaPhut <= 0 ? null : TimeSpan.FromMinutes(ThoiGianKhoaPhut);
}
