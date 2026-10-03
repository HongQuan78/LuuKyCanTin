namespace LuuKyCanTin.WinForms.Shell;

public partial class LoginForm : Form, ILoginView
{
    public LoginForm()
    {
        InitializeComponent();
    }

    public event EventHandler? SignInClicked;

    public string UserName => txtUserName.Text;

    public string Password => txtPassword.Text;

    public void ShowError(string message)
    {
        lblError.Text = message;
        txtPassword.SelectAll();
        txtPassword.Focus();
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
