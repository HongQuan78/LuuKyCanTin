using System.Text;
using LuuKyCanTin.Application;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure;
using LuuKyCanTin.Infrastructure.Common;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.HeThong;
using LuuKyCanTin.WinForms.Shell;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WinFormsApp = System.Windows.Forms.Application;

namespace LuuKyCanTin.WinForms;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        var command = AdminCommandLine.Parse(args);
        if (command.IsAdminCommand)
        {
            if (ParentConsole.TryAttach())
                Console.OutputEncoding = Encoding.UTF8;
        }
        else
        {
            GlobalExceptionHandler.Install();
            ApplicationConfiguration.Initialize();
        }

        try
        {
            using var host = CreateHost(command.HostArgs);
            return command.IsAdminCommand ? RunAdminCommand(host, command) : RunApplication(host);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "LuuKyCanTin failed to start");
            if (!command.IsAdminCommand)
                throw;

            Console.WriteLine($"Lỗi: {ex.Message} Chi tiết đã được ghi vào nhật ký.");
            return AdminCommandRunner.Failure;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHost CreateHost(string[] args)
    {
        // Before the builder exists, so the host's environment-variable provider sees the values.
        DotEnvFile.Load(Path.Combine(AppContext.BaseDirectory, DotEnvFile.FileName));

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
        builder.Services.AddSingleton<IDieuHuong, DieuHuong>();

        return builder.Build();
    }

    // No Form is resolved on this path, so it also works over a remote shell without a desktop.
    private static int RunAdminCommand(IHost host, AdminCommandLine command)
    {
        Log.Information("LuuKyCanTin admin command: migrate={Migrate}, seed-demo={SeedDemo}", command.Migrate, command.SeedDemo);
        var runner = new AdminCommandRunner(
            host.Services.GetRequiredService<IServiceScopeFactory>(),
            host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment(),
            Console.Out,
            host.Services.GetRequiredService<ILogger<AdminCommandRunner>>());

        return runner.RunAsync(command).GetAwaiter().GetResult();
    }

    private static int RunApplication(IHost host)
    {
        Log.Information("LuuKyCanTin starting");
        // Workstations never migrate: they only check, and refuse to run against a different schema.
        if (!SchemaVersionIsCurrent(host.Services))
            return 1;

        var mainForm = host.Services.GetRequiredService<MainForm>();
        // The presenter stays alive through its subscription to the form's events.
        ActivatorUtilities.CreateInstance<MainPresenter>(host.Services, mainForm);
        WinFormsApp.Run(mainForm);
        return 0;
    }

    private static bool SchemaVersionIsCurrent(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        // Blocking is safe here: no SynchronizationContext exists before the message loop starts.
        var result = scope.ServiceProvider.GetRequiredService<ISchemaVersionChecker>().CheckAsync().GetAwaiter().GetResult();

        var message = SchemaVersionGate.BlockingMessage(result);
        if (message is null)
            return true;

        Log.Error("Database schema check failed ({Status}): expected migration {Expected}, database has {Actual}",
            result.Status, result.Expected, result.Actual ?? "(none)");
        MessageBox.Show(message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
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
