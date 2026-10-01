using LuuKyCanTin.Application;
using LuuKyCanTin.Infrastructure;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.Shell;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using WinFormsApp = System.Windows.Forms.Application;

namespace LuuKyCanTin.WinForms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        GlobalExceptionHandler.Install();
        ApplicationConfiguration.Initialize();

        try
        {
            var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
            {
                Args = args,
                // Started from a desktop shortcut the working directory is arbitrary; config lives next to the exe.
                ContentRootPath = AppContext.BaseDirectory,
            });

            Log.Logger = CreateLogger(builder.Configuration);
            builder.Services.AddSerilog();

            builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddTransient<MainForm>();

            using var host = builder.Build();

            Log.Information("LuuKyCanTin starting");
            var mainForm = host.Services.GetRequiredService<MainForm>();
            // The presenter stays alive through its subscription to the form's events.
            ActivatorUtilities.CreateInstance<MainPresenter>(host.Services, mainForm);
            WinFormsApp.Run(mainForm);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "LuuKyCanTin failed to start");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static Serilog.ILogger CreateLogger(IConfiguration configuration)
    {
        // %ProgramData% is resolved here; Serilog does not expand environment variables in paths.
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LuuKyCanTin", "logs");
        Directory.CreateDirectory(logDirectory);

        return new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(logDirectory, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();
    }
}
