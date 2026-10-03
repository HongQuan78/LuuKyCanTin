using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Shell;

public partial class LoginForm : Form, ILoginView
{
    private const string SignInText = "Đăng &nhập";
    private const string SigningInText = "Đang đăng nhập…";

    // The designer needs a parameterless constructor; the app resolves the one with the workstation.
    public LoginForm()
        : this(null)
    {
    }

    public LoginForm(WorkstationInfo? workstation)
    {
        InitializeComponent();
        // The login is shown modeless, where a button's DialogResult alone never closes the form.
        btnClose.Click += (_, _) => Close();
        if (workstation is not null)
            lblWorkstation.Text = $"Máy: {workstation.WorkstationName} · {workstation.Version}";
    }

    public event EventHandler? SignInClicked;

    public string UserName => txtUserName.Text;

    public string Password => txtPassword.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsBusy
    {
        set
        {
            btnSignIn.Enabled = !value;
            btnSignIn.Text = value ? SigningInText : SignInText;
            UseWaitCursor = value;
        }
    }

    public void ShowError(string message)
    {
        bnrError.Message = message;
        ClearPassword();
    }

    public void ClearPassword()
    {
        txtPassword.Clear();
        txtPassword.Focus();
    }

    public void CloseWithResult(bool succeeded)
    {
        DialogResult = succeeded ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }

    private void OnSignInClicked(object? sender, EventArgs e) => SignInClicked?.Invoke(this, EventArgs.Empty);

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        txtUserName.Focus();
    }
}
