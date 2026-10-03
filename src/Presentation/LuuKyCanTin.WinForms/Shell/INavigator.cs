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

    /// <summary>Shows the role and permission screen in the content area.</summary>
    void OpenRoles();

    /// <summary>Opens the voluntary change-password dialog for the signed-in user.</summary>
    void OpenChangePassword();
}
