using System.Globalization;
using Microsoft.Data.SqlClient;

namespace SP03Search;

internal sealed class SpikeOptions
{
    public const string DatabaseName = "LuuKyCanTin_Spike_Search";
    public const string DefaultConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Initial Catalog=LuuKyCanTin_Spike_Search;Integrated Security=true;TrustServerCertificate=true";

    public string Mode { get; private set; } = "ui";

    public int Rows { get; private set; } = 10_000;

    public string ConnectionString { get; private set; } = DefaultConnectionString;

    public string OutputDirectory { get; private set; } = "artifacts";

    public bool ShowHelp { get; private set; }

    public string DatabaseConnectionString => WithCatalog(ConnectionString, DatabaseName);

    public string MasterConnectionString => WithCatalog(ConnectionString, "master");

    public static SpikeOptions Parse(string[] args)
    {
        var options = new SpikeOptions();
        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "--setup":
                    options.Mode = "setup";
                    break;
                case "--benchmark":
                    options.Mode = "benchmark";
                    break;
                case "--ui-check":
                    options.Mode = "ui-check";
                    break;
                case "--ui":
                    options.Mode = "ui";
                    break;
                case "--help" or "-h" or "/?":
                    options.ShowHelp = true;
                    break;
                case "--rows":
                    options.Rows = ParseRows(Next(args, ref index, "--rows"));
                    break;
                case "--connection":
                    options.ConnectionString = Next(args, ref index, "--connection");
                    break;
                case "--out":
                    options.OutputDirectory = Next(args, ref index, "--out");
                    break;
                default:
                    throw new ArgumentException($"Unknown argument '{args[index]}'.");
            }
        }

        return options;
    }

    private static string Next(string[] args, ref int index, string option)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {option}.");
        }

        var value = args[index + 1];
        if (value.StartsWith("--", StringComparison.Ordinal))
        {
            throw new ArgumentException($"Missing value for {option} (found '{value}').");
        }

        index++;
        return value;
    }

    private static int ParseRows(string value)
    {
        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rows) || rows < 1)
        {
            throw new ArgumentException($"--rows must be a positive integer, got '{value}'.");
        }

        return rows;
    }

    private static string WithCatalog(string connectionString, string catalog)
    {
        var builder = new SqlConnectionStringBuilder(connectionString) { InitialCatalog = catalog };
        return builder.ConnectionString;
    }
}

internal static class SpikeOptionsHelp
{
    public const string Text = """
        SP-03 spike — Vietnamese diacritic-insensitive incremental search (Story 1.7)

        Usage:
          SP-03-Search --setup     [--rows N] [--connection CS] [--out DIR]
          SP-03-Search --benchmark [--rows N] [--connection CS] [--out DIR]
          SP-03-Search --ui-check  [--rows N] [--connection CS] [--out DIR]
          SP-03-Search [--ui]      [--rows N] [--connection CS] [--out DIR]
          SP-03-Search --help

        Modes:
          --setup      Drop/recreate the throw-away database (collation Vietnamese_CI_AI)
                       and seed N names. Destructive: only for a dev server.
          --benchmark  Run the collation checks, raw SQL and EF Core measurements, the
                       cancellation probe and the headless debounce test. Writes
                       artifacts/benchmark-<rows>.txt and exits non-zero on a failed check.
          --ui-check   Drive the WinForms form with simulated typing, measure keystroke to
                       grid and write artifacts/ui-check-<rows>.txt.
          --ui         Open the form for manual testing (default).

        Options:
          --rows N        Number of names (default 10000; measure 50000 for the curve).
          --connection CS SQL Server connection string. Default is LocalDB:
                          Server=(localdb)\MSSQLLocalDB;...;Database is always LuuKyCanTin_Spike_Search.
          --out DIR       Artifact directory (default: artifacts).
        """;
}
