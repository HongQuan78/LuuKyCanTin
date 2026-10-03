using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.WinForms.Common;
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

    private static InmateField? ToField(string propertyName) => propertyName switch
    {
        nameof(AddInmateRequest.InmateCode) => InmateField.InmateCode,
        nameof(AddInmateRequest.BirthYear) => InmateField.BirthYear,
        nameof(AddInmateRequest.FullName) => InmateField.FullName,
        nameof(AddInmateRequest.InmateType) => InmateField.InmateType,
        nameof(AddInmateRequest.AdmissionDate) => InmateField.AdmissionDate,
        nameof(AddInmateRequest.Cell) => InmateField.Cell,
        _ => null,
    };

    public async Task SaveAsync()
    {
        // Every operation gets a fresh scope; the form never holds a DbContext.
        await using var scope = _scopeFactory.CreateAsyncScope();
        var addInmate = scope.ServiceProvider.GetRequiredService<AddInmateService>();

        var request = new AddInmateRequest(
            _view.InmateCode, _view.FullName, _view.BirthYear, _view.InmateType, _view.AdmissionDate, _view.Cell);

        var result = await addInmate.AddAsync(request);
        if (result.Succeeded)
        {
            _view.CloseWithResult(true);
            return;
        }

        if (result.Message == AddInmateService.DuplicateCodeMessage)
        {
            _view.ShowFieldErrors([new(InmateField.InmateCode, result.Message)]);
            return;
        }

        var (fieldErrors, otherMessages) = FieldMessages.Split(result.Errors, ToField);
        if (fieldErrors.Count > 0)
            _view.ShowFieldErrors(fieldErrors);
        if (otherMessages.Count > 0)
            _view.ShowError(string.Join('\n', otherMessages));
        else if (fieldErrors.Count == 0)
            _view.ShowError(result.Message ?? "Không lưu được đối tượng.");
    }
}
