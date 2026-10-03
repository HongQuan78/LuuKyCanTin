using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.WinForms.HeThong;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public sealed class AdminCommandRunnerTests
{
    private readonly IDatabaseMigrator _migrator = Substitute.For<IDatabaseMigrator>();
    private readonly IDemoDataSeeder _seeder = Substitute.For<IDemoDataSeeder>();
    private readonly StringWriter _output = new();

    private Task<int> ChayAsync(bool laMoiTruongPhatTrien, params string[] args)
    {
        var services = new ServiceCollection()
            .AddScoped(_ => _migrator)
            .AddScoped(_ => _seeder)
            .BuildServiceProvider();
        var runner = new AdminCommandRunner(
            services.GetRequiredService<IServiceScopeFactory>(), laMoiTruongPhatTrien, _output,
            NullLogger<AdminCommandRunner>.Instance);
        return runner.ChayAsync(AdminCommandLine.PhanTich(args));
    }

    [Fact]
    public async Task Chay_PendingMigrations_ReportsThemAndSucceeds()
    {
        _migrator.ApDungMigrationAsync(Arg.Any<CancellationToken>()).Returns(["20261001141501_InitialCreate"]);

        var exitCode = await ChayAsync(false, "--migrate");

        exitCode.ShouldBe(0);
        _output.ToString().ShouldContain("20261001141501_InitialCreate");
        await _seeder.DidNotReceiveWithAnyArgs().NapDuLieuMauAsync(default, default);
    }

    [Fact]
    public async Task Chay_MigrateAlreadyCurrent_SaysSo()
    {
        _migrator.ApDungMigrationAsync(Arg.Any<CancellationToken>()).Returns([]);

        (await ChayAsync(false, "--migrate")).ShouldBe(0);

        _output.ToString().ShouldContain("đã ở phiên bản mới nhất");
    }

    [Fact]
    public async Task Chay_MigrateFails_ReturnsFailureAndReportsError()
    {
        _migrator.ApDungMigrationAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        (await ChayAsync(false, "--migrate")).ShouldBe(1);

        _output.ToString().ShouldContain("boom");
    }

    [Fact]
    public async Task Chay_SeedDemo_PassesEnvironmentAndConfirmation()
    {
        _seeder.NapDuLieuMauAsync(false, "LuuKyCanTin", Arg.Any<CancellationToken>()).Returns(DemoSeedDecision.Allowed);

        (await ChayAsync(false, "--seed-demo", "--force=LuuKyCanTin")).ShouldBe(0);

        await _seeder.Received(1).NapDuLieuMauAsync(false, "LuuKyCanTin", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(DemoSeedDecision.NotDevelopment, "--force=")]
    [InlineData(DemoSeedDecision.DatabaseNameMismatch, "không khớp")]
    public async Task Chay_SeedDemoRefused_ReturnsFailureAndExplains(DemoSeedDecision decision, string expectedText)
    {
        _seeder.NapDuLieuMauAsync(Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(decision);

        (await ChayAsync(false, "--seed-demo")).ShouldBe(1);

        _output.ToString().ShouldContain(expectedText);
    }

    [Fact]
    public async Task Chay_MigrateAndSeedDemo_RunsMigrationFirst()
    {
        _migrator.ApDungMigrationAsync(Arg.Any<CancellationToken>()).Returns([]);
        _seeder.NapDuLieuMauAsync(true, null, Arg.Any<CancellationToken>()).Returns(DemoSeedDecision.Allowed);

        (await ChayAsync(true, "--seed-demo", "--migrate")).ShouldBe(0);

        Received.InOrder(() =>
        {
            _migrator.ApDungMigrationAsync(Arg.Any<CancellationToken>());
            _seeder.NapDuLieuMauAsync(true, null, Arg.Any<CancellationToken>());
        });
    }
}
