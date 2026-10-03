using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The "Thông tin đơn vị" screen: the three header lines on every printed template, with a print preview.</summary>
public interface IFacilityInfoView
{
    event EventHandler Loaded;

    event EventHandler SaveClicked;

    /// <summary>Discard the edits and reload from the database.</summary>
    event EventHandler ReloadClicked;

    string ParentAgencyName { get; }

    string FacilityName { get; }

    string Address { get; }

    void ShowFacility(FacilityInfoDto facility);

    void ShowFieldErrors(IReadOnlyList<FieldMessage<FacilityInfoField>> errors);

    void ShowMessage(string message);

    void ShowError(string message);

    /// <summary>Read-only by permission: the boxes are read-only and Lưu is hidden.</summary>
    void SetEditingEnabled(bool canEdit);

    /// <summary>Enables Lưu only once the stored row has loaded, so a click before the load cannot vanish.</summary>
    void SetSaveEnabled(bool isEnabled);
}
