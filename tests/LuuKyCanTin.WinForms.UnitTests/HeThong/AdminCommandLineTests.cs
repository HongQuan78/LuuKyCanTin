using LuuKyCanTin.WinForms.HeThong;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public sealed class AdminCommandLineTests
{
    [Fact]
    public void PhanTich_NoArguments_IsNotAdminCommand()
    {
        var lenh = AdminCommandLine.PhanTich([]);

        lenh.LaLenhQuanTri.ShouldBeFalse();
        lenh.HostArgs.ShouldBeEmpty();
    }

    [Fact]
    public void PhanTich_MigrateVerb_IsAdminCommand()
    {
        var lenh = AdminCommandLine.PhanTich(["--migrate"]);

        lenh.LaLenhQuanTri.ShouldBeTrue();
        lenh.CoApDungMigration.ShouldBeTrue();
        lenh.CoNapDuLieuMau.ShouldBeFalse();
    }

    [Fact]
    public void PhanTich_MigrateAndSeedDemo_SetsBoth()
    {
        var lenh = AdminCommandLine.PhanTich(["--migrate", "--seed-demo"]);

        lenh.CoApDungMigration.ShouldBeTrue();
        lenh.CoNapDuLieuMau.ShouldBeTrue();
        lenh.TenCoSoDuLieuXacNhan.ShouldBeNull();
    }

    [Theory]
    [InlineData("--force=LuuKyCanTin", "LuuKyCanTin")]
    [InlineData("--force=", "")]
    [InlineData("--force", "")]
    public void PhanTich_ForceFlag_CarriesConfirmedDatabaseName(string argument, string expected)
    {
        AdminCommandLine.PhanTich(["--seed-demo", argument]).TenCoSoDuLieuXacNhan.ShouldBe(expected);
    }

    [Fact]
    public void PhanTich_VerbInUpperCase_IsRecognised()
    {
        AdminCommandLine.PhanTich(["--MIGRATE"]).CoApDungMigration.ShouldBeTrue();
    }

    [Fact]
    public void PhanTich_MixedArguments_PassesOnlyNonAdminArgumentsToHost()
    {
        // The host's command-line provider would read "--migrate --environment" as the key "migrate".
        var lenh = AdminCommandLine.PhanTich(["--migrate", "--environment", "Development", "--force=X"]);

        lenh.HostArgs.ShouldBe(["--environment", "Development"]);
    }
}
