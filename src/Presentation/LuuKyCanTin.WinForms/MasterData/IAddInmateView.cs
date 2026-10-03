using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.MasterData;

public interface IAddInmateView
{
    event EventHandler? SaveClicked;

    string InmateCode { get; }

    string FullName { get; }

    short? BirthYear { get; }

    InmateType InmateType { get; }

    DateOnly AdmissionDate { get; }

    string? Cell { get; }

    /// <summary>Marks each field invalid with its message under it and focuses the first; the dialog stays open.</summary>
    void ShowFieldErrors(IReadOnlyList<FieldMessage<InmateField>> errors);

    /// <summary>Shows an error that concerns no single field in the banner above the fields; the dialog stays open.</summary>
    void ShowError(string message);

    void CloseWithResult(bool succeeded);
}
