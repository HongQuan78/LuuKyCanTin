using System.Reflection;
using System.Text.RegularExpressions;
using LuuKyCanTin.Application.Administration;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Administration;

public class PermissionCodesTests
{
    // The full module × action grid: 8 modules, 6 actions.
    private const int StandardPermissionCount = 48;

    [Fact]
    public void All_HasThe48StandardEntries()
    {
        PermissionCodes.All.Count.ShouldBe(StandardPermissionCount);
        PermissionCodes.Modules.Count.ShouldBe(8);
        PermissionCodes.Actions.Count.ShouldBe(6);
    }

    [Fact]
    public void All_HasUniqueCodesAndIds()
    {
        PermissionCodes.All.Select(q => q.Code).Distinct(StringComparer.Ordinal).Count().ShouldBe(StandardPermissionCount);
        PermissionCodes.All.Select(q => q.Id).Distinct().Count().ShouldBe(StandardPermissionCount);
        PermissionCodes.All.Select(q => q.Id).Order().ShouldBe(Enumerable.Range(1, StandardPermissionCount));
    }

    [Fact]
    public void All_EveryCodeMatchesTheModuleActionPattern()
    {
        foreach (var permission in PermissionCodes.All)
        {
            Regex.IsMatch(permission.Code, "^[A-Z-]+\\.[A-Za-z]+$").ShouldBeTrue(permission.Code);
            permission.Code.ShouldStartWith(permission.Module + ".");
            PermissionCodes.Modules.ShouldContain(m => m.Code == permission.Module);
        }
    }

    [Fact]
    public void All_CoversEveryModuleAndActionExactlyOnce()
    {
        foreach (var module in PermissionCodes.Modules)
        {
            var moduleCodes = PermissionCodes.ForModule(module.Code);
            moduleCodes.Count.ShouldBe(PermissionCodes.Actions.Count);
        }
    }

    [Fact]
    public void EveryConstantInTheNestedClasses_AppearsInAll()
    {
        var nested = typeof(PermissionCodes).GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .Where(t => t is { IsAbstract: true, IsSealed: true })
            .ToList();
        nested.Count.ShouldBe(PermissionCodes.Modules.Count);

        var catalogueCodes = PermissionCodes.All.Select(q => q.Code).ToHashSet(StringComparer.Ordinal);
        foreach (var moduleClass in nested)
        {
            var constants = moduleClass
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue()!)
                .ToList();
            constants.Count.ShouldBe(PermissionCodes.Actions.Count);
            foreach (var code in constants)
                catalogueCodes.ShouldContain(code);
        }
    }

    [Fact]
    public void ForModule_CanLeaveApproveOut()
    {
        PermissionCodes.ForModule(PermissionCodes.ModuleCustodyIncrease).Count.ShouldBe(6);
        PermissionCodes.ForModule(PermissionCodes.ModuleCustodyIncrease, includeApprove: false).ShouldNotContain(PermissionCodes.CustodyIncrease.Approve);
        PermissionCodes.ForModule(PermissionCodes.ModuleCustodyIncrease, includeApprove: false).Count.ShouldBe(5);
    }

    [Fact]
    public void IsSpecial_IsTrueOnlyOutsideTheStandardGrid()
    {
        var administrationView = PermissionCodes.All.Single(q => q.Code == PermissionCodes.Administration.View);
        PermissionCodes.IsSpecial(administrationView).ShouldBeFalse();

        // What a later story adds: a new action on an existing module, or a new module.
        PermissionCodes.IsSpecial(new PermissionCodes.PermissionDefinition(49, "HT.KhoaSo", "Khoá sổ — Hệ thống", "HT")).ShouldBeTrue();
        PermissionCodes.IsSpecial(new PermissionCodes.PermissionDefinition(50, "LK.LuiNgay", "Lùi ngày — Lưu ký", "LK")).ShouldBeTrue();
    }
}
