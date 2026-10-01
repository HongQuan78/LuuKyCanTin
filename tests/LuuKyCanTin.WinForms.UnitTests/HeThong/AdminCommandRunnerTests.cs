using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.WinForms.HeThong;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public class AdminCommandRunnerTests
{
    private readonly IDatabaseMigrator _migrator = Substitute.For<IDatabaseMigrator>();
    private readonly IDemoDataSeeder _seeder = Substitute.For<IDemoDataSeeder>();
    private readonly StringWriter _output = new();

    private Task<int> RunAsync(bool isDevelopment, params string[] args)
    {
        var services = new ServiceCollection()
            .AddScoped(_ => _migrator)
            .AddScoped(_ => _seeder)
            .BuildServiceProvider();
        var runner = new AdminCommandRunner(
            services.GetRequiredService<IServiceScopeFactory>(), isDevelopment, _output, NullLogger<AdminCommandRunner>.Instance);
        return runner.RunAsync(AdminCommandLine.Parse(args));
    }

    [Fact]
    public async Task Migrate_ReportsAppliedMigrationsAndSucceeds()
    {
        _migrator.MigrateAsync(Arg.Any<CancellationToken>()).Returns(["20261001141501_InitialCreate"]);

        var exitCode = await RunAsync(false, "--migrate");

        exitCode.ShouldBe(0);
        _output.ToString().ShouldContain("20261001141501_InitialCreate");
        await _seeder.DidNotReceiveWithAnyArgs().SeedAsync(default, default);
    }

    [Fact]
    public async Task Migrate_AlreadyCurrent_SaysSo()
    {
        _migrator.MigrateAsync(Arg.Any<CancellationToken>()).Returns([]);

        (await RunAsync(false, "--migrate")).ShouldBe(0);

        _output.ToString().ShouldContain("đã ở phiên bản mới nhất");
    }

    [Fact]
    public async Task Migrate_Failure_ReturnsOneAndReportsError()
    {
        _migrator.MigrateAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("boom"));

        (await RunAsync(false, "--migrate")).ShouldBe(1);

        _output.ToString().ShouldContain("boom");
    }

    [Fact]
    public async Task SeedDemo_PassesEnvironmentAndConfirmation()
    {
        _seeder.SeedAsync(false, "LuuKyCanTin", Arg.Any<CancellationToken>()).Returns(DemoSeedDecision.Allowed);

        (await RunAsync(false, "--seed-demo", "--force=LuuKyCanTin")).ShouldBe(0);

        await _seeder.Received(1).SeedAsync(false, "LuuKyCanTin", Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(DemoSeedDecision.NotDevelopment, "--force=")]
    [InlineData(DemoSeedDecision.DatabaseNameMismatch, "không khớp")]
    public async Task SeedDemo_Refused_ReturnsOneAndExplains(DemoSeedDecision decision, string expectedText)
    {
        _seeder.SeedAsync(Arg.Any<bool>(), Arg.Any<string?>(), Arg.Any<CancellationToken>()).Returns(decision);

        (await RunAsync(false, "--seed-demo")).ShouldBe(1);

        _output.ToString().ShouldContain(expectedText);
    }

    [Fact]
    public async Task MigrateThenSeed_RunsMigrationFirst()
    {
        _migrator.MigrateAsync(Arg.Any<CancellationToken>()).Returns([]);
        _seeder.SeedAsync(true, null, Arg.Any<CancellationToken>()).Returns(DemoSeedDecision.Allowed);

        (await RunAsync(true, "--seed-demo", "--migrate")).ShouldBe(0);

        Received.InOrder(() =>
        {
            _migrator.MigrateAsync(Arg.Any<CancellationToken>());
            _seeder.SeedAsync(true, null, Arg.Any<CancellationToken>());
        });
    }
}
