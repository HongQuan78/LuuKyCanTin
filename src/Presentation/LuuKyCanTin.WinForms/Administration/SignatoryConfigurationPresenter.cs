using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Administration;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Administration;

public sealed class SignatoryConfigurationPresenter
{
    private readonly ISignatoryConfigurationView _view;
    private readonly IServiceScopeFactory _scopes;
    private bool _isSaving;

    public SignatoryConfigurationPresenter(ISignatoryConfigurationView view, IServiceScopeFactory scopes, ICurrentUser currentUser)
    {
        _view = view;
        _scopes = scopes;
        // Cosmetic only: the service re-checks the database before any write.
        _view.SetEditingEnabled(currentUser.HasPermission(PermissionCodes.Administration.Update));
        // Lưu stays disabled until the selected template's rows have loaded.
        _view.SetSaveEnabled(false);
        _view.Loaded += OnLoaded;
        _view.TemplateChanged += OnTemplateChanged;
        _view.AddLineClicked += OnAddLineClicked;
        _view.RemoveLineClicked += OnRemoveLineClicked;
        _view.MoveUpClicked += OnMoveUpClicked;
        _view.MoveDownClicked += OnMoveDownClicked;
        _view.SaveClicked += OnSaveClicked;
        _view.DiscardClicked += OnDiscardClicked;
    }

    private async void OnLoaded(object? sender, EventArgs e)
    {
        _view.ShowTemplates(TemplateCodes.All);
        // ShowTemplates selects the first template; the view suppresses its event while binding, so load it here.
        if (_view.SelectedTemplateCode is not null)
            await LoadAsync();
    }

    private async void OnTemplateChanged(object? sender, EventArgs e)
    {
        // A message about the previous template no longer applies.
        _view.ShowMessage("");
        await LoadAsync();
    }

    private async void OnDiscardClicked(object? sender, EventArgs e)
    {
        _view.ShowMessage("");
        await LoadAsync();
    }

    /// <summary>Loads the selected template. False when nothing was loaded or the selection changed meanwhile.</summary>
    private async Task<bool> LoadAsync()
    {
        if (_view.SelectedTemplateCode is not { } templateCode)
            return false;

        try
        {
            using var scope = _scopes.CreateScope();
            var rows = await scope.ServiceProvider.GetRequiredService<ISignatoryConfigurationService>()
                .GetByTemplateAsync(templateCode);
            var activeOfficers = await scope.ServiceProvider.GetRequiredService<IOfficerService>()
                .GetActiveOfficersAsync(supervisingOnly: false);

            // The user may have switched template while the load was in flight; those rows are not this screen's.
            if (_view.SelectedTemplateCode != templateCode)
                return false;

            _view.ShowSignatories(rows, activeOfficers);
            _view.SetSaveEnabled(true);
            return true;
        }
        catch (Exception ex)
        {
            if (_view.SelectedTemplateCode != templateCode)
                return false;

            // Never leave the previous template's rows on screen under the failed template's header.
            _view.ShowSignatories([], []);
            _view.SetSaveEnabled(false);
            _view.ShowError($"Không tải được cấu hình người ký: {ex.Message}");
            return false;
        }
    }

    private void OnAddLineClicked(object? sender, EventArgs e)
    {
        var lines = _view.Lines.ToList();
        lines.Add(new SignatoryLine(""));
        _view.ShowLines(lines, lines.Count - 1);
    }

    private void OnRemoveLineClicked(object? sender, EventArgs e)
    {
        if (_view.SelectedLineIndex is not { } index)
            return;

        var lines = _view.Lines.ToList();
        // AC 3: a template always keeps at least one signer. Checked here, before any save, so nothing is written.
        if (lines.Count <= 1)
        {
            _view.ShowError(SignatoryConfigurationService.AtLeastOneSignatoryMessage);
            return;
        }

        if (index >= lines.Count)
            return;

        lines.RemoveAt(index);
        _view.ShowLines(lines, Math.Min(index, lines.Count - 1));
    }

    private void OnMoveUpClicked(object? sender, EventArgs e)
    {
        if (_view.SelectedLineIndex is not { } index || index <= 0)
            return;

        var lines = _view.Lines.ToList();
        (lines[index - 1], lines[index]) = (lines[index], lines[index - 1]);
        _view.ShowLines(lines, index - 1);
    }

    private void OnMoveDownClicked(object? sender, EventArgs e)
    {
        if (_view.SelectedLineIndex is not { } index)
            return;

        var lines = _view.Lines.ToList();
        if (index < 0 || index >= lines.Count - 1)
            return;

        (lines[index + 1], lines[index]) = (lines[index], lines[index + 1]);
        _view.ShowLines(lines, index + 1);
    }

    private async void OnSaveClicked(object? sender, EventArgs e)
    {
        if (_isSaving || _view.SelectedTemplateCode is not { } templateCode)
            return;

        _isSaving = true;
        _view.ShowMessage("");
        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ISignatoryConfigurationService>()
                .SaveAsync(templateCode, _view.Lines);
            // Reload so a later edit starts from the saved rows; confirm only when that reload succeeded, so a
            // failed reload's error is never overwritten by a success message.
            if (_view.SelectedTemplateCode == templateCode && await LoadAsync())
                _view.ShowMessage("Đã lưu cấu hình người ký.");
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
