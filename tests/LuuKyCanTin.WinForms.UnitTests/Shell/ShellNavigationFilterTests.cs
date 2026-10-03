using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Infrastructure.Persistence.Seed;
using LuuKyCanTin.WinForms.Shell;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

/// <summary>The pure permission filter behind the shell's sidebar and Trang chủ tiles.</summary>
public class ShellNavigationFilterTests
{
    private static NavigationModel Model() =>
        ShellNavigation.Create(Substitute.For<INavigator>(), () => { }, () => { }, () => { });

    private static NavigationModel Visible(params string[] granted) =>
        ShellNavigation.BuildVisible(Model(), granted.Contains);

    [Fact]
    public void BuildVisible_ItemWithoutItsPermission_IsHidden()
    {
        var model = Visible(PermissionCodes.MasterData.View);

        model.AllItems.ShouldNotContain(i => i.Key == ShellNavigation.AddInmateKey);
        model.AllItems.ShouldNotContain(i => i.Key == ShellNavigation.DepositReceiptKey);
        model.AllItems.ShouldContain(i => i.Key == ShellNavigation.OfficersKey);
    }

    [Fact]
    public void BuildVisible_GroupWithNoVisibleItem_IsHidden()
    {
        // Master data view alone: Lưu ký holds only the receipt item, so the whole group disappears.
        var model = Visible(PermissionCodes.MasterData.View);

        model.Groups.Select(g => g.Key).ShouldNotContain("custody");
        model.Groups.Select(g => g.Key).ShouldContain("master-data");
    }

    [Fact]
    public void BuildVisible_ItemsWithoutAPermission_AlwaysStay()
    {
        var model = Visible();

        model.Home.Key.ShouldBe(ShellNavigation.HomeKey);
        model.AllItems.Select(i => i.Key).ShouldContain(ShellNavigation.ChangePasswordKey);
        model.AllItems.Select(i => i.Key).ShouldContain(ShellNavigation.LockSessionKey);
        model.AllItems.Select(i => i.Key).ShouldContain(ShellNavigation.SignOutKey);
        model.Groups.Select(g => g.Key).ShouldContain("administration");
    }

    [Fact]
    public void BuildVisible_FacilityInfo_RequiresAdministrationView()
    {
        Model().AllItems.Single(i => i.Key == ShellNavigation.FacilityInfoKey).PermissionCode
            .ShouldBe(PermissionCodes.Administration.View);

        Visible(PermissionCodes.Administration.View).AllItems
            .ShouldContain(i => i.Key == ShellNavigation.FacilityInfoKey);
        Visible(PermissionCodes.MasterData.View).AllItems
            .ShouldNotContain(i => i.Key == ShellNavigation.FacilityInfoKey);
    }

    [Fact]
    public void BuildVisible_SignatoryConfiguration_RequiresAdministrationView()
    {
        Model().AllItems.Single(i => i.Key == ShellNavigation.SignatoryConfigurationKey).PermissionCode
            .ShouldBe(PermissionCodes.Administration.View);

        Visible(PermissionCodes.Administration.View).AllItems
            .ShouldContain(i => i.Key == ShellNavigation.SignatoryConfigurationKey);
        Visible(PermissionCodes.MasterData.View).AllItems
            .ShouldNotContain(i => i.Key == ShellNavigation.SignatoryConfigurationKey);
    }

    [Fact]
    public void BuildVisible_TilesAndShortcuts_FollowTheVisibleItems()
    {
        var withoutReceipt = Visible(PermissionCodes.MasterData.View);
        withoutReceipt.Tiles.ShouldBeEmpty();
        withoutReceipt.FindByShortcut(Keys.F2).ShouldBeNull();

        var withReceipt = Visible(PermissionCodes.CustodyIncrease.Create);
        withReceipt.Tiles.ShouldHaveSingleItem().Key.ShouldBe(ShellNavigation.DepositReceiptKey);
        withReceipt.FindByShortcut(Keys.F2)!.Key.ShouldBe(ShellNavigation.DepositReceiptKey);
    }

    [Theory]
    [InlineData(RoleCodes.Administrator)]
    [InlineData(RoleCodes.CustodyOfficer)]
    [InlineData(RoleCodes.CanteenOfficer)]
    [InlineData(RoleCodes.SupervisingOfficer)]
    [InlineData(RoleCodes.Leader)]
    [InlineData(RoleCodes.Accountant)]
    public void BuildVisible_SeededRole_SeesOnlyTheGroupsItsGrantsReach(string roleCode)
    {
        var seededGrants = DefaultRoles.All.Single(role => role.Code == roleCode).Permission;

        var model = ShellNavigation.BuildVisible(Model(), seededGrants.Contains);

        model.Groups.Select(g => g.Caption).ShouldBe(ExpectedGroups(roleCode));
    }

    // Hệ thống stays for every role: đổi mật khẩu and đăng xuất need no permission.
    private static IReadOnlyList<string> ExpectedGroups(string roleCode) => roleCode switch
    {
        RoleCodes.Administrator => ["&Danh mục", "&Hệ thống"],
        RoleCodes.CustodyOfficer => ["&Lưu ký", "&Danh mục", "&Hệ thống"],
        _ => ["&Hệ thống"],
    };
}
