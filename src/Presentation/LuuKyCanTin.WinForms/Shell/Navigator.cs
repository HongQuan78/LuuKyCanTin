using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.Custody;
using LuuKyCanTin.WinForms.MasterData;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// Composes each screen with its presenter, for one shell session. Module screens are hosted in the content area and
/// cached by the host; dialogs open modally over the shell. Presenters get the scope factory, never a scope or a DbContext.
/// </summary>
internal sealed class Navigator(IServiceScopeFactory scopes, IContentHost host, ICurrentUser currentUser) : INavigator
{
    public const string OfficersTitle = "Danh mục cán bộ";
    public const string DepositReceiptTitle = "Lập biên nhận thu";
    public const string AccountsTitle = "Tài khoản";
    public const string RolesTitle = "Vai trò và phân quyền";
    public const string FacilityInfoTitle = "Thông tin đơn vị";
    public const string SignatoryConfigurationTitle = "Cấu hình người ký";

    public void ShowPage(string key, string title, Func<Control> create) => host.ShowPage(key, title, create);

    public void OpenOfficers() => host.ShowPage(ShellNavigation.OfficersKey, OfficersTitle, () =>
    {
        var page = new OfficerForm();
        _ = new OfficerPresenter(page, scopes, () => new OfficerEditForm(), currentUser);
        return page;
    });

    public void OpenAddInmate()
    {
        using var scope = scopes.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<AddInmateForm>();
        ActivatorUtilities.CreateInstance<AddInmatePresenter>(scope.ServiceProvider, form);
        form.ShowDialog(host);
    }

    public void OpenDepositReceipt() => host.ShowPage(ShellNavigation.DepositReceiptKey, DepositReceiptTitle, () =>
    {
        var page = new DepositReceiptForm();
        _ = new DepositReceiptPresenter(page, scopes);
        return page;
    });

    public void OpenChangePassword()
    {
        using var form = new ChangePasswordForm();
        _ = new ChangePasswordPresenter(form, scopes, isForced: false);
        form.ShowModal();
    }

    public void OpenAccounts() => host.ShowPage(ShellNavigation.AccountsKey, AccountsTitle, () =>
    {
        var page = new AccountForm();
        _ = new AccountPresenter(
            page,
            scopes,
            () => new CreateAccountForm(),
            () => new AccountRolesForm(),
            temporaryPassword =>
            {
                using var form = new TemporaryPasswordForm(temporaryPassword);
                form.ShowDialog(host);
            },
            currentUser);
        return page;
    });

    public void OpenRoles() => host.ShowPage(ShellNavigation.RolesKey, RolesTitle, () =>
    {
        var page = new RoleForm();
        _ = new RolePresenter(page, scopes, currentUser);
        return page;
    });

    public void OpenFacilityInfo() => host.ShowPage(ShellNavigation.FacilityInfoKey, FacilityInfoTitle, () =>
    {
        var page = new FacilityInfoForm();
        _ = new FacilityInfoPresenter(page, scopes, currentUser);
        return page;
    });

    public void OpenSignatoryConfiguration() => host.ShowPage(
        ShellNavigation.SignatoryConfigurationKey, SignatoryConfigurationTitle, () =>
    {
        var page = new SignatoryConfigurationForm();
        _ = new SignatoryConfigurationPresenter(page, scopes, currentUser);
        return page;
    });

    public void OpenLockScreen(
        string title, string initials, string displayName, Action onUnlocked, Action onSignedOut)
    {
        var form = new LockScreenForm();
        _ = new LockScreenPresenter(form, scopes, title, initials, displayName, onUnlocked, onSignedOut);
        // Show, not ShowDialog: open dialogs keep their state, and the overlay disables them itself.
        form.Show(host);
    }
}
