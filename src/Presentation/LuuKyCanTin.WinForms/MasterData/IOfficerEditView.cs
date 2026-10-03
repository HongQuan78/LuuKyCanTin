using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.MasterData;

/// <summary>The add/edit dialog for one staff member.</summary>
public interface IOfficerEditView : IDisposable
{
    event EventHandler SaveClicked;

    /// <summary>The window caption.</summary>
    string Title { set; }

    string OfficerCode { get; set; }

    string FullName { get; set; }

    string Position { get; set; }

    bool IsSupervisingOfficer { get; set; }

    bool IsActive { get; set; }

    /// <summary>The dialog head: the title (the person's name on Sửa) and a muted subtitle.</summary>
    void ShowHeading(string heading, string subtitle);

    /// <summary>Shows the dialog modally; true if it closed after a successful save.</summary>
    bool ShowModal();

    /// <summary>Marks each field invalid with its message under it and focuses the first; the dialog stays open.</summary>
    void ShowFieldErrors(IReadOnlyList<FieldMessage<OfficerField>> errors);

    /// <summary>Shows an error that concerns no single field in the banner above the fields; the dialog stays open.</summary>
    void ShowError(string message);

    void CloseAsSaved();
}
