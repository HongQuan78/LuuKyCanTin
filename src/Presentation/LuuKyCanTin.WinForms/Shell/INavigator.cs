namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Opens the app's screens, so the shell's presenter and view never create a screen themselves.</summary>
public interface INavigator
{
    /// <summary>
    /// Shows a screen in the shell's content area and its title in the header bar. The screen is created by
    /// <paramref name="create"/> on first use and cached for the session, so its state survives switching screens.
    /// </summary>
    void ShowPage(string key, string title, Func<Control> create);

    /// <summary>Shows the staff register in the content area.</summary>
    void OpenOfficers();

    /// <summary>Opens the add-detainee dialog.</summary>
    void OpenAddInmate();

    /// <summary>Shows the deposit-receipt screen in the content area.</summary>
    void OpenDepositReceipt();

    /// <summary>Shows the user-account screen in the content area.</summary>
    void OpenAccounts();

    /// <summary>Shows the role and permission screen in the content area.</summary>
    void OpenRoles();

    /// <summary>Opens the voluntary change-password dialog for the signed-in user.</summary>
    void OpenChangePassword();

    /// <summary>
    /// Shows the lock overlay over the whole shell (story 2.7). It is modeless and owned by the shell, so every open
    /// dialog keeps its state while locked. The callbacks tell the shell that the session resumed or ended.
    /// </summary>
    void OpenLockScreen(string title, string initials, string displayName, Action onUnlocked, Action onSignedOut);
}
