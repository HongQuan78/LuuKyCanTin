using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.MasterData;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The create-account dialog: one active staff member without an account, plus at least one role.</summary>
public interface ICreateAccountView : IDisposable
{
    event EventHandler Loaded;

    event EventHandler CreateClicked;

    string UserName { get; }

    int? OfficerId { get; }

    IReadOnlyList<int> SelectedRoleIds { get; }

    void ShowOfficers(IReadOnlyList<OfficerDto> items);

    void ShowRoles(IReadOnlyList<RoleDto> items);

    /// <summary>Shows the dialog modally; true if it closed after a successful create.</summary>
    bool ShowModal();

    void ShowError(string message);

    void CloseAsSaved();
}
