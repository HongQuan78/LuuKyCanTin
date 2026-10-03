namespace LuuKyCanTin.WinForms.Shell;

public interface IMainView
{
    event EventHandler Loaded;

    /// <summary>A sidebar item, a Trang chủ tile or a global shortcut asked to open an entry of the navigation model.</summary>
    event EventHandler<NavItem> NavigationRequested;

    string Title { set; }

    /// <summary>The unit name under the product name in the sidebar; empty shows nothing.</summary>
    string FacilityName { set; }

    /// <summary>Builds the sidebar and the Trang chủ tiles from the model.</summary>
    void ShowNavigation(NavigationModel navigation);

    void ShowWorkstation(WorkstationInfo workstation);

    /// <summary>The user block at the bottom of the sidebar.</summary>
    void ShowUser(string initials, string displayName, string roleText);

    /// <summary>Shows Trang chủ in the content area with the greeting and date line.</summary>
    void ShowHome(string greeting, string dateLine);

    /// <summary>Closes the shell so the application context can show the login form again.</summary>
    void CloseShell();

    void ShowError(string message);
}
