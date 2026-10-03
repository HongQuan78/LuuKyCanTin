using FluentValidation.Results;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Administration;

public sealed class FacilityInfoPresenter
{
    private readonly IFacilityInfoView _view;
    private readonly IServiceScopeFactory _scopes;
    private FacilityInfoDto? _facility;
    private bool _isSaving;

    public FacilityInfoPresenter(IFacilityInfoView view, IServiceScopeFactory scopes, ICurrentUser currentUser)
    {
        _view = view;
        _scopes = scopes;
        // Cosmetic only: the service re-checks the database before any write.
        _view.SetEditingEnabled(currentUser.HasPermission(PermissionCodes.Administration.Update));
        // Lưu stays disabled until the stored row has loaded, so a click before the load can't silently do nothing.
        _view.SetSaveEnabled(false);
        _view.Loaded += async (_, _) => await LoadAsync();
        _view.ReloadClicked += async (_, _) => await LoadAsync();
        _view.SaveClicked += OnSaveClicked;
    }

    private static FacilityInfoField? ToField(string propertyName) => propertyName switch
    {
        nameof(SaveFacilityInfoRequest.ParentAgencyName) => FacilityInfoField.ParentAgencyName,
        nameof(SaveFacilityInfoRequest.FacilityName) => FacilityInfoField.FacilityName,
        nameof(SaveFacilityInfoRequest.Address) => FacilityInfoField.Address,
        _ => null,
    };

    private async Task<bool> LoadAsync()
    {
        try
        {
            using var scope = _scopes.CreateScope();
            var facility = await scope.ServiceProvider.GetRequiredService<IFacilityInfoService>().GetAsync();
            _facility = facility;
            _view.ShowFacility(facility);
            _view.SetSaveEnabled(true);
            return true;
        }
        catch (Exception ex)
        {
            _facility = null;
            _view.SetSaveEnabled(false);
            _view.ShowError($"Không tải được thông tin đơn vị: {ex.Message}");
            return false;
        }
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        // A second click while the first save is still running would send the same row version twice.
        if (_isSaving || _facility is null)
            return;
        _isSaving = true;
        try
        {
            var request = new SaveFacilityInfoRequest(
                _view.ParentAgencyName, _view.FacilityName, _view.Address, _facility.RowVer);
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IFacilityInfoService>().SaveAsync(request);
            // Reload so the next save carries the new row version; the reload itself reports a failure.
            if (await LoadAsync())
                _view.ShowMessage("Đã lưu thông tin đơn vị.");
        }
        catch (RequestValidationException ex)
        {
            ShowErrors(ex.Errors);
        }
        catch (PermissionDeniedException ex)
        {
            _view.ShowError(ex.Message);
        }
        catch (BusinessRuleException ex)
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

    private void ShowErrors(IReadOnlyList<ValidationFailure> errors)
    {
        var (fieldErrors, otherMessages) = FieldMessages.Split(errors, ToField);
        if (fieldErrors.Count > 0)
            _view.ShowFieldErrors(fieldErrors);
        if (otherMessages.Count > 0)
            _view.ShowError(string.Join('\n', otherMessages));
    }
}
