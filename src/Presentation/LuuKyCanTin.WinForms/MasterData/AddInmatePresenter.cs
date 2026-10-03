using LuuKyCanTin.Application.MasterData;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.MasterData;

/// <summary>The skeleton's minimal detainee form. Search, edit and type history are Epic 3.</summary>
public sealed class AddInmatePresenter
{
    private readonly IAddInmateView _view;
    private readonly IServiceScopeFactory _scopeFactory;

    public AddInmatePresenter(IAddInmateView view, IServiceScopeFactory scopeFactory)
    {
        _view = view;
        _scopeFactory = scopeFactory;
        _view.SaveClicked += async (_, _) =>
        {
            try
            {
                await SaveAsync();
            }
            catch (Exception ex)
            {
                // An unexpected failure (database down) must still reach the user, not the global handler.
                _view.ShowError($"Không lưu được đối tượng: {ex.Message}");
            }
        };
    }

    public async Task SaveAsync()
    {
        // Every operation gets a fresh scope; the form never holds a DbContext.
        await using var scope = _scopeFactory.CreateAsyncScope();
        var addInmate = scope.ServiceProvider.GetRequiredService<AddInmateService>();

        var request = new AddInmateRequest(
            _view.InmateCode, _view.FullName, _view.BirthYear, _view.InmateType, _view.AdmissionDate, _view.Cell);

        var result = await addInmate.AddAsync(request);
        if (result.Succeeded)
            _view.CloseWithResult(true);
        else
            _view.ShowError(result.Message ?? "Không lưu được đối tượng.");
    }
}
