using Microsoft.Win32;

namespace LuuKyCanTin.IntegrationTests.Common;

/// <summary>
/// A fact that needs SQL Server LocalDB. On a dev machine without LocalDB it is reported as skipped;
/// on CI (the <c>CI</c> variable is set) it always runs, so a missing LocalDB fails the build.
/// </summary>
public sealed class LocalDbFactAttribute : FactAttribute
{
    public const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=60";

    /// <summary>False only when the tests are going to be skipped, so fixtures can avoid touching LocalDB.</summary>
    public static bool ShouldRun { get; } = Environment.GetEnvironmentVariable("CI") is not null || IsLocalDbInstalled();

    public LocalDbFactAttribute()
    {
        if (!ShouldRun)
            Skip = "SQL Server LocalDB is not installed on this machine.";
    }

    private static bool IsLocalDbInstalled()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        using var versions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions");
        return versions?.SubKeyCount > 0;
    }
}
