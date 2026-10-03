namespace LuuKyCanTin.WinForms.MasterData;

/// <summary>The add/edit dialog for one staff member.</summary>
public interface IOfficerEditView : IDisposable
{
    event EventHandler SaveClicked;

    string Title { set; }

    string OfficerCode { get; set; }

    string FullName { get; set; }

    string Position { get; set; }

    bool IsSupervisingOfficer { get; set; }

    bool IsActive { get; set; }

    /// <summary>Shows the dialog modally; true if it closed after a successful save.</summary>
    bool DisplayText();

    /// <summary>Shows why the save failed and keeps the dialog open so the user can correct it.</summary>
    void ShowError(string message);

    void CloseAsSaved();
}
