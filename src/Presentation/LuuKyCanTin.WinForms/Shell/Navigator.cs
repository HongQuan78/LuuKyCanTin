using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.Custody;
using LuuKyCanTin.WinForms.MasterData;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>
/// Composes each screen with its presenter, for one shell session. Presenters get scopes, never a DbContext.
/// Screens that are not UserControls yet still open in their own window.
/// </summary>
internal sealed class Navigator(IServiceScopeFactory scopes, IContentHost host) : INavigator
{
    private OfficerForm? _officer;
    private RoleForm? _roleForm;

    public void ShowPage(string key, string title, Func<Control> create) => host.ShowPage(key, title, create);

    // One staff window at a time: a second click brings the open one forward.
    public void OpenOfficers()
    {
        if (_officer is { IsDisposed: false })
        {
            _officer.Activate();
            return;
        }

        _officer = new OfficerForm();
        _ = new OfficerPresenter(_officer, scopes, () => new OfficerEditForm());
        _officer.Show();
    }

    public void OpenAddInmate()
    {
        using var scope = scopes.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<AddInmateForm>();
        ActivatorUtilities.CreateInstance<AddInmatePresenter>(scope.ServiceProvider, form);
        form.ShowDialog(host);
    }

    public void OpenDepositReceipt()
    {
        using var scope = scopes.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<DepositReceiptForm>();
        ActivatorUtilities.CreateInstance<DepositReceiptPresenter>(scope.ServiceProvider, form);
        form.ShowDialog(host);
    }

    public void OpenChangePassword()
    {
        using var form = new ChangePasswordForm();
        _ = new ChangePasswordPresenter(form, scopes, isForced: false);
        form.ShowModal();
    }

    // One role window at a time, like the staff register.
    public void OpenRoles()
    {
        if (_roleForm is { IsDisposed: false })
        {
            _roleForm.Activate();
            return;
        }

        _roleForm = new RoleForm();
        _ = new RolePresenter(_roleForm, scopes);
        _roleForm.Show();
    }
}
