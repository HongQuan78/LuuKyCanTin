using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Administration;

public sealed class CreateAccountPresenter
{
    private readonly ICreateAccountView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly Action<string> _showTemporaryPassword;
    private bool _isSaving;

    public CreateAccountPresenter(ICreateAccountView view, IServiceScopeFactory scopes, Action<string> showTemporaryPassword)
    {
        _view = view;
        _scopes = scopes;
        _showTemporaryPassword = showTemporaryPassword;
        _view.Loaded += async (_, _) => await LoadListsAsync();
        _view.CreateClicked += OnCreateClicked;
    }

    private static CreateAccountField? ToField(string propertyName) => propertyName switch
    {
        nameof(CreateAccountRequest.UserName) => CreateAccountField.UserName,
        nameof(CreateAccountRequest.OfficerId) => CreateAccountField.Officer,
        _ => null,
    };

    private async Task LoadListsAsync()
    {
        using var scope = _scopes.CreateScope();
        var officers = await scope.ServiceProvider.GetRequiredService<IAccountService>().GetOfficersForAccountCreationAsync();
        var roles = await scope.ServiceProvider.GetRequiredService<IRoleService>().GetAllAsync();
        _view.ShowOfficers(officers);
        _view.ShowRoles(roles);
    }

    private async void OnCreateClicked(object? sender, EventArgs e)
    {
        // A second click while the first save is still running would create the account twice.
        if (_isSaving)
            return;

        _isSaving = true;
        try
        {
            // No officer selected still goes to the service, so its validator produces the message under the field.
            var request = new CreateAccountRequest(_view.UserName, _view.OfficerId ?? 0, _view.SelectedRoleIds);
            using var scope = _scopes.CreateScope();
            var result = await scope.ServiceProvider.GetRequiredService<IAccountService>().CreateAsync(request);
            // The temporary password is shown once, then only its hash exists.
            _showTemporaryPassword(result.TemporaryPassword);
            _view.CloseAsSaved();
        }
        catch (RequestValidationException ex)
        {
            var (fieldErrors, otherMessages) = FieldMessages.Split(ex.Errors, ToField);
            if (fieldErrors.Count > 0)
                _view.ShowFieldErrors(fieldErrors);
            if (otherMessages.Count > 0)
                _view.ShowError(string.Join('\n', otherMessages));
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
            _view.ShowError($"Không tạo được tài khoản: {ex.Message}");
        }
        finally
        {
            _isSaving = false;
        }
    }
}
