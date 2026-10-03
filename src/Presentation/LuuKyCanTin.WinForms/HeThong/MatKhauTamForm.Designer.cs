namespace LuuKyCanTin.WinForms.HeThong;

partial class MatKhauTamForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblHuongDan = null!;
    private TextBox txtMatKhau = null!;
    private Label lblGhiChu = null!;
    private Button btnSaoChep = null!;
    private Button btnDong = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblHuongDan = new Label();
        txtMatKhau = new TextBox();
        lblGhiChu = new Label();
        btnSaoChep = new Button();
        btnDong = new Button();
        SuspendLayout();

        lblHuongDan.AutoSize = true;
        lblHuongDan.Location = new Point(14, 14);
        lblHuongDan.Text = "Mật khẩu tạm thời:";

        txtMatKhau.Font = new Font("Consolas", 14F, FontStyle.Bold);
        txtMatKhau.Location = new Point(14, 38);
        txtMatKhau.Name = "txtMatKhau";
        txtMatKhau.ReadOnly = true;
        txtMatKhau.Size = new Size(380, 30);
        txtMatKhau.TabIndex = 0;
        txtMatKhau.TextAlign = HorizontalAlignment.Center;

        lblGhiChu.ForeColor = Color.DimGray;
        lblGhiChu.Location = new Point(14, 78);
        lblGhiChu.Size = new Size(380, 40);
        lblGhiChu.Text = "Người dùng phải đổi mật khẩu ở lần đăng nhập đầu tiên.";

        btnSaoChep.Location = new Point(14, 126);
        btnSaoChep.Name = "btnSaoChep";
        btnSaoChep.Size = new Size(140, 30);
        btnSaoChep.TabIndex = 1;
        btnSaoChep.Text = "&Sao chép";

        btnDong.DialogResult = DialogResult.OK;
        btnDong.Location = new Point(294, 126);
        btnDong.Name = "btnDong";
        btnDong.Size = new Size(100, 30);
        btnDong.TabIndex = 2;
        btnDong.Text = "&Đóng";

        AcceptButton = btnDong;
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = btnDong;
        ClientSize = new Size(410, 172);
        Controls.Add(btnDong);
        Controls.Add(btnSaoChep);
        Controls.Add(lblGhiChu);
        Controls.Add(txtMatKhau);
        Controls.Add(lblHuongDan);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "MatKhauTamForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Mật khẩu tạm thời";
        ResumeLayout(false);
        PerformLayout();
    }
}
