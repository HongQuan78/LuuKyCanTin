namespace LuuKyCanTin.WinForms.Shell;

partial class ChangePasswordForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblTitle = null!;
    private Label lblCurrentPassword = null!;
    private TextBox txtCurrentPassword = null!;
    private Label lblNewPassword = null!;
    private TextBox txtNewPassword = null!;
    private Label lblConfirmation = null!;
    private TextBox txtConfirmation = null!;
    private Label lblHint = null!;
    private Label lblError = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblTitle = new Label();
        lblCurrentPassword = new Label();
        txtCurrentPassword = new TextBox();
        lblNewPassword = new Label();
        txtNewPassword = new TextBox();
        lblConfirmation = new Label();
        txtConfirmation = new TextBox();
        lblHint = new Label();
        lblError = new Label();
        btnSave = new Button();
        btnCancel = new Button();
        SuspendLayout();

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTitle.Location = new Point(20, 16);
        lblTitle.Text = "Đổi mật khẩu";

        lblCurrentPassword.AutoSize = true;
        lblCurrentPassword.Location = new Point(20, 58);
        lblCurrentPassword.Text = "Mật khẩu hiện tại";

        txtCurrentPassword.Location = new Point(20, 78);
        txtCurrentPassword.Size = new Size(320, 27);
        txtCurrentPassword.UseSystemPasswordChar = true;
        txtCurrentPassword.Name = "txtCurrentPassword";

        lblNewPassword.AutoSize = true;
        lblNewPassword.Location = new Point(20, 116);
        lblNewPassword.Text = "Mật khẩu mới";

        txtNewPassword.Location = new Point(20, 136);
        txtNewPassword.Size = new Size(320, 27);
        txtNewPassword.UseSystemPasswordChar = true;
        txtNewPassword.Name = "txtNewPassword";

        lblConfirmation.AutoSize = true;
        lblConfirmation.Location = new Point(20, 174);
        lblConfirmation.Text = "Xác nhận mật khẩu mới";

        txtConfirmation.Location = new Point(20, 194);
        txtConfirmation.Size = new Size(320, 27);
        txtConfirmation.UseSystemPasswordChar = true;
        txtConfirmation.Name = "txtConfirmation";

        lblHint.ForeColor = Color.DimGray;
        lblHint.Location = new Point(20, 228);
        lblHint.Size = new Size(320, 44);
        lblHint.Text = "Ít nhất 8 ký tự, gồm chữ in hoa, chữ thường và chữ số.";

        lblError.ForeColor = Color.Firebrick;
        lblError.Location = new Point(20, 272);
        lblError.Size = new Size(320, 44);
        lblError.Text = "";

        btnSave.Location = new Point(20, 322);
        btnSave.Size = new Size(150, 32);
        btnSave.Text = "Lưu";
        btnSave.Name = "btnSave";

        btnCancel.Location = new Point(190, 322);
        btnCancel.Size = new Size(150, 32);
        btnCancel.Text = "Hủy";
        btnCancel.Name = "btnCancel";

        AcceptButton = btnSave;
        CancelButton = btnCancel;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(370, 374);
        Controls.Add(lblTitle);
        Controls.Add(lblCurrentPassword);
        Controls.Add(txtCurrentPassword);
        Controls.Add(lblNewPassword);
        Controls.Add(txtNewPassword);
        Controls.Add(lblConfirmation);
        Controls.Add(txtConfirmation);
        Controls.Add(lblHint);
        Controls.Add(lblError);
        Controls.Add(btnSave);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Đổi mật khẩu";
        ResumeLayout(false);
        PerformLayout();
    }
}
