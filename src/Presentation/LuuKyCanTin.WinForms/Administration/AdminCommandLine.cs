namespace LuuKyCanTin.WinForms.Administration;

/// <summary>
/// The admin verbs. Everything else is passed on to the Generic Host (for example <c>--environment Development</c>).
/// </summary>
/// <param name="ConfirmedDatabaseName">The database name given with <c>--force=</c>; empty when the name was left out.</param>
internal sealed record AdminCommandLine(
    bool MustMigrate, bool MustSeedDemo, string? ConfirmedDatabaseName, string[] HostArgs)
{
    private const string MigrateVerb = "--migrate";
    private const string SeedDemoVerb = "--seed-demo";
    private const string ForceFlag = "--force";

    public bool IsAdminCommand => MustMigrate || MustSeedDemo;

    public static AdminCommandLine Parse(IReadOnlyList<string> args)
    {
        bool mustMigrate = false, mustSeedDemo = false;
        string? confirmedDatabaseName = null;
        var hostArgs = new List<string>();

        foreach (var arg in args)
        {
            if (arg.Equals(MigrateVerb, StringComparison.OrdinalIgnoreCase))
                mustMigrate = true;
            else if (arg.Equals(SeedDemoVerb, StringComparison.OrdinalIgnoreCase))
                mustSeedDemo = true;
            else if (arg.Equals(ForceFlag, StringComparison.OrdinalIgnoreCase))
                confirmedDatabaseName = "";
            else if (arg.StartsWith(ForceFlag + "=", StringComparison.OrdinalIgnoreCase))
                confirmedDatabaseName = arg[(ForceFlag.Length + 1)..];
            else
                hostArgs.Add(arg);
        }

        return new AdminCommandLine(mustMigrate, mustSeedDemo, confirmedDatabaseName, [.. hostArgs]);
    }
}
