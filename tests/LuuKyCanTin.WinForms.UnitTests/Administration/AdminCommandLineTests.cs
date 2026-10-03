using LuuKyCanTin.WinForms.Administration;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

public sealed class AdminCommandLineTests
{
    [Fact]
    public void Parse_NoArguments_IsNotAdminCommand()
    {
        var command = AdminCommandLine.Parse([]);

        command.IsAdminCommand.ShouldBeFalse();
        command.HostArgs.ShouldBeEmpty();
    }

    [Fact]
    public void Parse_MigrateVerb_IsAdminCommand()
    {
        var command = AdminCommandLine.Parse(["--migrate"]);

        command.IsAdminCommand.ShouldBeTrue();
        command.MustMigrate.ShouldBeTrue();
        command.MustSeedDemo.ShouldBeFalse();
    }

    [Fact]
    public void Parse_MigrateAndSeedDemo_SetsBoth()
    {
        var command = AdminCommandLine.Parse(["--migrate", "--seed-demo"]);

        command.MustMigrate.ShouldBeTrue();
        command.MustSeedDemo.ShouldBeTrue();
        command.ConfirmedDatabaseName.ShouldBeNull();
    }

    [Theory]
    [InlineData("--force=LuuKyCanTin", "LuuKyCanTin")]
    [InlineData("--force=", "")]
    [InlineData("--force", "")]
    public void Parse_ForceFlag_CarriesConfirmedDatabaseName(string argument, string expected)
    {
        AdminCommandLine.Parse(["--seed-demo", argument]).ConfirmedDatabaseName.ShouldBe(expected);
    }

    [Fact]
    public void Parse_VerbInUpperCase_IsRecognised()
    {
        AdminCommandLine.Parse(["--MIGRATE"]).MustMigrate.ShouldBeTrue();
    }

    [Fact]
    public void Parse_MixedArguments_PassesOnlyNonAdminArgumentsToHost()
    {
        // The host's command-line provider would read "--migrate --environment" as the key "migrate".
        var command = AdminCommandLine.Parse(["--migrate", "--environment", "Development", "--force=X"]);

        command.HostArgs.ShouldBe(["--environment", "Development"]);
    }
}
