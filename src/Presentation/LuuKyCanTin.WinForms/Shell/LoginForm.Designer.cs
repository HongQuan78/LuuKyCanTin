namespace LuuKyCanTin.WinForms.Shell;

partial class LoginForm
{
    private System.ComponentModel.IContainer components = null!;
    private TextBox txtTenDangNhap = null!;
    private TextBox txtMatKhau = null!;
    private Label lblTenDangNhap = null!;
    private Label lblMatKhau = null!;
    private Label lblLoi = null!;
    private Button btnDangNhap = null!;
    private Button btnHuy = null!;
    private Label lblTieuDe = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        txtTenDangNhap = new TextBox();
        txtMatKhau = new TextBox();
        lblTenDangNhap = new Label();
        lblMatKhau = new Label();
        lblLoi = new Label();
        btnDangNhap = new Button();
        btnHuy = new Button();
        lblTieuDe = new Label();
        SuspendLayout();

        lblTieuDe.AutoSize = true;
        lblTieuDe.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
        lblTieuDe.Location = new Point(24, 20);
        lblTieuDe.Text = "Lưu ký – Căn tin";

        lblTenDangNhap.AutoSize = true;
        lblTenDangNhap.Location = new Point(24, 70);
        lblTenDangNhap.Text = "Tên đăng nhập";

        txtTenDangNhap.Location = new Point(24, 90);
        txtTenDangNhap.Size = new Size(280, 27);
        txtTenDangNhap.Name = "txtTenDangNhap";

        lblMatKhau.AutoSize = true;
        lblMatKhau.Location = new Point(24, 130);
        lblMatKhau.Text = "Mật khẩu";

        txtMatKhau.Location = new Point(24, 150);
        txtMatKhau.Size = new Size(280, 27);
        txtMatKhau.UseSystemPasswordChar = true;
        txtMatKhau.Name = "txtMatKhau";

        lblLoi.ForeColor = Color.Firebrick;
        lblLoi.Location = new Point(24, 184);
        lblLoi.Size = new Size(280, 40);
        lblLoi.Text = "";

        btnDangNhap.Location = new Point(24, 228);
        btnDangNhap.Size = new Size(135, 32);
        btnDangNhap.Text = "Đăng nhập";
        btnDangNhap.Click += OnDangNhapBam;

        btnHuy.DialogResult = DialogResult.Cancel;
        btnHuy.Location = new Point(169, 228);
        btnHuy.Size = new Size(135, 32);
        btnHuy.Text = "Hủy";

        AcceptButton = btnDangNhap;
        CancelButton = btnHuy;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(330, 280);
        Controls.Add(lblTieuDe);
        Controls.Add(lblTenDangNhap);
        Controls.Add(txtTenDangNhap);
        Controls.Add(lblMatKhau);
        Controls.Add(txtMatKhau);
        Controls.Add(lblLoi);
        Controls.Add(btnDangNhap);
        Controls.Add(btnHuy);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Đăng nhập";
        ResumeLayout(false);
        PerformLayout();
    }
}
