using System.Data.Common;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>What the status bar and the sign-in panel say about this workstation.</summary>
/// <param name="DatabaseText">"server / catalog" from the connection string, never its credentials.</param>
public sealed record WorkstationInfo(string DatabaseText, bool IsDatabaseConnected, string WorkstationName, string Version)
{
    /// <remarks>
    /// Connected by construction: the app exits before any form opens when the startup schema check fails, so a
    /// shell that shows this has already reached the database.
    /// </remarks>
    public static WorkstationInfo Create(string? connectionString, string machineName, Version? version) =>
        new(ToDatabaseText(connectionString), IsDatabaseConnected: true, machineName,
            version is null ? "" : $"v{version.ToString(3)}");

    public static string ToDatabaseText(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
            return "";

        DbConnectionStringBuilder builder;
        try
        {
            builder = new DbConnectionStringBuilder { ConnectionString = connectionString };
        }
        catch (ArgumentException)
        {
            return "";
        }

        var server = GetFirst(builder, "Server", "Data Source", "Address", "Addr", "Network Address");
        var database = GetFirst(builder, "Database", "Initial Catalog");
        return (server, database) switch
        {
            ("", _) => "",
            (_, "") => server,
            _ => $"{server} / {database}",
        };
    }

    private static string GetFirst(DbConnectionStringBuilder builder, params string[] keys) =>
        keys.Select(key => builder.TryGetValue(key, out var value) ? value?.ToString() : null)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "";
}
