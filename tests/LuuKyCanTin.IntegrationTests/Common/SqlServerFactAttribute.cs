using LuuKyCanTin.Infrastructure.Common;
using Microsoft.Win32;

namespace LuuKyCanTin.IntegrationTests.Common;

/// <summary>
/// A fact that needs a SQL Server to create throw-away databases on. The server is
/// <c>LUUKYCANTIN_TEST_SQLSERVER</c> (from the environment or the repository's <c>.env</c>, e.g. the
/// docker-compose container), otherwise LocalDB. Without either it is skipped on a dev machine; on CI
/// (the <c>CI</c> variable is set) it always runs, so a missing server fails the build.
/// </summary>
public sealed class SqlServerFactAttribute : FactAttribute
{
    public const string ServerVariable = "LUUKYCANTIN_TEST_SQLSERVER";

    private const string LocalDbConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;TrustServerCertificate=true;Connect Timeout=60";

    private static readonly string? ConfiguredServer = ReadConfiguredServer();

    /// <summary>A connection string to the server's master database.</summary>
    public static string ConnectionString { get; } = ConfiguredServer ?? LocalDbConnectionString;

    /// <summary>False only when the tests are going to be skipped, so fixtures can avoid touching a server.</summary>
    public static bool ShouldRun { get; } =
        ConfiguredServer is not null || Environment.GetEnvironmentVariable("CI") is not null || IsLocalDbInstalled();

    public SqlServerFactAttribute()
    {
        if (!ShouldRun)
            Skip = $"No test SQL Server: set {ServerVariable} in .env (see docker-compose.yml) or install LocalDB.";
    }

    private static string? ReadConfiguredServer()
    {
        DotEnvFile.Load(Path.Combine(RepositoryPaths.Root, DotEnvFile.FileName));
        var value = Environment.GetEnvironmentVariable(ServerVariable);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool IsLocalDbInstalled()
    {
        if (!OperatingSystem.IsWindows())
            return false;

        using var versions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server Local DB\Installed Versions");
        return versions?.SubKeyCount > 0;
    }
}
