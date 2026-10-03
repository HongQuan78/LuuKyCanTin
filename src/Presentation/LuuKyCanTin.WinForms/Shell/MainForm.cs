using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Shell;

public partial class MainForm : Form, IMainView, IContentHost
{
    private const string HomeTitle = "Trang chủ";

    // Screens shown in the content area, created on first use and kept for the session.
    private readonly Dictionary<string, Control> _pages = [];
    private NavigationModel? _navigation;
    private HomePage? _home;

    public MainForm()
    {
        InitializeComponent();
        sidebar.ItemClicked += (_, item) => NavigationRequested?.Invoke(this, item);
    }

    public event EventHandler? Loaded;

    public event EventHandler<NavItem>? NavigationRequested;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        set => Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string FacilityName
    {
        set => sidebar.FacilityName = value;
    }

    public void ShowNavigation(NavigationModel navigation)
    {
        _navigation = navigation;
        sidebar.ShowNavigation(navigation);
        _home?.ShowTiles(navigation.Tiles);
    }

    public void ShowWorkstation(WorkstationInfo workstation) => statusBar.ShowWorkstation(workstation);

    public void ShowUser(string initials, string displayName, string roleText) =>
        sidebar.ShowUser(initials, displayName, roleText);

    public void ShowHome(string greeting, string dateLine)
    {
        ShowPage(ShellNavigation.HomeKey, HomeTitle, CreateHomePage);
        _home!.ShowGreeting(greeting, dateLine);
    }

    public void ShowPage(string key, string title, Func<Control> create)
    {
        if (!_pages.TryGetValue(key, out var page))
        {
            page = create();
            page.Dock = DockStyle.Fill;
            _pages[key] = page;
            pnlContent.Controls.Add(page);
        }

        foreach (var other in _pages.Values.Where(p => p != page))
            other.Visible = false;
        page.Visible = true;
        headerBar.ShowTitle(title);
        sidebar.SetActive(key);
    }

    public void CloseShell() => Close();

    public void ShowError(string message) =>
        MessageBox.Show(this, message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    // Global shortcuts (F2 …) work from anywhere in the shell, whatever has the focus.
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_navigation?.FindByShortcut(keyData) is { } item)
        {
            NavigationRequested?.Invoke(this, item);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private HomePage CreateHomePage()
    {
        _home = new HomePage();
        _home.TileClicked += (_, item) => NavigationRequested?.Invoke(this, item);
        if (_navigation is not null)
            _home.ShowTiles(_navigation.Tiles);
        return _home;
    }
}
