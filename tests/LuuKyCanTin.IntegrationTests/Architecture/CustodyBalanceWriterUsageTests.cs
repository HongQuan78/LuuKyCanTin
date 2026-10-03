using System.Text.RegularExpressions;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.IntegrationTests.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Architecture;

/// <summary>AC 3's guard rail: only the ledger engine may change a custodial balance.</summary>
public class CustodyBalanceWriterUsageTests
{
    // Matches "CustodyBalance =", "[CustodyBalance] =", "[CustodyBalance]=" and case/spacing variants, but not "==" comparisons.
    private static readonly Regex BalanceUpdate = new(@"\[?\s*CustodyBalance\s*\]?\s*=(?!=)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Fact]
    public void OnlyCustodyLedgerService_DependsOnTheBalanceWriter()
    {
        var user = typeof(CustodyLedgerService).Assembly.GetTypes()
            .Where(t => t.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Any(p => p.ParameterType == typeof(ICustodyBalanceWriter)))
            .ToList();

        user.ShouldBe([typeof(CustodyLedgerService)]);
    }

    [Fact]
    public void TheBalanceUpdateStatement_LivesOnlyInTheInfrastructureWriter()
    {
        var sourceDirectory = Path.Combine(RepositoryPaths.Root, "src");
        var violations = Directory.EnumerateFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories)
            .Where(file => !file.Contains("Migrations", StringComparison.OrdinalIgnoreCase))
            .Where(file => BalanceUpdate.IsMatch(File.ReadAllText(file)))
            .ToList();

        violations.ShouldHaveSingleItem().ShouldEndWith("CustodyBalanceWriter.cs");
    }
}
