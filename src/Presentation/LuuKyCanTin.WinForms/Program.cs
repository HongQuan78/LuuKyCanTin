using System.Text;
using LuuKyCanTin.Application;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure;
using LuuKyCanTin.Infrastructure.Common;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.DanhMuc;
using LuuKyCanTin.WinForms.HeThong;
using LuuKyCanTin.WinForms.LuuKy;
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
            GlobalExceptionHandler.CaiDat();
            ApplicationConfiguration.Initialize();
        }

        try
        {
            // Until the host has read its configuration, a startup failure still needs a file to land in.
            Log.Logger = TaoLogger(configuration: null);
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

        // Release the bootstrap logger's file before the configured logger opens the same one.
        Log.CloseAndFlush();
        Log.Logger = TaoLogger(builder.Configuration);
        builder.Services.AddSerilog();

        builder.Services.Configure<AppOptions>(builder.Configuration.GetSection(AppOptions.SectionName));
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddTransient<LoginForm>();
        builder.Services.AddTransient(sp => new MainForm(sp.GetRequiredService<IServiceScopeFactory>()));
        builder.Services.AddTransient<ThemDoiTuongForm>();
        builder.Services.AddTransient<BienNhanThuForm>();
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

        // Sign-in and sign-out loop without restarting: the context swaps login and shell as the user signs out.
        var context = new ShellApplicationContext(host.Services);
        context.BatDau();
        WinFormsApp.Run(context);
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

    private static Serilog.ILogger TaoLogger(IConfiguration? configuration)
    {
        // %ProgramData% is resolved here; Serilog does not expand environment variables in paths.
        var logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "LuuKyCanTin", "logs");
        Directory.CreateDirectory(logDirectory);

        var loggerConfiguration = new LoggerConfiguration();
        if (configuration is not null)
            loggerConfiguration.ReadFrom.Configuration(configuration);

        return loggerConfiguration
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(logDirectory, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30)
            .CreateLogger();
    }
}
