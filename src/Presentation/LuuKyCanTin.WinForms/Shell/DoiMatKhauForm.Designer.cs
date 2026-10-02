namespace LuuKyCanTin.WinForms.Shell;

partial class DoiMatKhauForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblTieuDe = null!;
    private Label lblMatKhauHienTai = null!;
    private TextBox txtMatKhauHienTai = null!;
    private Label lblMatKhauMoi = null!;
    private TextBox txtMatKhauMoi = null!;
    private Label lblXacNhan = null!;
    private TextBox txtXacNhan = null!;
    private Label lblHuongDan = null!;
    private Label lblLoi = null!;
    private Button btnLuu = null!;
    private Button btnHuy = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblTieuDe = new Label();
        lblMatKhauHienTai = new Label();
        txtMatKhauHienTai = new TextBox();
        lblMatKhauMoi = new Label();
        txtMatKhauMoi = new TextBox();
        lblXacNhan = new Label();
        txtXacNhan = new TextBox();
        lblHuongDan = new Label();
        lblLoi = new Label();
        btnLuu = new Button();
        btnHuy = new Button();
        SuspendLayout();

        lblTieuDe.AutoSize = true;
        lblTieuDe.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        lblTieuDe.Location = new Point(20, 16);
        lblTieuDe.Text = "Đổi mật khẩu";

        lblMatKhauHienTai.AutoSize = true;
        lblMatKhauHienTai.Location = new Point(20, 58);
        lblMatKhauHienTai.Text = "Mật khẩu hiện tại";

        txtMatKhauHienTai.Location = new Point(20, 78);
        txtMatKhauHienTai.Size = new Size(320, 27);
        txtMatKhauHienTai.UseSystemPasswordChar = true;
        txtMatKhauHienTai.Name = "txtMatKhauHienTai";

        lblMatKhauMoi.AutoSize = true;
        lblMatKhauMoi.Location = new Point(20, 116);
        lblMatKhauMoi.Text = "Mật khẩu mới";

        txtMatKhauMoi.Location = new Point(20, 136);
        txtMatKhauMoi.Size = new Size(320, 27);
        txtMatKhauMoi.UseSystemPasswordChar = true;
        txtMatKhauMoi.Name = "txtMatKhauMoi";

        lblXacNhan.AutoSize = true;
        lblXacNhan.Location = new Point(20, 174);
        lblXacNhan.Text = "Xác nhận mật khẩu mới";

        txtXacNhan.Location = new Point(20, 194);
        txtXacNhan.Size = new Size(320, 27);
        txtXacNhan.UseSystemPasswordChar = true;
        txtXacNhan.Name = "txtXacNhan";

        lblHuongDan.ForeColor = Color.DimGray;
        lblHuongDan.Location = new Point(20, 228);
        lblHuongDan.Size = new Size(320, 44);
        lblHuongDan.Text = "Ít nhất 8 ký tự, gồm chữ in hoa, chữ thường và chữ số.";

        lblLoi.ForeColor = Color.Firebrick;
        lblLoi.Location = new Point(20, 272);
        lblLoi.Size = new Size(320, 44);
        lblLoi.Text = "";

        btnLuu.Location = new Point(20, 322);
        btnLuu.Size = new Size(150, 32);
        btnLuu.Text = "Lưu";
        btnLuu.Name = "btnLuu";

        btnHuy.Location = new Point(190, 322);
        btnHuy.Size = new Size(150, 32);
        btnHuy.Text = "Hủy";
        btnHuy.Name = "btnHuy";

        AcceptButton = btnLuu;
        CancelButton = btnHuy;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(370, 374);
        Controls.Add(lblTieuDe);
        Controls.Add(lblMatKhauHienTai);
        Controls.Add(txtMatKhauHienTai);
        Controls.Add(lblMatKhauMoi);
        Controls.Add(txtMatKhauMoi);
        Controls.Add(lblXacNhan);
        Controls.Add(txtXacNhan);
        Controls.Add(lblHuongDan);
        Controls.Add(lblLoi);
        Controls.Add(btnLuu);
        Controls.Add(btnHuy);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Đổi mật khẩu";
        ResumeLayout(false);
        PerformLayout();
    }
}
