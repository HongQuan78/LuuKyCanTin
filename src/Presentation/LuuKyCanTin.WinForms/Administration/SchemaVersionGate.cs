using LuuKyCanTin.Application.Administration;
using Microsoft.Extensions.Logging;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>Decides whether the workstation may open its main form against the database it found.</summary>
internal static class SchemaVersionGate
{
    public const string VersionMismatchMessage = "Phiên bản cơ sở dữ liệu không khớp, vui lòng liên hệ quản trị viên.";
    public const string ConnectionFailedMessage = "Không kết nối được cơ sở dữ liệu, vui lòng liên hệ quản trị viên.";

    /// <returns>The message to show before exiting, or null when startup may continue.</returns>
    public static string? GetBlockingMessage(SchemaVersionCheckResult result) => result.Status switch
    {
        SchemaVersionStatus.Matches => null,
        SchemaVersionStatus.ConnectionFailed => ConnectionFailedMessage,
        _ => VersionMismatchMessage,
    };

    /// <summary>
    /// Workstations never migrate: they only check. On a mismatch or a failed connection it logs both versions,
    /// shows the message through <paramref name="showError"/> and returns false, and startup must stop.
    /// </summary>
    public static async Task<bool> CanOpenAsync(
        ISchemaVersionChecker checker, Action<string> showError, ILogger logger, CancellationToken ct = default)
    {
        var result = await checker.CheckAsync(ct);
        var message = GetBlockingMessage(result);
        if (message is null)
            return true;

        logger.LogError("Database schema check failed ({Status}): expected migration {Expected}, database has {Actual}",
            result.Status, result.Expected, result.Actual ?? "(none)");
        showError(message);
        return false;
    }
}
