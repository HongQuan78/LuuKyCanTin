namespace LuuKyCanTin.WinForms.Administration;

/// <summary>
/// Shows a temporary password once, with a copy button. The value lives only in the text box; closing the
/// dialog discards it and the database holds only its hash.
/// </summary>
public partial class TemporaryPasswordForm : Form
{
    public TemporaryPasswordForm(string temporaryPassword)
    {
        InitializeComponent();
        txtPassword.Text = temporaryPassword;
        btnCopy.Click += OnCopyClicked;
    }

    public string TemporaryPassword => txtPassword.Text;

    private void OnCopyClicked(object? sender, EventArgs e)
    {
        if (TemporaryPassword.Length > 0)
            Clipboard.SetText(TemporaryPassword);
    }
}
