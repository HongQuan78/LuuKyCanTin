using LuuKyCanTin.WinForms.Common;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Common;

public class DotEnvFileTests
{
    [Fact]
    public void Parse_ReadsKeyValuePairs_SkippingBlankLinesAndComments()
    {
        string[] lines =
        [
            "# Kết nối cơ sở dữ liệu",
            "",
            "  ConnectionStrings__LuuKyCanTin = Server=.\\SQLEXPRESS;Database=LuuKyCanTin  ",
            "App__TieuDe=Lưu ký",
        ];

        DotEnvFile.Parse(lines).ShouldBe(
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
    public void Parse_StripsMatchingQuotesOnly(string line, string expected)
    {
        DotEnvFile.Parse([line]).ShouldHaveSingleItem().Value.ShouldBe(expected);
    }

    [Fact]
    public void Parse_KeepsBackslashesLiteral()
    {
        // Unlike JSON, a SQL Server instance name needs no escaping.
        DotEnvFile.Parse([@"KEY=Server=.\SQLEXPRESS"]).ShouldHaveSingleItem().Value.ShouldBe(@"Server=.\SQLEXPRESS");
    }

    [Theory]
    [InlineData("no equals sign")]
    [InlineData("=value without key")]
    public void Parse_MalformedLine_FailsWithItsLineNumber(string badLine)
    {
        Should.Throw<FormatException>(() => DotEnvFile.Parse(["# ok", badLine]))
            .Message.ShouldContain("line 2");
    }

    [Fact]
    public void Load_SetsMissingVariables_ButNeverOverridesExistingOnes()
    {
        var unset = "LUUKY_TEST_" + Guid.NewGuid().ToString("N");
        var alreadySet = "LUUKY_TEST_" + Guid.NewGuid().ToString("N");
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".env");
        File.WriteAllLines(path, [$"{unset}=from-file", $"{alreadySet}=from-file"]);
        Environment.SetEnvironmentVariable(alreadySet, "from-environment");
        try
        {
            DotEnvFile.Load(path);

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
    public void Load_MissingFile_DoesNothing()
    {
        Should.NotThrow(() => DotEnvFile.Load(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".env")));
    }
}
