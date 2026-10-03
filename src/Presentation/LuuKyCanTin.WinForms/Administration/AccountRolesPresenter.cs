using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Administration;

public sealed class AccountRolesPresenter
{
    private readonly IAccountRolesView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly AccountDto _account;
    private bool _isSaving;

    public AccountRolesPresenter(IAccountRolesView view, IServiceScopeFactory scopes, AccountDto account)
    {
        _view = view;
        _scopes = scopes;
        _account = account;
        _view.Title = $"Phân vai trò — {account.UserName}";
        _view.Loaded += async (_, _) => await LoadAsync();
        _view.SaveClicked += OnSaveClicked;
    }

    private async Task LoadAsync()
    {
        using var scope = _scopes.CreateScope();
        var roles = await scope.ServiceProvider.GetRequiredService<IRoleService>().GetAllAsync();
        _view.ShowRoles(roles);
        _view.ShowSelectedRoles(_account.RoleIds);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_isSaving)
            return;

        _isSaving = true;
        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IAccountService>()
                .UpdateRolesAsync(_account.Id, _view.SelectedRoleIds);
            _view.CloseAsSaved();
        }
        catch (BusinessRuleException ex)
        {
            _view.ShowError(ex.Message);
        }
        catch (PermissionDeniedException ex)
        {
            _view.ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            _view.ShowError($"Không lưu được: {ex.Message}");
        }
        finally
        {
            _isSaving = false;
        }
    }
}
