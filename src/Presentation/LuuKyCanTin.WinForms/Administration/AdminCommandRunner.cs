using LuuKyCanTin.Application.Administration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>Runs <c>--migrate</c> and <c>--seed-demo</c> without opening any window.</summary>
internal sealed class AdminCommandRunner(
    IServiceScopeFactory scopeFactory, bool isDevelopment, TextWriter output, ILogger<AdminCommandRunner> logger)
{
    public const int Success = 0;
    public const int Failure = 1;

    public async Task<int> RunAsync(AdminCommandLine command, CancellationToken ct = default)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();

            if (command.MustMigrate)
                await MigrateAsync(scope.ServiceProvider.GetRequiredService<IDatabaseMigrator>(), ct);

            if (command.MustSeedDemo)
            {
                var decision = await scope.ServiceProvider.GetRequiredService<IDemoDataSeeder>()
                    .SeedAsync(isDevelopment, command.ConfirmedDatabaseName, ct);
                if (decision != DemoSeedDecision.Allowed)
                {
                    logger.LogWarning("Demo data seeding refused: {Decision}", decision);
                    output.WriteLine(GetRefusalMessage(decision));
                    return Failure;
                }

                logger.LogInformation("Demo data seeded");
                output.WriteLine("Đã nạp dữ liệu mẫu.");
            }

            return Success;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Admin command failed");
            output.WriteLine($"Lỗi: {ex.Message} Chi tiết đã được ghi vào nhật ký.");
            return Failure;
        }
    }

    private async Task MigrateAsync(IDatabaseMigrator migrator, CancellationToken ct)
    {
        var applied = await migrator.MigrateAsync(ct);
        logger.LogInformation("Applied {Count} migration(s): {Migrations}", applied.Count, applied);

        if (applied.Count == 0)
        {
            output.WriteLine("Cơ sở dữ liệu đã ở phiên bản mới nhất.");
            return;
        }

        output.WriteLine($"Đã áp dụng {applied.Count} migration:");
        foreach (var migration in applied)
            output.WriteLine("  " + migration);
    }

    private static string GetRefusalMessage(DemoSeedDecision decision) => decision switch
    {
        DemoSeedDecision.NotDevelopment =>
            "Chỉ nạp dữ liệu mẫu trong môi trường Development. Nếu chắc chắn, chạy lại với --force=<tên cơ sở dữ liệu>.",
        _ => "Tên cơ sở dữ liệu sau --force= không khớp với cơ sở dữ liệu đang kết nối.",
    };
}
