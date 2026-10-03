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
        var lenh = AdminCommandLine.PhanTich(args);
        if (lenh.LaLenhQuanTri)
        {
            if (ParentConsole.ThuGan())
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
            using var host = TaoHost(lenh.HostArgs);
            return lenh.LaLenhQuanTri ? ChayLenhQuanTri(host, lenh) : ChayUngDung(host);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "LuuKyCanTin failed to start");
            if (!lenh.LaLenhQuanTri)
                throw;

            Console.WriteLine($"Lỗi: {ex.Message} Chi tiết đã được ghi vào nhật ký.");
            return AdminCommandRunner.Failure;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IHost TaoHost(string[] args)
    {
        // Before the builder exists, so the host's environment-variable provider sees the values.
        DotEnvFile.Nap(Path.Combine(AppContext.BaseDirectory, DotEnvFile.FileName));

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
    private static int ChayLenhQuanTri(IHost host, AdminCommandLine lenh)
    {
        Log.Information("LuuKyCanTin admin command: migrate={Migrate}, seed-demo={SeedDemo}", lenh.CoApDungMigration, lenh.CoNapDuLieuMau);
        var runner = new AdminCommandRunner(
            host.Services.GetRequiredService<IServiceScopeFactory>(),
            host.Services.GetRequiredService<IHostEnvironment>().IsDevelopment(),
            Console.Out,
            host.Services.GetRequiredService<ILogger<AdminCommandRunner>>());

        return runner.ChayAsync(lenh).GetAwaiter().GetResult();
    }

    private static int ChayUngDung(IHost host)
    {
        Log.Information("LuuKyCanTin starting");
        // Workstations never migrate: they only check, and refuse to run against a different schema.
        if (!LaPhienBanCoSoDuLieuKhop(host.Services))
            return 1;

        // Sign-in and sign-out loop without restarting: the context swaps login and shell as the user signs out.
        var context = new ShellApplicationContext(host.Services);
        context.BatDau();
        WinFormsApp.Run(context);
        return 0;
    }

    private static bool LaPhienBanCoSoDuLieuKhop(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        // Blocking is safe here: no SynchronizationContext exists before the message loop starts.
        return SchemaVersionGate.DuocPhepMoAsync(
                scope.ServiceProvider.GetRequiredService<ISchemaVersionChecker>(),
                thongBao => MessageBox.Show(thongBao, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error),
                services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(SchemaVersionGate)))
            .GetAwaiter().GetResult();
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
