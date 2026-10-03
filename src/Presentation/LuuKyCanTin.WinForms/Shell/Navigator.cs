using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.MasterData;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Composes each screen with its presenter. Presenters get scopes, never a DbContext.</summary>
internal sealed class Navigator(IServiceScopeFactory scopes) : INavigator
{
    private OfficerForm? _officer;
    private RoleForm? _roleForm;

    // One staff window at a time: a second menu click brings the open one forward.
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

    public void OpenChangePassword()
    {
        using var form = new ChangePasswordForm();
        _ = new ChangePasswordPresenter(form, scopes, isForced: false);
        form.ShowDialog();
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
