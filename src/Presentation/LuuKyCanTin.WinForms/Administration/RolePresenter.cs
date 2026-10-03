using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Administration;

public sealed class RolePresenter
{
    private readonly IRoleView _view;
    private readonly IServiceScopeFactory _scopes;
    private IReadOnlyList<RoleDto> _roles = [];
    private byte[]? _selectedRowVer;
    private bool _isSaving;

    public RolePresenter(IRoleView view, IServiceScopeFactory scopes, ICurrentUser currentUser)
    {
        _view = view;
        _scopes = scopes;
        // Cosmetic only: the service re-checks the database before any write.
        _view.SetEditingEnabled(currentUser.HasPermission(PermissionCodes.Administration.Update));
        _view.Loaded += async (_, _) => await LoadRolesAsync();
        _view.RoleChanged += async (_, _) => await LoadPermissionsAsync();
        _view.SaveClicked += OnSaveClicked;
        _view.DiscardClicked += OnDiscardClicked;
    }

    private async void OnDiscardClicked(object? sender, EventArgs e)
    {
        _view.ShowMessage("");
        await LoadPermissionsAsync();
    }

    private async Task LoadRolesAsync()
    {
        using var scope = _scopes.CreateScope();
        _roles = await scope.ServiceProvider.GetRequiredService<IRoleService>().GetAllAsync();
        _view.ShowRoles(_roles);

        // Selecting a role raises RoleChanged; only load here if it didn't (for example an empty list).
        if (_view.SelectedRoleId is not null)
            await LoadPermissionsAsync();
    }

    private async Task LoadPermissionsAsync()
    {
        if (_view.SelectedRoleId is not { } roleId)
            return;

        _selectedRowVer = _roles.FirstOrDefault(v => v.Id == roleId)?.RowVer;
        using var scope = _scopes.CreateScope();
        var permissionCode = await scope.ServiceProvider.GetRequiredService<IRoleService>()
            .GetPermissionsAsync(roleId);
        _view.ShowPermissions(permissionCode);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_isSaving || _view.SelectedRoleId is not { } roleId || _selectedRowVer is null)
            return;

        _isSaving = true;
        _view.ShowMessage("");
        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IRoleService>()
                .UpdatePermissionsAsync(roleId, _view.SelectedPermissions, _selectedRowVer);
            // Reload so the next save carries the new row version, then confirm (the reload clears the message).
            await LoadRolesAsync();
            _view.ShowMessage("Đã lưu quyền của vai trò.");
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
