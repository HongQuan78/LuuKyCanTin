using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

public sealed class ChangePasswordPresenter
{
    private readonly IChangePasswordView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly bool _isForced;
    private bool _isSaving;

    /// <param name="isForced">Forced mode: cancel signs the temporary session out and returns to login.</param>
    public ChangePasswordPresenter(IChangePasswordView view, IServiceScopeFactory scopes, bool isForced)
    {
        _view = view;
        _scopes = scopes;
        _isForced = isForced;
        _view.Title = isForced ? "Đổi mật khẩu lần đầu" : "Đổi mật khẩu";
        _view.IsForced = isForced;
        _view.SaveClicked += OnSaveClicked;
        _view.CancelClicked += OnCancelClicked;
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        // A second click while the first save is still running would hash twice for nothing.
        if (_isSaving)
            return;
        _isSaving = true;
        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ChangePasswordService>()
                .ChangePasswordAsync(_view.CurrentPassword, _view.NewPassword, _view.Confirmation);
            _view.CloseWithResult(true);
        }
        catch (BusinessRuleException ex)
        {
            _view.ShowError(ex.Message);
        }
        catch (Exception ex)
        {
            _view.ShowError($"Không đổi được mật khẩu: {ex.Message}");
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async void OnCancelClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_isForced)
            {
                using var scope = _scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<SignInService>().SignOutAsync();
            }
        }
        catch (Exception ex)
        {
            _view.ShowError($"Không đăng xuất được: {ex.Message}");
            return;
        }

        _view.CloseWithResult(false);
    }
}
