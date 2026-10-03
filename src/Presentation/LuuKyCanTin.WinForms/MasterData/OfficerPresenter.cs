using LuuKyCanTin.Application.MasterData;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.MasterData;

public sealed class OfficerPresenter
{
    private readonly IOfficerView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly Func<IOfficerEditView> _createDialog;
    private int _searchVersion;

    public OfficerPresenter(IOfficerView view, IServiceScopeFactory scopes, Func<IOfficerEditView> createDialog)
    {
        _view = view;
        _scopes = scopes;
        _createDialog = createDialog;
        _view.Loaded += async (_, _) => await ReloadAsync();
        _view.SearchChanged += async (_, _) => await ReloadAsync();
        _view.AddClicked += async (_, _) => await OpenDialogAsync(officer: null);
        _view.EditClicked += async (_, _) =>
        {
            if (_view.SelectedOfficer is { } officer)
                await OpenDialogAsync(officer);
        };
    }

    private async Task ReloadAsync()
    {
        // Searches overlap while the user types; a slow earlier one must not overwrite the latest result.
        var version = ++_searchVersion;
        using var scope = _scopes.CreateScope();
        var items = await scope.ServiceProvider.GetRequiredService<IOfficerService>()
            .SearchAsync(_view.Keyword, _view.ShowInactive);
        if (version == _searchVersion)
            _view.ShowList(items);
    }

    private async Task OpenDialogAsync(OfficerDto? officer)
    {
        bool saved;
        using (var dialog = _createDialog())
        {
            _ = new OfficerEditPresenter(dialog, _scopes, officer);
            saved = dialog.DisplayText();
        }

        if (saved)
            await ReloadAsync();
    }
}
