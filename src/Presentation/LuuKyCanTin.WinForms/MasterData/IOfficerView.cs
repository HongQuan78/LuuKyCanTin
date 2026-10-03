using LuuKyCanTin.Application.MasterData;

namespace LuuKyCanTin.WinForms.MasterData;

/// <summary>The staff list with its search box.</summary>
public interface IOfficerView
{
    event EventHandler Loaded;

    /// <summary>Raised once typing pauses, or when the "show staff who left" box changes.</summary>
    event EventHandler SearchChanged;

    event EventHandler AddClicked;

    event EventHandler EditClicked;

    string Keyword { get; }

    bool ShowInactive { get; }

    OfficerDto? SelectedOfficer { get; }

    void ShowList(IReadOnlyList<OfficerDto> items);
}
