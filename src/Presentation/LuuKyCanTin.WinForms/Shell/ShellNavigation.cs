using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// The sidebar entries for the screens built so far, every one shown as the old menu did. A permission-driven
/// registry will replace this static list; groups without a built screen (Căn tin, Báo cáo) are left out.
/// </summary>
public static class ShellNavigation
{
    public const string HomeKey = "home";
    public const string DepositReceiptKey = "custody.deposit-receipt";
    public const string OfficersKey = "master-data.officers";
    public const string AddInmateKey = "master-data.add-inmate";
    public const string RolesKey = "administration.roles";
    public const string ChangePasswordKey = "administration.change-password";
    public const string SignOutKey = "administration.sign-out";

    private const string BusinessSection = "NGHIỆP VỤ";
    private const string ManagementSection = "QUẢN LÝ";

    public static NavigationModel Create(INavigator navigator, Action showHome, Action signOut) => new(
        new NavItem(HomeKey, "&Trang chủ", showHome) { Glyph = Glyphs.Home },
        [
            new NavGroup("custody", BusinessSection, "&Lưu ký", Glyphs.Custody,
            [
                new NavItem(DepositReceiptKey, "Lập &biên nhận thu…", navigator.OpenDepositReceipt)
                {
                    Shortcut = Keys.F2,
                    Tile = new NavTile("Lập biên nhận thu", "Nhận tiền gửi lưu ký cho đối tượng", Glyphs.Add),
                },
            ]),
            new NavGroup("master-data", ManagementSection, "&Danh mục", Glyphs.MasterData,
            [
                new NavItem(OfficersKey, "&Cán bộ", navigator.OpenOfficers),
                new NavItem(AddInmateKey, "Thêm đố&i tượng…", navigator.OpenAddInmate),
            ]),
            new NavGroup("administration", ManagementSection, "&Hệ thống", Glyphs.Administration,
            [
                new NavItem(RolesKey, "&Vai trò", navigator.OpenRoles),
                new NavItem(ChangePasswordKey, "Đổi &mật khẩu", navigator.OpenChangePassword),
                new NavItem(SignOutKey, "Đăng &xuất", signOut),
            ]),
        ]);
}
