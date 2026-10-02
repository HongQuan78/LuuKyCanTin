using System.Text.RegularExpressions;
using LuuKyCanTin.Application.LuuKy;
using LuuKyCanTin.IntegrationTests.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

/// <summary>AC 3's guard rail: only the ledger engine may change a custodial balance.</summary>
public class SoDuLuuKyWriterUsageTests
{
    // Matches "SoDuLuuKy =", "[SoDuLuuKy] =", "[SoDuLuuKy]=" and case/spacing variants, but not "==" comparisons.
    private static readonly Regex CapNhatSoDu = new(@"\[?\s*SoDuLuuKy\s*\]?\s*=(?!=)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Fact]
    public void OnlyGhiSoLuuKyService_DependsOnTheBalanceWriter()
    {
        var nguoiDung = typeof(GhiSoLuuKyService).Assembly.GetTypes()
            .Where(t => t.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Any(p => p.ParameterType == typeof(ISoDuLuuKyWriter)))
            .ToList();

        nguoiDung.ShouldBe([typeof(GhiSoLuuKyService)]);
    }

    [Fact]
    public void TheBalanceUpdateStatement_LivesOnlyInTheInfrastructureWriter()
    {
        var nguon = Path.Combine(RepositoryPaths.Root, "src");
        var viPham = Directory.EnumerateFiles(nguon, "*.cs", SearchOption.AllDirectories)
            .Where(tep => !tep.Contains("Migrations", StringComparison.OrdinalIgnoreCase))
            .Where(tep => CapNhatSoDu.IsMatch(File.ReadAllText(tep)))
            .ToList();

        viPham.ShouldHaveSingleItem().ShouldEndWith("SoDuLuuKyWriter.cs");
    }
}
