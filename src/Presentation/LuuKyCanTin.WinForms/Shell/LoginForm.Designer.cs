namespace LuuKyCanTin.WinForms.Shell;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null!;
    private TextBox txtUserName = null!;
    private TextBox txtPassword = null!;
    private Label lblUserName = null!;
    private Label lblPassword = null!;
    private Label lblError = null!;
    private Button btnSignIn = null!;
    private Button btnCancel = null!;
    private Label lblTitle = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        txtUserName = new TextBox();
        txtPassword = new TextBox();
        lblUserName = new Label();
        lblPassword = new Label();
        lblError = new Label();
        btnSignIn = new Button();
        btnCancel = new Button();
        lblTitle = new Label();
        SuspendLayout();

        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTitle.Location = new Point(24, 20);
        lblTitle.Text = "Lưu ký – Căn tin";

        lblUserName.AutoSize = true;
        lblUserName.Location = new Point(24, 70);
        lblUserName.Text = "Tên đăng nhập";

        txtUserName.Location = new Point(24, 90);
        txtUserName.Size = new Size(280, 27);
        txtUserName.Name = "txtUserName";

        lblPassword.AutoSize = true;
        lblPassword.Location = new Point(24, 130);
        lblPassword.Text = "Mật khẩu";

        txtPassword.Location = new Point(24, 150);
        txtPassword.Size = new Size(280, 27);
        txtPassword.UseSystemPasswordChar = true;
        txtPassword.Name = "txtPassword";

        lblError.ForeColor = Color.Firebrick;
        lblError.Location = new Point(24, 184);
        lblError.Size = new Size(280, 40);
        lblError.Text = "";

        btnSignIn.Location = new Point(24, 228);
        btnSignIn.Size = new Size(135, 32);
        btnSignIn.Text = "Đăng nhập";
        btnSignIn.Click += OnSignInClicked;

        btnCancel.DialogResult = DialogResult.Cancel;
        btnCancel.Location = new Point(169, 228);
        btnCancel.Size = new Size(135, 32);
        btnCancel.Text = "Hủy";

        AcceptButton = btnSignIn;
        CancelButton = btnCancel;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(330, 280);
        Controls.Add(lblTitle);
        Controls.Add(lblUserName);
        Controls.Add(txtUserName);
        Controls.Add(lblPassword);
        Controls.Add(txtPassword);
        Controls.Add(lblError);
        Controls.Add(btnSignIn);
        Controls.Add(btnCancel);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Đăng nhập";
        ResumeLayout(false);
        PerformLayout();
    }
}
