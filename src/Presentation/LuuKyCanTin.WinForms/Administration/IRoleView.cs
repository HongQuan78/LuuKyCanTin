using LuuKyCanTin.Application.Administration;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The "Vai trò" screen: role list on the left, module × action checkboxes on the right.</summary>
public interface IRoleView
{
    event EventHandler Loaded;

    /// <summary>The selected role changed; load its permission set.</summary>
    event EventHandler RoleChanged;

    event EventHandler SaveClicked;

    /// <summary>Discard the edits and reload from the database.</summary>
    event EventHandler DiscardClicked;

    int? SelectedRoleId { get; }

    /// <summary>Every ticked permission code, in the grid and in the special list.</summary>
    IReadOnlyList<string> SelectedPermissions { get; }

    void ShowRoles(IReadOnlyList<RoleDto> items);

    void ShowPermissions(IReadOnlyList<string> permissionCode);

    void ShowMessage(string message);

    void ShowError(string message);
}
