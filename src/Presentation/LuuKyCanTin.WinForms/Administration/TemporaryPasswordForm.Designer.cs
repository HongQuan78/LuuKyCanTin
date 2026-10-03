using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

partial class TemporaryPasswordForm
{
    private const int DialogPadding = AppTheme.DialogPadding;
    private const int ButtonWidth = 110;

    private System.ComponentModel.IContainer components = null!;
    private Label lblHeading = null!;
    private TextBox txtPassword = null!;
    private Label lblNote = null!;
    private Button btnCopy = null!;
    private Button btnClose = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblHeading = new Label();
        txtPassword = new TextBox();
        lblNote = new Label();
        btnCopy = new Button();
        btnClose = new Button();
        SuspendLayout();

        lblHeading.AutoSize = true;
        lblHeading.Font = AppTheme.DialogTitleFont;
        lblHeading.ForeColor = AppTheme.Text;
        lblHeading.Location = new Point(DialogPadding, 18);
        lblHeading.Name = "lblHeading";
        lblHeading.Text = "Mật khẩu tạm thời";
        lblHeading.UseMnemonic = false;

        txtPassword.Font = AppTheme.AmountMediumFont;
        txtPassword.Location = new Point(DialogPadding, 52);
        txtPassword.Name = "txtPassword";
        txtPassword.ReadOnly = true;
        txtPassword.Size = new Size(420, 30);
        txtPassword.TabIndex = 0;
        txtPassword.TextAlign = HorizontalAlignment.Center;

        lblNote.ForeColor = AppTheme.Muted;
        lblNote.Location = new Point(DialogPadding, 94);
        lblNote.Name = "lblNote";
        lblNote.Size = new Size(420, 38);
        lblNote.Text = "Người dùng phải đổi mật khẩu ở lần đăng nhập đầu tiên.";
        lblNote.UseMnemonic = false;

        btnCopy.AutoSize = true;
        btnCopy.Location = new Point(DialogPadding, 144);
        btnCopy.MinimumSize = new Size(ButtonWidth, AppTheme.ButtonHeight);
        btnCopy.Name = "btnCopy";
        btnCopy.TabIndex = 1;
        btnCopy.Text = "&Sao chép";
        AppTheme.StyleSecondary(btnCopy);

        btnClose.AutoSize = true;
        btnClose.DialogResult = DialogResult.OK;
        btnClose.Location = new Point(340, 144);
        btnClose.MinimumSize = new Size(100, AppTheme.ButtonHeight);
        btnClose.Name = "btnClose";
        btnClose.TabIndex = 2;
        btnClose.Text = "&Đóng";
        AppTheme.StylePrimary(btnClose);

        AcceptButton = btnClose;
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Card;
        CancelButton = btnClose;
        ClientSize = new Size(460, 192);
        Controls.Add(btnClose);
        Controls.Add(btnCopy);
        Controls.Add(lblNote);
        Controls.Add(txtPassword);
        Controls.Add(lblHeading);
        Font = AppTheme.BodyFont;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "TemporaryPasswordForm";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Mật khẩu tạm thời";
        ResumeLayout(false);
        PerformLayout();
    }
}
