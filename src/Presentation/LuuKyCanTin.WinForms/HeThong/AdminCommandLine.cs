namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>
/// The admin verbs. Everything else is passed on to the Generic Host (for example <c>--environment Development</c>).
/// </summary>
/// <param name="TenCoSoDuLieuXacNhan">The database name given with <c>--force=</c>; empty when the name was left out.</param>
internal sealed record AdminCommandLine(
    bool CoApDungMigration, bool CoNapDuLieuMau, string? TenCoSoDuLieuXacNhan, string[] HostArgs)
{
    private const string MigrateVerb = "--migrate";
    private const string SeedDemoVerb = "--seed-demo";
    private const string ForceFlag = "--force";

    public bool LaLenhQuanTri => CoApDungMigration || CoNapDuLieuMau;

    public static AdminCommandLine PhanTich(IReadOnlyList<string> args)
    {
        bool coApDungMigration = false, coNapDuLieuMau = false;
        string? tenCoSoDuLieuXacNhan = null;
        var hostArgs = new List<string>();

        foreach (var arg in args)
        {
            if (arg.Equals(MigrateVerb, StringComparison.OrdinalIgnoreCase))
                coApDungMigration = true;
            else if (arg.Equals(SeedDemoVerb, StringComparison.OrdinalIgnoreCase))
                coNapDuLieuMau = true;
            else if (arg.Equals(ForceFlag, StringComparison.OrdinalIgnoreCase))
                tenCoSoDuLieuXacNhan = "";
            else if (arg.StartsWith(ForceFlag + "=", StringComparison.OrdinalIgnoreCase))
                tenCoSoDuLieuXacNhan = arg[(ForceFlag.Length + 1)..];
            else
                hostArgs.Add(arg);
        }

        return new AdminCommandLine(coApDungMigration, coNapDuLieuMau, tenCoSoDuLieuXacNhan, [.. hostArgs]);
    }
}
