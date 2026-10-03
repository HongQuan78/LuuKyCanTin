using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// The sidebar entries for the screens built so far, each declaring the permission it needs. Later stories add
/// their own entries; groups without a built screen (Căn tin, Báo cáo) are left out.
/// </summary>
public static class ShellNavigation
{
    public const string HomeKey = "home";
    public const string DepositReceiptKey = "custody.deposit-receipt";
    public const string OfficersKey = "master-data.officers";
    public const string AddInmateKey = "master-data.add-inmate";
    public const string AccountsKey = "administration.accounts";
    public const string RolesKey = "administration.roles";
    public const string FacilityInfoKey = "administration.facility-info";
    public const string ChangePasswordKey = "administration.change-password";
    public const string LockSessionKey = "administration.lock-session";
    public const string SignOutKey = "administration.sign-out";

    private const string BusinessSection = "NGHIỆP VỤ";
    private const string ManagementSection = "QUẢN LÝ";

    public static NavigationModel Create(INavigator navigator, Action showHome, Action signOut, Action lockSession) => new(
        new NavItem(HomeKey, "&Trang chủ", showHome) { Glyph = Glyphs.Home },
        [
            new NavGroup("custody", BusinessSection, "&Lưu ký", Glyphs.Custody,
            [
                new NavItem(DepositReceiptKey, "Lập &biên nhận thu…", navigator.OpenDepositReceipt)
                {
                    PermissionCode = PermissionCodes.CustodyIncrease.Create,
                    Shortcut = Keys.F2,
                    Tile = new NavTile("Lập biên nhận thu", "Nhận tiền gửi lưu ký cho đối tượng", Glyphs.Add),
                },
            ]),
            new NavGroup("master-data", ManagementSection, "&Danh mục", Glyphs.MasterData,
            [
                new NavItem(OfficersKey, "&Cán bộ", navigator.OpenOfficers)
                {
                    PermissionCode = PermissionCodes.MasterData.View,
                },
                new NavItem(AddInmateKey, "Thêm đố&i tượng…", navigator.OpenAddInmate)
                {
                    PermissionCode = PermissionCodes.MasterData.Create,
                },
            ]),
            new NavGroup("administration", ManagementSection, "&Hệ thống", Glyphs.Administration,
            [
                new NavItem(AccountsKey, "Tài &khoản", navigator.OpenAccounts)
                {
                    PermissionCode = PermissionCodes.Administration.View,
                },
                new NavItem(RolesKey, "&Vai trò", navigator.OpenRoles)
                {
                    PermissionCode = PermissionCodes.Administration.View,
                },
                new NavItem(FacilityInfoKey, "Thông tin đơ&n vị", navigator.OpenFacilityInfo)
                {
                    PermissionCode = PermissionCodes.Administration.View,
                },
                // Change password, lock and sign-out act on the caller's own session: every signed-in user sees them.
                new NavItem(ChangePasswordKey, "Đổi &mật khẩu", navigator.OpenChangePassword),
                new NavItem(LockSessionKey, "Kh&oá máy", lockSession) { Shortcut = Keys.Control | Keys.L },
                new NavItem(SignOutKey, "Đăng &xuất", signOut),
            ]),
        ]);

    /// <summary>
    /// The pure builder of the model a user may see: an item without its permission disappears, a group left with
    /// no item disappears, and Trang chủ always stays. Tiles and shortcuts follow the visible items.
    /// </summary>
    public static NavigationModel BuildVisible(NavigationModel source, Func<string, bool> hasPermission) => new(
        source.Home,
        [
            .. source.Groups
                .Select(group => group with
                {
                    Items = [.. group.Items.Where(item => item.PermissionCode is null || hasPermission(item.PermissionCode))],
                })
                .Where(group => group.Items.Count > 0),
        ]);
}
