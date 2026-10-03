namespace LuuKyCanTin.WinForms.HeThong;

partial class TaoTaiKhoanForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblTenDangNhap = null!;
    private TextBox txtTenDangNhap = null!;
    private Label lblCanBo = null!;
    private ComboBox cboCanBo = null!;
    private Label lblVaiTro = null!;
    private CheckedListBox lstVaiTro = null!;
    private Label lblLoi = null!;
    private Button btnTao = null!;
    private Button btnHuy = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblTenDangNhap = new Label();
        txtTenDangNhap = new TextBox();
        lblCanBo = new Label();
        cboCanBo = new ComboBox();
        lblVaiTro = new Label();
        lstVaiTro = new CheckedListBox();
        lblLoi = new Label();
        btnTao = new Button();
        btnHuy = new Button();
        SuspendLayout();

        lblTenDangNhap.AutoSize = true;
        lblTenDangNhap.Location = new Point(14, 18);
        lblTenDangNhap.Text = "Tên đăng nhập:";

        txtTenDangNhap.Location = new Point(130, 15);
        txtTenDangNhap.MaxLength = 50;
        txtTenDangNhap.Name = "txtTenDangNhap";
        txtTenDangNhap.Size = new Size(240, 23);
        txtTenDangNhap.TabIndex = 0;

        lblCanBo.AutoSize = true;
        lblCanBo.Location = new Point(14, 50);
        lblCanBo.Text = "Cán bộ:";

        cboCanBo.DropDownStyle = ComboBoxStyle.DropDownList;
        cboCanBo.Location = new Point(130, 47);
        cboCanBo.Name = "cboCanBo";
        cboCanBo.Size = new Size(340, 23);
        cboCanBo.TabIndex = 1;

        lblVaiTro.AutoSize = true;
        lblVaiTro.Location = new Point(14, 82);
        lblVaiTro.Text = "Vai trò:";

        lstVaiTro.CheckOnClick = true;
        lstVaiTro.FormattingEnabled = true;
        lstVaiTro.Location = new Point(130, 82);
        lstVaiTro.Name = "lstVaiTro";
        lstVaiTro.Size = new Size(340, 112);
        lstVaiTro.TabIndex = 2;

        lblLoi.ForeColor = Color.Firebrick;
        lblLoi.Location = new Point(14, 204);
        lblLoi.Size = new Size(456, 40);
        lblLoi.Text = "";

        btnTao.Location = new Point(314, 250);
        btnTao.Name = "btnTao";
        btnTao.Size = new Size(75, 25);
        btnTao.TabIndex = 3;
        btnTao.Text = "&Tạo";

        btnHuy.DialogResult = DialogResult.Cancel;
        btnHuy.Location = new Point(395, 250);
        btnHuy.Name = "btnHuy";
        btnHuy.Size = new Size(75, 25);
        btnHuy.TabIndex = 4;
        btnHuy.Text = "&Hủy";

        AcceptButton = btnTao;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnHuy;
        ClientSize = new Size(484, 288);
        Controls.Add(btnHuy);
        Controls.Add(btnTao);
        Controls.Add(lblLoi);
        Controls.Add(lstVaiTro);
        Controls.Add(lblVaiTro);
        Controls.Add(cboCanBo);
        Controls.Add(lblCanBo);
        Controls.Add(txtTenDangNhap);
        Controls.Add(lblTenDangNhap);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "TaoTaiKhoanForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Thêm tài khoản";
        ResumeLayout(false);
        PerformLayout();
    }
}
