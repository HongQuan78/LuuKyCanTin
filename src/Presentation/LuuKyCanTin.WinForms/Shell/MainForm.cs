using System.ComponentModel;
using LuuKyCanTin.WinForms.Custody;
using LuuKyCanTin.WinForms.MasterData;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

public partial class MainForm : Form, IMainView
{
    // The designer needs a parameterless constructor; the app resolves the scope factory one.
    private readonly IServiceScopeFactory? _scopeFactory;

    public MainForm()
        : this(null)
    {
    }

    public MainForm(IServiceScopeFactory? scopeFactory)
    {
        _scopeFactory = scopeFactory;
        InitializeComponent();
        mnuOfficers.Click += (_, _) => OfficersClicked?.Invoke(this, EventArgs.Empty);
        mnuRoles.Click += (_, _) => RolesClicked?.Invoke(this, EventArgs.Empty);
        mnuChangePassword.Click += (_, _) => ChangePasswordClicked?.Invoke(this, EventArgs.Empty);
        mnuSignOut.Click += (_, _) => SignOutClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Loaded;

    public event EventHandler? OfficersClicked;

    public event EventHandler? RolesClicked;

    public event EventHandler? ChangePasswordClicked;

    public event EventHandler? SignOutClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        set => Text = value;
    }

    public void CloseShell() => Close();

    public void ShowError(string message) =>
        MessageBox.Show(this, message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    private void OnAddInmate(object? sender, EventArgs e)
    {
        using var scope = _scopeFactory!.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<AddInmateForm>();
        ActivatorUtilities.CreateInstance<AddInmatePresenter>(scope.ServiceProvider, form);
        form.ShowDialog(this);
    }

    private void OnCreateDepositReceipt(object? sender, EventArgs e)
    {
        using var scope = _scopeFactory!.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<DepositReceiptForm>();
        ActivatorUtilities.CreateInstance<DepositReceiptPresenter>(scope.ServiceProvider, form);
        form.ShowDialog(this);
    }
}
