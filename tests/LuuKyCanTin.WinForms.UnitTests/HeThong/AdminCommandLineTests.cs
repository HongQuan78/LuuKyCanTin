using LuuKyCanTin.WinForms.HeThong;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public class AdminCommandLineTests
{
    [Fact]
    public void NoArguments_IsNormalStartup()
    {
        var command = AdminCommandLine.Parse([]);

        command.IsAdminCommand.ShouldBeFalse();
        command.HostArgs.ShouldBeEmpty();
    }

    [Fact]
    public void Migrate_IsAdminCommand()
    {
        var command = AdminCommandLine.Parse(["--migrate"]);

        command.IsAdminCommand.ShouldBeTrue();
        command.Migrate.ShouldBeTrue();
        command.SeedDemo.ShouldBeFalse();
    }

    [Fact]
    public void MigrateAndSeedDemo_CanBeCombined()
    {
        var command = AdminCommandLine.Parse(["--migrate", "--seed-demo"]);

        command.Migrate.ShouldBeTrue();
        command.SeedDemo.ShouldBeTrue();
        command.ForceDatabaseName.ShouldBeNull();
    }

    [Theory]
    [InlineData("--force=LuuKyCanTin", "LuuKyCanTin")]
    [InlineData("--force=", "")]
    [InlineData("--force", "")]
    public void Force_CarriesTheConfirmedDatabaseName(string argument, string expected)
    {
        AdminCommandLine.Parse(["--seed-demo", argument]).ForceDatabaseName.ShouldBe(expected);
    }

    [Fact]
    public void Verbs_AreCaseInsensitive()
    {
        AdminCommandLine.Parse(["--MIGRATE"]).Migrate.ShouldBeTrue();
    }

    [Fact]
    public void OwnVerbs_AreNotPassedToTheHost_OtherArgumentsAre()
    {
        // The host's command-line provider would read "--migrate --environment" as the key "migrate".
        var command = AdminCommandLine.Parse(["--migrate", "--environment", "Development", "--force=X"]);

        command.HostArgs.ShouldBe(["--environment", "Development"]);
    }
}
