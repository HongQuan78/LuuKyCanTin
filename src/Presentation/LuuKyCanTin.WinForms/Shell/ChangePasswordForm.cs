using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Shell;

public partial class ChangePasswordForm : Form, IChangePasswordView
{
    public ChangePasswordForm()
    {
        InitializeComponent();
        btnSave.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        btnCancel.Click += (_, _) => CancelClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? SaveClicked;

    public event EventHandler? CancelClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        set => Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsForced
    {
        set => lblHint.Text = value
            ? "Bạn phải đổi mật khẩu trước khi vào hệ thống. Ít nhất 8 ký tự, gồm chữ in hoa, chữ thường và chữ số."
            : "Ít nhất 8 ký tự, gồm chữ in hoa, chữ thường và chữ số.";
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CurrentPassword => txtCurrentPassword.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string NewPassword => txtNewPassword.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Confirmation => txtConfirmation.Text;

    public void ShowError(string message)
    {
        lblError.Text = message;
        txtCurrentPassword.SelectAll();
        txtCurrentPassword.Focus();
    }

    public void CloseWithResult(bool succeeded)
    {
        DialogResult = succeeded ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }

    public bool DisplayText() => ShowDialog() == DialogResult.OK;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        txtCurrentPassword.Focus();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // The X behaves like Hủy, so a forced change signs out before the dialog really closes. When the
        // presenter closes the dialog it has already set DialogResult, so this branch is skipped.
        if (DialogResult == DialogResult.None && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
