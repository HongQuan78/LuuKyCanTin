using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class ShellNavigationTests
{
    private readonly INavigator _navigator = Substitute.For<INavigator>();
    private int _homeCount;
    private int _signOutCount;
    private int _lockCount;

    private NavigationModel Create() =>
        ShellNavigation.Create(_navigator, () => _homeCount++, () => _signOutCount++, () => _lockCount++);

    [Fact]
    public void Create_Sections_ListBusinessThenManagementGroupsWithBuiltScreensOnly()
    {
        var model = Create();

        model.Home.Caption.ShouldBe("&Trang chủ");
        model.Groups.Select(g => (g.Section, g.Caption)).ShouldBe(
        [
            ("NGHIỆP VỤ", "&Lưu ký"),
            ("QUẢN LÝ", "&Danh mục"),
            ("QUẢN LÝ", "&Hệ thống"),
        ]);
    }

    [Theory]
    [InlineData(ShellNavigation.DepositReceiptKey)]
    [InlineData(ShellNavigation.OfficersKey)]
    [InlineData(ShellNavigation.AddInmateKey)]
    [InlineData(ShellNavigation.AccountsKey)]
    [InlineData(ShellNavigation.RolesKey)]
    [InlineData(ShellNavigation.FacilityInfoKey)]
    [InlineData(ShellNavigation.ChangePasswordKey)]
    public void Open_EachScreenItem_CallsItsNavigatorMethod(string key)
    {
        Item(Create(), key).Open();

        var expected = key switch
        {
            ShellNavigation.DepositReceiptKey => nameof(INavigator.OpenDepositReceipt),
            ShellNavigation.OfficersKey => nameof(INavigator.OpenOfficers),
            ShellNavigation.AddInmateKey => nameof(INavigator.OpenAddInmate),
            ShellNavigation.AccountsKey => nameof(INavigator.OpenAccounts),
            ShellNavigation.RolesKey => nameof(INavigator.OpenRoles),
            ShellNavigation.FacilityInfoKey => nameof(INavigator.OpenFacilityInfo),
            _ => nameof(INavigator.OpenChangePassword),
        };
        _navigator.ReceivedCalls().Select(c => c.GetMethodInfo().Name).ShouldBe([expected]);
    }

    [Fact]
    public void Open_HomeLockAndSignOut_CallTheShellActions()
    {
        var model = Create();

        model.Home.Open();
        Item(model, ShellNavigation.LockSessionKey).Open();
        Item(model, ShellNavigation.SignOutKey).Open();

        _homeCount.ShouldBe(1);
        _lockCount.ShouldBe(1);
        _signOutCount.ShouldBe(1);
        _navigator.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public void LockSession_IsInTheSystemGroupOnCtrlLWithoutAPermission()
    {
        var item = Item(Create(), ShellNavigation.LockSessionKey);

        item.Caption.ShouldBe("Kh&oá máy");
        item.PermissionCode.ShouldBeNull();
        item.Shortcut.ShouldBe(Keys.Control | Keys.L);
        item.ShortcutText.ShouldBe("Ctrl+L");
    }

    [Fact]
    public void Tiles_Today_AreOnlyTheDepositReceiptOnF2()
    {
        var tile = Create().Tiles.ShouldHaveSingleItem();

        tile.Key.ShouldBe(ShellNavigation.DepositReceiptKey);
        tile.Shortcut.ShouldBe(Keys.F2);
        tile.Tile!.Title.ShouldBe("Lập biên nhận thu");
    }

    [Fact]
    public void FindByShortcut_F2_FindsTheDepositReceipt()
    {
        var model = Create();

        model.FindByShortcut(Keys.F2)!.Key.ShouldBe(ShellNavigation.DepositReceiptKey);
        model.FindByShortcut(Keys.F3).ShouldBeNull();
        model.FindByShortcut(Keys.None).ShouldBeNull();
    }

    [Fact]
    public void Captions_AcrossTheSidebar_HaveUniqueMnemonics()
    {
        var letters = Create().AllItems.Select(i => i.Caption)
            .Concat(Create().Groups.Select(g => g.Caption))
            .Select(MnemonicAssert.GetMnemonic)
            .ToList();

        letters.ShouldAllBe(letter => letter != null);
        letters.ShouldBeUnique();
    }

    private static NavItem Item(NavigationModel model, string key) => model.AllItems.Single(i => i.Key == key);
}
