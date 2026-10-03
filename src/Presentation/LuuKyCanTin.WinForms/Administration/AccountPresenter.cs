using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Administration;

public sealed class AccountPresenter
{
    private readonly IAccountView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly Func<ICreateAccountView> _createAccountDialog;
    private readonly Func<IAccountRolesView> _accountRolesDialog;
    private readonly Action<string> _showTemporaryPassword;
    private bool _isBusy;

    public AccountPresenter(
        IAccountView view,
        IServiceScopeFactory scopes,
        Func<ICreateAccountView> createAccountDialog,
        Func<IAccountRolesView> accountRolesDialog,
        Action<string> showTemporaryPassword,
        ICurrentUser currentUser)
    {
        _view = view;
        _scopes = scopes;
        _createAccountDialog = createAccountDialog;
        _accountRolesDialog = accountRolesDialog;
        _showTemporaryPassword = showTemporaryPassword;
        // Cosmetic only: the service re-checks the database before any write.
        _view.SetEditingEnabled(currentUser.HasPermission(PermissionCodes.Administration.Update));
        _view.Loaded += async (_, _) => await ReloadAsync();
        _view.AddClicked += async (_, _) => await OpenCreateAccountAsync();
        _view.RolesClicked += async (_, _) => await OpenRolesAsync();
        _view.ToggleActiveClicked += async (_, _) => await ToggleActiveAsync();
        _view.UnlockClicked += async (_, _) => await UnlockAsync();
        _view.ResetPasswordClicked += async (_, _) => await ResetPasswordAsync();
    }

    private async Task ReloadAsync()
    {
        using var scope = _scopes.CreateScope();
        var accounts = await scope.ServiceProvider.GetRequiredService<IAccountService>().GetAllAsync();
        _view.ShowAccounts(accounts);
    }

    private async Task OpenCreateAccountAsync()
    {
        using var dialog = _createAccountDialog();
        _ = new CreateAccountPresenter(dialog, _scopes, _showTemporaryPassword);
        if (dialog.ShowModal())
            await ReloadAsync();
    }

    private async Task OpenRolesAsync()
    {
        if (_view.SelectedAccount is not { } account)
            return;

        using var dialog = _accountRolesDialog();
        _ = new AccountRolesPresenter(dialog, _scopes, account);
        if (dialog.ShowModal())
            await ReloadAsync();
    }

    private async Task ToggleActiveAsync()
    {
        if (_isBusy || _view.SelectedAccount is not { } account)
            return;

        if (account.IsActive)
        {
            if (!_view.Confirm($"Ngừng hoạt động tài khoản '{account.UserName}'?"))
                return;
            await RunAsync(
                service => service.DeactivateAsync(account.Id),
                "Đã ngừng hoạt động tài khoản.");
        }
        else
        {
            await RunAsync(
                service => service.ReactivateAsync(account.Id),
                "Đã kích hoạt lại tài khoản.");
        }
    }

    private async Task UnlockAsync()
    {
        if (_isBusy || _view.SelectedAccount is not { } account)
            return;

        await RunAsync(service => service.UnlockAsync(account.Id), "Đã mở khoá tài khoản.");
    }

    private async Task ResetPasswordAsync()
    {
        if (_isBusy || _view.SelectedAccount is not { } account)
            return;
        if (!_view.Confirm($"Đặt lại mật khẩu cho tài khoản '{account.UserName}'?"))
            return;

        _isBusy = true;
        _view.ShowMessage("");
        try
        {
            using var scope = _scopes.CreateScope();
            var temporaryPassword = await scope.ServiceProvider.GetRequiredService<IAccountService>()
                .ResetPasswordAsync(account.Id);
            // The password is committed already; show it before the reload, so a reload failure loses nothing.
            _showTemporaryPassword(temporaryPassword);
            await ReloadAsync();
            _view.ShowMessage("Đã đặt lại mật khẩu.");
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
            _view.ShowError($"Không đặt lại được mật khẩu: {ex.Message}");
        }
        finally
        {
            _isBusy = false;
        }
    }

    private async Task RunAsync(Func<IAccountService, Task> operation, string successMessage)
    {
        _isBusy = true;
        _view.ShowMessage("");
        try
        {
            using var scope = _scopes.CreateScope();
            await operation(scope.ServiceProvider.GetRequiredService<IAccountService>());
            await ReloadAsync();
            _view.ShowMessage(successMessage);
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
            _view.ShowError($"Không thực hiện được: {ex.Message}");
        }
        finally
        {
            _isBusy = false;
        }
    }
}
