namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>
/// The admin verbs. Everything else is passed on to the Generic Host (for example <c>--environment Development</c>).
/// </summary>
/// <param name="ForceDatabaseName">The database name given with <c>--force=</c>; empty when the name was left out.</param>
internal sealed record AdminCommandLine(bool Migrate, bool SeedDemo, string? ForceDatabaseName, string[] HostArgs)
{
    private const string MigrateVerb = "--migrate";
    private const string SeedDemoVerb = "--seed-demo";
    private const string ForceFlag = "--force";

    public bool IsAdminCommand => Migrate || SeedDemo;

    public static AdminCommandLine Parse(IReadOnlyList<string> args)
    {
        bool migrate = false, seedDemo = false;
        string? forceDatabaseName = null;
        var hostArgs = new List<string>();

        foreach (var arg in args)
        {
            if (arg.Equals(MigrateVerb, StringComparison.OrdinalIgnoreCase))
                migrate = true;
            else if (arg.Equals(SeedDemoVerb, StringComparison.OrdinalIgnoreCase))
                seedDemo = true;
            else if (arg.Equals(ForceFlag, StringComparison.OrdinalIgnoreCase))
                forceDatabaseName = "";
            else if (arg.StartsWith(ForceFlag + "=", StringComparison.OrdinalIgnoreCase))
                forceDatabaseName = arg[(ForceFlag.Length + 1)..];
            else
                hostArgs.Add(arg);
        }

        return new AdminCommandLine(migrate, seedDemo, forceDatabaseName, [.. hostArgs]);
    }
}
