using System.Reflection;
using Velopack;

namespace SP02Velopack;

internal static class Program
{
    private const string PackId = "LuuKyCanTinSpike";
    private const string DefaultUpdateSource = @"\\server\LuuKyCanTin\releases";

    [STAThread]
    private static void Main(string[] args)
    {
        // Must be the first line: Velopack handles install/update hooks here and may exit the process.
        VelopackApp.Build().Run();

        ApplicationConfiguration.Initialize();

        string currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.0.0";
        string appData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), PackId);
        string logPath = Path.Combine(appData, "update.log");
        string markerPath = Path.Combine(appData, "version-marker.txt");

        try
        {
            Directory.CreateDirectory(appData);
            File.WriteAllText(markerPath, currentVersion);
        }
        catch (Exception ex)
        {
            // An unwritable/redirected LOCALAPPDATA must never stop the app.
            Log(logPath, $"Could not write version marker ({ex.GetType().Name}: {ex.Message}); continuing.");
        }

        string source = ParseSourceOrLog(args, logPath);
        Log(logPath, $"Started version {currentVersion}; update source: {source}");

        TryUpdateAsync(source, args, logPath).GetAwaiter().GetResult();

        Application.Run(new MainForm(currentVersion));
    }

    private static string ParseSourceOrLog(string[] args, string logPath)
    {
        try
        {
            return ParseSource(args);
        }
        catch (ArgumentException ex)
        {
            Log(logPath, $"Warning: {ex.Message} Using the default update source {DefaultUpdateSource}.");
            return DefaultUpdateSource;
        }
    }

    private static string ParseSource(string[] args)
    {
        for (int i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--source", StringComparison.OrdinalIgnoreCase))
            {
                if (i + 1 >= args.Length)
                {
                    throw new ArgumentException("--source was provided without a value;");
                }

                return args[i + 1];
            }
        }

        return DefaultUpdateSource;
    }

    private static async Task TryUpdateAsync(string source, string[] args, string logPath)
    {
        try
        {
            var manager = new UpdateManager(source);
            var update = await manager.CheckForUpdatesAsync().WaitAsync(TimeSpan.FromSeconds(10));
            if (update is null)
            {
                Log(logPath, "No update available; starting normally.");
                return;
            }

            Log(logPath, $"Update found: {update.TargetFullRelease.Version}");
            using var downloadCts = new CancellationTokenSource(TimeSpan.FromSeconds(120));
            await manager.DownloadUpdatesAsync(update, progress: null, downloadCts.Token);
            Log(logPath, "Update downloaded; applying and restarting.");
            manager.ApplyUpdatesAndRestart(update, args);
        }
        catch (Exception ex)
        {
            Log(logPath, $"Update check failed; starting normally on the current version. {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void Log(string path, string message)
    {
        try
        {
            File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}{Environment.NewLine}");
        }
        catch
        {
            // Logging must never take the app down.
        }
    }
}