using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
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
            _view.IsActive = true;
            return;
        }

        _view.Title = "Sửa cán bộ";
        _view.OfficerCode = officer.OfficerCode;
        _view.FullName = officer.FullName;
        _view.Position = officer.Position ?? "";
        _view.IsSupervisingOfficer = officer.IsSupervisingOfficer;
        _view.IsActive = officer.IsActive;
    }

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
        catch (BusinessRuleException ex)
        {
            _view.ShowError(ex.Message);
        }
        finally
        {
            _isSaving = false;
        }
    }
}
