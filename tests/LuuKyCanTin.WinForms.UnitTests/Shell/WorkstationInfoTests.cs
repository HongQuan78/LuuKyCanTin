using LuuKyCanTin.WinForms.Shell;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class WorkstationInfoTests
{
    [Theory]
    [InlineData(@"Server=SRV-LUUKY\SQLEXPRESS;Database=LuuKyCanTin;User Id=sa;Password=Bí-mật-1;TrustServerCertificate=True", @"SRV-LUUKY\SQLEXPRESS / LuuKyCanTin")]
    [InlineData(@"Data Source=127.0.0.1,1433;Initial Catalog=LuuKyCanTin_Dev;Integrated Security=True", "127.0.0.1,1433 / LuuKyCanTin_Dev")]
    [InlineData(@"Server=(localdb)\MSSQLLocalDB", @"(localdb)\MSSQLLocalDB")]
    public void ToDatabaseText_AConnectionString_ShowsOnlyServerAndCatalog(string connectionString, string expected)
    {
        WorkstationInfo.ToDatabaseText(connectionString).ShouldBe(expected);
    }

    [Fact]
    public void ToDatabaseText_WithCredentials_NeverShowsThem()
    {
        var text = WorkstationInfo.ToDatabaseText("Server=SRV;Database=Db;User Id=sa;Password=Bí-mật-1");

        text.ShouldNotContain("sa");
        text.ShouldNotContain("Bí-mật-1");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("this is not = a ; valid = ; = string")]
    public void ToDatabaseText_MissingOrInvalid_IsEmpty(string? connectionString)
    {
        WorkstationInfo.ToDatabaseText(connectionString).ShouldBe("");
    }

    [Fact]
    public void Create_AfterTheStartupCheck_IsConnectedAndFormatsTheVersion()
    {
        var info = WorkstationInfo.Create("Server=SRV;Database=Db", "QUAY-01", new Version(1, 2, 3, 4));

        info.DatabaseText.ShouldBe("SRV / Db");
        info.IsDatabaseConnected.ShouldBeTrue();
        info.WorkstationName.ShouldBe("QUAY-01");
        info.Version.ShouldBe("v1.2.3");
    }
}
