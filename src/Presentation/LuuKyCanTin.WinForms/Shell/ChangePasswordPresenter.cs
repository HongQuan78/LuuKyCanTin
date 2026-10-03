using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

public sealed class ChangePasswordPresenter
{
    private static readonly string[] NewPasswordMessages =
    [
        PasswordPolicy.TooShortMessage,
        PasswordPolicy.MissingUpperCaseMessage,
        PasswordPolicy.MissingLowerCaseMessage,
        PasswordPolicy.MissingDigitMessage,
        PasswordPolicy.SameAsCurrentMessage,
    ];

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
        _view.InputChanged += (_, _) => EvaluateInput();
        EvaluateInput();
    }

    private void EvaluateInput()
    {
        var results = PasswordPolicy.Evaluate(_view.NewPassword, _view.CurrentPassword);
        _view.ShowRuleResults(results);
        _view.CanSave = results.All(r => r.IsSatisfied)
            && string.Equals(_view.NewPassword, _view.Confirmation, StringComparison.Ordinal);
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
            ShowServiceError(ex.Message);
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

    // The service answers with its message constants; each one belongs to the field the user has to fix.
    private void ShowServiceError(string message)
    {
        if (message == SignInService.InvalidCredentialsMessage)
            _view.ShowFieldError(PasswordField.Current, message);
        else if (message == PasswordPolicy.ConfirmationMismatchMessage)
            _view.ShowFieldError(PasswordField.Confirmation, message);
        else if (message.Split('\n').All(NewPasswordMessages.Contains))
            _view.ShowFieldError(PasswordField.New, message);
        else
            _view.ShowError(message);
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
