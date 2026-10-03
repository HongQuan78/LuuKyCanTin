using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null!;
    private Panel pnlSide = null!;
    private Label lblLogo = null!;
    private Label lblProductName = null!;
    private Label lblProductDescription = null!;
    private Label lblWorkstation = null!;
    private FlowLayoutPanel pnlForm = null!;
    private Label lblTitle = null!;
    private Label lblHint = null!;
    private Banner bnrError = null!;
    private Label lblUserName = null!;
    private InputFrame frmUserName = null!;
    private TextBox txtUserName = null!;
    private Label lblPassword = null!;
    private InputFrame frmPassword = null!;
    private TextBox txtPassword = null!;
    private Button btnSignIn = null!;
    private Label lblForgotPassword = null!;
    private Button btnClose = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        const int formWidth = 372;

        components = new System.ComponentModel.Container();
        pnlSide = new Panel();
        lblLogo = new Label();
        lblProductName = new Label();
        lblProductDescription = new Label();
        lblWorkstation = new Label();
        pnlForm = new FlowLayoutPanel();
        lblTitle = new Label();
        lblHint = new Label();
        bnrError = new Banner();
        lblUserName = new Label();
        frmUserName = new InputFrame();
        txtUserName = new TextBox();
        lblPassword = new Label();
        frmPassword = new InputFrame();
        txtPassword = new TextBox();
        btnSignIn = new Button();
        lblForgotPassword = new Label();
        btnClose = new Button();
        pnlSide.SuspendLayout();
        pnlForm.SuspendLayout();
        SuspendLayout();

        // Left: the dark side panel with the product name.
        pnlSide.BackColor = AppTheme.Side;
        pnlSide.Dock = DockStyle.Left;
        pnlSide.Width = 300;
        pnlSide.Controls.Add(lblLogo);
        pnlSide.Controls.Add(lblProductName);
        pnlSide.Controls.Add(lblProductDescription);
        pnlSide.Controls.Add(lblWorkstation);

        lblLogo.BackColor = AppTheme.Accent;
        lblLogo.ForeColor = AppTheme.OnAccent;
        lblLogo.Font = AppTheme.CardTitleFont;
        lblLogo.Location = new Point(28, 32);
        lblLogo.Size = new Size(48, 48);
        lblLogo.Text = "LK";
        lblLogo.TextAlign = ContentAlignment.MiddleCenter;
        lblLogo.UseMnemonic = false;

        lblProductName.AutoSize = true;
        lblProductName.ForeColor = AppTheme.OnAccent;
        lblProductName.Font = AppTheme.GreetingFont;
        lblProductName.Location = new Point(26, 100);
        lblProductName.Text = "Lưu ký – Căn tin";
        lblProductName.UseMnemonic = false;

        lblProductDescription.ForeColor = AppTheme.SideText;
        lblProductDescription.Location = new Point(28, 136);
        lblProductDescription.Size = new Size(244, 44);
        lblProductDescription.Text = "Quản lý tiền gửi lưu ký và bán hàng căn tin";
        lblProductDescription.UseMnemonic = false;

        lblWorkstation.ForeColor = AppTheme.SideMuted;
        lblWorkstation.Font = AppTheme.SmallFont;
        lblWorkstation.Location = new Point(28, 406);
        lblWorkstation.Size = new Size(244, 20);
        lblWorkstation.UseMnemonic = false;

        // Right: the sign-in fields, top to bottom, so the banner pushes the fields down when it shows.
        pnlForm.BackColor = AppTheme.Card;
        pnlForm.Dock = DockStyle.Fill;
        pnlForm.FlowDirection = FlowDirection.TopDown;
        pnlForm.WrapContents = false;
        pnlForm.Padding = new Padding(44, 40, 44, 0);
        pnlForm.Controls.Add(lblTitle);
        pnlForm.Controls.Add(lblHint);
        pnlForm.Controls.Add(bnrError);
        pnlForm.Controls.Add(lblUserName);
        pnlForm.Controls.Add(frmUserName);
        pnlForm.Controls.Add(lblPassword);
        pnlForm.Controls.Add(frmPassword);
        pnlForm.Controls.Add(btnSignIn);
        pnlForm.Controls.Add(lblForgotPassword);

        lblTitle.AutoSize = true;
        lblTitle.Font = AppTheme.GreetingFont;
        lblTitle.ForeColor = AppTheme.Text;
        lblTitle.Margin = Padding.Empty;
        lblTitle.Text = "Đăng nhập";
        lblTitle.UseMnemonic = false;

        lblHint.AutoSize = true;
        lblHint.ForeColor = AppTheme.Muted;
        lblHint.Margin = new Padding(0, 4, 0, AppTheme.Gap);
        lblHint.Text = "Dùng tài khoản do quản trị viên cấp.";
        lblHint.UseMnemonic = false;

        bnrError.Margin = new Padding(0, 0, 0, AppTheme.Gap);
        bnrError.Name = "bnrError";
        bnrError.Width = formWidth;

        lblUserName.AutoSize = true;
        lblUserName.Font = AppTheme.LabelFont;
        lblUserName.ForeColor = AppTheme.Label;
        lblUserName.Margin = new Padding(0, 0, 0, AppTheme.LabelGap);
        lblUserName.Name = "lblUserName";
        lblUserName.TabIndex = 0;
        lblUserName.Text = "&Tên đăng nhập";

        frmUserName.Margin = new Padding(0, 0, 0, AppTheme.Gap);
        frmUserName.Width = formWidth;
        frmUserName.TabIndex = 1;
        txtUserName.Name = "txtUserName";
        txtUserName.TabIndex = 0;
        frmUserName.Inner = txtUserName;
        frmUserName.Glyph = Glyphs.Contact;

        lblPassword.AutoSize = true;
        lblPassword.Font = AppTheme.LabelFont;
        lblPassword.ForeColor = AppTheme.Label;
        lblPassword.Margin = new Padding(0, 0, 0, AppTheme.LabelGap);
        lblPassword.Name = "lblPassword";
        lblPassword.TabIndex = 2;
        lblPassword.Text = "&Mật khẩu";

        frmPassword.Margin = new Padding(0, 0, 0, 2 * AppTheme.Gap);
        frmPassword.Width = formWidth;
        frmPassword.TabIndex = 3;
        txtPassword.Name = "txtPassword";
        txtPassword.PlaceholderText = "Nhập mật khẩu";
        txtPassword.TabIndex = 0;
        txtPassword.UseSystemPasswordChar = true;
        frmPassword.Inner = txtPassword;
        frmPassword.Glyph = Glyphs.Lock;

        btnSignIn.Margin = new Padding(0, 0, 0, AppTheme.Gap);
        btnSignIn.Name = "btnSignIn";
        btnSignIn.Size = new Size(formWidth, AppTheme.ButtonHeightLarge);
        btnSignIn.TabIndex = 4;
        btnSignIn.Text = SignInText;
        btnSignIn.Click += OnSignInClicked;
        AppTheme.StylePrimary(btnSignIn);

        lblForgotPassword.ForeColor = AppTheme.Muted;
        lblForgotPassword.Margin = Padding.Empty;
        lblForgotPassword.Size = new Size(formWidth, 20);
        lblForgotPassword.Text = "Quên mật khẩu? Liên hệ quản trị viên.";
        lblForgotPassword.TextAlign = ContentAlignment.MiddleCenter;
        lblForgotPassword.UseMnemonic = false;

        // The window has no title bar: this ✕ (and Esc) closes it and exits the app.
        btnClose.DialogResult = DialogResult.Cancel;
        btnClose.FlatStyle = FlatStyle.Flat;
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = AppTheme.DangerSoft;
        btnClose.BackColor = AppTheme.Card;
        btnClose.Font = AppTheme.IconFont(10F);
        btnClose.ForeColor = AppTheme.Muted;
        btnClose.Location = new Point(714, 0);
        btnClose.Name = "btnClose";
        btnClose.Size = new Size(46, 32);
        btnClose.TabStop = false;
        btnClose.Text = Glyphs.Cancel;
        btnClose.UseMnemonic = false;

        AcceptButton = btnSignIn;
        CancelButton = btnClose;
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Card;
        ClientSize = new Size(760, 460);
        Controls.Add(btnClose);
        Controls.Add(pnlForm);
        Controls.Add(pnlSide);
        Font = AppTheme.BodyFont;
        FormBorderStyle = FormBorderStyle.None;
        Name = "LoginForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Đăng nhập";
        pnlSide.ResumeLayout(false);
        pnlSide.PerformLayout();
        pnlForm.ResumeLayout(false);
        pnlForm.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
