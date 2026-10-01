using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>Decides whether the workstation may open its main form against the database it found.</summary>
internal static class SchemaVersionGate
{
    public const string MismatchMessage = "Phiên bản cơ sở dữ liệu không khớp, vui lòng liên hệ quản trị viên.";
    public const string ConnectionFailedMessage = "Không kết nối được cơ sở dữ liệu, vui lòng liên hệ quản trị viên.";

    /// <returns>The message to show before exiting, or null when startup may continue.</returns>
    public static string? BlockingMessage(SchemaVersionCheckResult result) => result.Status switch
    {
        SchemaVersionStatus.Matches => null,
        SchemaVersionStatus.ConnectionFailed => ConnectionFailedMessage,
        _ => MismatchMessage,
    };
}
