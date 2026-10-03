using FluentValidation.Results;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.MasterData;

public sealed class OfficerEditPresenter
{
    private readonly IOfficerEditView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly OfficerDto? _officer;
    private bool _isSaving;

    /// <param name="officer">The staff member to edit, or null to add one.</param>
    public OfficerEditPresenter(IOfficerEditView view, IServiceScopeFactory scopes, OfficerDto? officer)
    {
        _view = view;
        _scopes = scopes;
        _officer = officer;
        _view.SaveClicked += OnSaveClicked;

        if (officer is null)
        {
            _view.Title = "Thêm cán bộ";
            _view.ShowHeading("Thêm cán bộ", "Nhập thông tin cán bộ mới.");
            _view.IsActive = true;
            return;
        }

        _view.Title = "Sửa cán bộ";
        _view.ShowHeading(officer.FullName, string.IsNullOrEmpty(officer.Position)
            ? officer.OfficerCode
            : $"{officer.OfficerCode} · {officer.Position}");
        _view.OfficerCode = officer.OfficerCode;
        _view.FullName = officer.FullName;
        _view.Position = officer.Position ?? "";
        _view.IsSupervisingOfficer = officer.IsSupervisingOfficer;
        _view.IsActive = officer.IsActive;
    }

    private static OfficerField? ToField(string propertyName) => propertyName switch
    {
        nameof(SaveOfficerRequest.OfficerCode) => OfficerField.OfficerCode,
        nameof(SaveOfficerRequest.FullName) => OfficerField.FullName,
        nameof(SaveOfficerRequest.Position) => OfficerField.Position,
        _ => null,
    };

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        // A second click while the first save is still running would add the same person twice.
        if (_isSaving)
            return;
        _isSaving = true;
        try
        {
            var request = new SaveOfficerRequest(_view.OfficerCode, _view.FullName, _view.Position, _view.IsSupervisingOfficer, _view.IsActive);
            using var scope = _scopes.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IOfficerService>();
            if (_officer is null)
                await service.AddAsync(request);
            else
                await service.UpdateAsync(_officer.Id, request with { RowVer = _officer.RowVer });
            _view.CloseAsSaved();
        }
        catch (RequestValidationException ex)
        {
            ShowErrors(ex.Errors);
        }
        catch (BusinessRuleException ex) when (ex.Message == OfficerService.DuplicateCodeMessage)
        {
            _view.ShowFieldErrors([new(OfficerField.OfficerCode, ex.Message)]);
        }
        catch (BusinessRuleException ex)
        {
            _view.ShowError(ex.Message);
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
