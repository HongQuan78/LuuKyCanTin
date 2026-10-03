using LuuKyCanTin.Application.MasterData;

namespace LuuKyCanTin.WinForms.MasterData;

/// <summary>The staff list with its search box and Trạng thái filter.</summary>
public interface IOfficerView
{
    event EventHandler Loaded;

    /// <summary>Raised once typing pauses, or when the Trạng thái filter changes.</summary>
    event EventHandler SearchChanged;

    event EventHandler AddClicked;

    event EventHandler EditClicked;

    string Keyword { get; }

    /// <summary>True when the filter is "Tất cả", so staff who left are listed too.</summary>
    bool ShowInactive { get; }

    OfficerDto? SelectedOfficer { get; }

    void ShowList(IReadOnlyList<OfficerDto> items);

    /// <summary>Read-only by permission: Thêm and Sửa are disabled, the list still opens.</summary>
    void SetEditingEnabled(bool canAdd, bool canEdit);
}
