using LuuKyCanTin.Domain.MasterData;

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

    void ShowError(string message);

    void CloseWithResult(bool succeeded);
}
