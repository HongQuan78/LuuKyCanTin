using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The "Cấu hình người ký" screen: the template list on the left, the signer grid and preview on the right.</summary>
public interface ISignatoryConfigurationView
{
    event EventHandler Loaded;

    /// <summary>The selected template changed; load its signer rows.</summary>
    event EventHandler TemplateChanged;

    event EventHandler AddLineClicked;

    event EventHandler RemoveLineClicked;

    event EventHandler MoveUpClicked;

    event EventHandler MoveDownClicked;

    event EventHandler SaveClicked;

    /// <summary>Discard the edits and reload from the database.</summary>
    event EventHandler DiscardClicked;

    string? SelectedTemplateCode { get; }

    int? SelectedLineIndex { get; }

    /// <summary>The grid's current signer lines, in printed order.</summary>
    IReadOnlyList<SignatoryLine> Lines { get; }

    void ShowTemplates(IReadOnlyList<TemplateCodes.TemplateDefinition> templates);

    /// <summary>Binds the stored rows and the officer choices: active staff, plus any retired default already in use.</summary>
    void ShowSignatories(IReadOnlyList<SignatoryRowDto> rows, IReadOnlyList<OfficerDto> activeOfficers);

    /// <summary>Re-binds the grid after an add, remove or reorder; <paramref name="selectedIndex"/> keeps the moved row selected.</summary>
    void ShowLines(IReadOnlyList<SignatoryLine> lines, int selectedIndex);

    /// <summary>Read-only by permission: the grid is read-only and the write buttons are disabled.</summary>
    void SetEditingEnabled(bool canEdit);

    /// <summary>Enables Lưu only once the selected template's rows have loaded, so a click before the load can't save nothing.</summary>
    void SetSaveEnabled(bool isEnabled);

    void ShowMessage(string message);

    void ShowError(string message);
}
