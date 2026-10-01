using SP03Search.Benchmark;
using SP03Search.Data;

namespace SP03Search;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        ParentConsole.TryAttach();

        SpikeOptions options;
        try
        {
            options = SpikeOptions.Parse(args);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine();
            Console.Error.WriteLine(SpikeOptionsHelp.Text);
            return 2;
        }

        if (options.ShowHelp)
        {
            Console.WriteLine(SpikeOptionsHelp.Text);
            return 0;
        }

        try
        {
            return options.Mode switch
            {
                "setup" => RunSetupAsync(options).GetAwaiter().GetResult(),
                "benchmark" => RunBenchmarkAsync(options).GetAwaiter().GetResult(),
                "ui-check" => RunUiCheck(options),
                _ => RunUi(options),
            };
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"SP-03 failed: {ex}");
            return 1;
        }
    }

    private static async Task<int> RunSetupAsync(SpikeOptions options)
    {
        var database = new SpikeDatabase(options);
        Console.WriteLine($"Creating {SpikeOptions.DatabaseName} and seeding {options.Rows:N0} names...");
        await database.RecreateAsync(CancellationToken.None);
        await database.SeedAsync(options.Rows, CancellationToken.None);
        var info = await database.GetInfoAsync(CancellationToken.None);
        Console.WriteLine($"SQL Server : {info.ServerInfo}");
        Console.WriteLine($"Indexes    : {string.Join(", ", info.Indexes)}");
        Console.WriteLine("Setup complete.");
        return 0;
    }

    private static async Task<int> RunBenchmarkAsync(SpikeOptions options)
    {
        var database = new SpikeDatabase(options);
        await database.EnsureSeededAsync(options.Rows, CancellationToken.None);

        var runner = new BenchmarkRunner(options);
        var report = await runner.RunAsync(options.Rows, CancellationToken.None);

        Directory.CreateDirectory(options.OutputDirectory);
        var path = Path.Combine(options.OutputDirectory, $"benchmark-{options.Rows}.txt");
        await File.WriteAllTextAsync(path, report, CancellationToken.None);
        Console.Write(report);
        Console.WriteLine();
        Console.WriteLine($"Report written: {Path.GetFullPath(path)}");
        return runner.Failures > 0 ? 1 : 0;
    }

    private static int RunUiCheck(SpikeOptions options)
    {
        new SpikeDatabase(options).EnsureSeededAsync(options.Rows, CancellationToken.None).GetAwaiter().GetResult();
        ApplicationConfiguration.Initialize();
        return MainForm.RunUiCheck(options);
    }

    private static int RunUi(SpikeOptions options)
    {
        new SpikeDatabase(options).EnsureSeededAsync(options.Rows, CancellationToken.None).GetAwaiter().GetResult();
        ApplicationConfiguration.Initialize();
        using var form = new MainForm(options);
        Application.Run(form);
        return 0;
    }
}
