using LuuKyCanTin.Application.Administration;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The role dialog for one account: a checked list of the 6 standard roles.</summary>
public interface IAccountRolesView : IDisposable
{
    event EventHandler Loaded;

    event EventHandler SaveClicked;

    string Title { set; }

    IReadOnlyList<int> SelectedRoleIds { get; }

    void ShowRoles(IReadOnlyList<RoleDto> items);

    /// <summary>Ticks the roles the account already holds.</summary>
    void ShowSelectedRoles(IReadOnlyList<int> roleIds);

    /// <summary>Shows the dialog modally; true if it closed after a successful save.</summary>
    bool ShowModal();

    void ShowError(string message);

    void CloseAsSaved();
}
