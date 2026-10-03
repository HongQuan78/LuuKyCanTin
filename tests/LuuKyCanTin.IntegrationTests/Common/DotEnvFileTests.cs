using LuuKyCanTin.Infrastructure.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Common;

public sealed class DotEnvFileTests
{
    [Fact]
    public void PhanTich_BlankLinesAndComments_ReturnsOnlyKeyValuePairs()
    {
        string[] lines =
        [
            "# Kết nối cơ sở dữ liệu",
            "",
            "  ConnectionStrings__LuuKyCanTin = Server=.\\SQLEXPRESS;Database=LuuKyCanTin  ",
            "App__TieuDe=Lưu ký",
        ];

        DotEnvFile.PhanTich(lines).ShouldBe(
        [
            new("ConnectionStrings__LuuKyCanTin", @"Server=.\SQLEXPRESS;Database=LuuKyCanTin"),
            new("App__TieuDe", "Lưu ký"),
        ]);
    }

    [Theory]
    [InlineData("KEY=\"a=b; c\"", "a=b; c")]
    [InlineData("KEY='a=b; c'", "a=b; c")]
    [InlineData("KEY=\"unbalanced", "\"unbalanced")]
    [InlineData("KEY=", "")]
    public void PhanTich_QuotedValue_StripsOnlyMatchingQuotes(string line, string expected)
    {
        DotEnvFile.PhanTich([line]).ShouldHaveSingleItem().Value.ShouldBe(expected);
    }

    [Fact]
    public void PhanTich_ValueWithBackslashes_KeepsThemLiteral()
    {
        // Unlike JSON, a SQL Server instance name needs no escaping.
        DotEnvFile.PhanTich([@"KEY=Server=.\SQLEXPRESS"]).ShouldHaveSingleItem().Value.ShouldBe(@"Server=.\SQLEXPRESS");
    }

    [Theory]
    [InlineData("no equals sign")]
    [InlineData("=value without key")]
    public void PhanTich_MalformedLine_FailsWithItsLineNumber(string badLine)
    {
        Should.Throw<FormatException>(() => DotEnvFile.PhanTich(["# ok", badLine]))
            .Message.ShouldContain("line 2");
    }

    [Fact]
    public void Nap_VariableAlreadySet_SetsOnlyMissingOnes()
    {
        var unset = "LUUKY_TEST_" + Guid.NewGuid().ToString("N");
        var alreadySet = "LUUKY_TEST_" + Guid.NewGuid().ToString("N");
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".env");
        File.WriteAllLines(path, [$"{unset}=from-file", $"{alreadySet}=from-file"]);
        Environment.SetEnvironmentVariable(alreadySet, "from-environment");
        try
        {
            DotEnvFile.Nap(path);

            Environment.GetEnvironmentVariable(unset).ShouldBe("from-file");
            Environment.GetEnvironmentVariable(alreadySet).ShouldBe("from-environment");
        }
        finally
        {
            File.Delete(path);
            Environment.SetEnvironmentVariable(unset, null);
            Environment.SetEnvironmentVariable(alreadySet, null);
        }
    }

    [Fact]
    public void Nap_MissingFile_DoesNothing()
    {
        Should.NotThrow(() => DotEnvFile.Nap(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".env")));
    }
}
