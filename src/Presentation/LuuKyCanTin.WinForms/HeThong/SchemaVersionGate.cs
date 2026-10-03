using LuuKyCanTin.Application.HeThong;
using Microsoft.Extensions.Logging;

namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>Decides whether the workstation may open its main form against the database it found.</summary>
internal static class SchemaVersionGate
{
    public const string LoiPhienBanKhongKhop = "Phiên bản cơ sở dữ liệu không khớp, vui lòng liên hệ quản trị viên.";
    public const string LoiKhongKetNoiDuoc = "Không kết nối được cơ sở dữ liệu, vui lòng liên hệ quản trị viên.";

    /// <returns>The message to show before exiting, or null when startup may continue.</returns>
    public static string? LayThongBaoChan(SchemaVersionCheckResult result) => result.Status switch
    {
        SchemaVersionStatus.Matches => null,
        SchemaVersionStatus.ConnectionFailed => LoiKhongKetNoiDuoc,
        _ => LoiPhienBanKhongKhop,
    };

    /// <summary>
    /// Workstations never migrate: they only check. On a mismatch or a failed connection it logs both versions,
    /// shows the message through <paramref name="hienLoi"/> and returns false, and startup must stop.
    /// </summary>
    public static async Task<bool> DuocPhepMoAsync(
        ISchemaVersionChecker checker, Action<string> hienLoi, ILogger logger, CancellationToken ct = default)
    {
        var result = await checker.KiemTraAsync(ct);
        var thongBao = LayThongBaoChan(result);
        if (thongBao is null)
            return true;

        logger.LogError("Database schema check failed ({Status}): expected migration {Expected}, database has {Actual}",
            result.Status, result.Expected, result.Actual ?? "(none)");
        hienLoi(thongBao);
        return false;
    }
}
