using LuuKyCanTin.Application.Administration;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The "Tài khoản" screen: the account grid and its administration actions.</summary>
public interface IAccountView
{
    event EventHandler Loaded;

    event EventHandler AddClicked;

    event EventHandler RolesClicked;

    /// <summary>Deactivates the selected account, or reactivates it when it is already off.</summary>
    event EventHandler ToggleActiveClicked;

    event EventHandler UnlockClicked;

    event EventHandler ResetPasswordClicked;

    AccountDto? SelectedAccount { get; }

    void ShowAccounts(IReadOnlyList<AccountDto> items);

    /// <summary>Read-only by permission: every write action is disabled, the list still opens.</summary>
    void SetEditingEnabled(bool canEdit);

    /// <summary>Asks the user to confirm a destructive action; true means go ahead.</summary>
    bool Confirm(string message);

    void ShowMessage(string message);

    void ShowError(string message);
}
