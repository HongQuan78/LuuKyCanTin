namespace LuuKyCanTin.WinForms.LuuKy;

partial class BienNhanThuForm
{
    private System.ComponentModel.IContainer components = null!;
    private Label lblDoiTuong = null!;
    private ComboBox cmbDoiTuong = null!;
    private Label lblNghiepVu = null!;
    private ComboBox cmbNghiepVu = null!;
    private Label lblNguoiGui = null!;
    private TextBox txtNguoiGuiHoTen = null!;
    private Label lblQuanHe = null!;
    private TextBox txtQuanHe = null!;
    private Label lblHinhThuc = null!;
    private ComboBox cmbHinhThuc = null!;
    private Label lblSoTaiKhoan = null!;
    private TextBox txtSoTaiKhoan = null!;
    private Label lblNgayChungTu = null!;
    private DateTimePicker dtpNgayChungTu = null!;
    private Label lblSoTien = null!;
    private TextBox txtSoTien = null!;
    private Label lblBangChu = null!;
    private Label lblNoiDung = null!;
    private TextBox txtNoiDung = null!;
    private Label lblTrangThai = null!;
    private Button btnGhiSo = null!;
    private Button btnIn = null!;
    private Button btnDong = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        lblDoiTuong = new Label();
        cmbDoiTuong = new ComboBox();
        lblNghiepVu = new Label();
        cmbNghiepVu = new ComboBox();
        lblNguoiGui = new Label();
        txtNguoiGuiHoTen = new TextBox();
        lblQuanHe = new Label();
        txtQuanHe = new TextBox();
        lblHinhThuc = new Label();
        cmbHinhThuc = new ComboBox();
        lblSoTaiKhoan = new Label();
        txtSoTaiKhoan = new TextBox();
        lblNgayChungTu = new Label();
        dtpNgayChungTu = new DateTimePicker();
        lblSoTien = new Label();
        txtSoTien = new TextBox();
        lblBangChu = new Label();
        lblNoiDung = new Label();
        txtNoiDung = new TextBox();
        lblTrangThai = new Label();
        btnGhiSo = new Button();
        btnIn = new Button();
        btnDong = new Button();
        SuspendLayout();

        lblDoiTuong.AutoSize = true;
        lblDoiTuong.Location = new Point(20, 20);
        lblDoiTuong.Text = "Đối tượng (*)";
        cmbDoiTuong.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbDoiTuong.Location = new Point(160, 17);
        cmbDoiTuong.Size = new Size(380, 27);
        cmbDoiTuong.Name = "cmbDoiTuong";

        lblNghiepVu.AutoSize = true;
        lblNghiepVu.Location = new Point(20, 56);
        lblNghiepVu.Text = "Nghiệp vụ (*)";
        cmbNghiepVu.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbNghiepVu.Location = new Point(160, 53);
        cmbNghiepVu.Size = new Size(380, 27);
        cmbNghiepVu.Name = "cmbNghiepVu";

        lblNguoiGui.AutoSize = true;
        lblNguoiGui.Location = new Point(20, 92);
        lblNguoiGui.Text = "Người gửi (*)";
        txtNguoiGuiHoTen.Location = new Point(160, 89);
        txtNguoiGuiHoTen.Size = new Size(380, 27);
        txtNguoiGuiHoTen.Name = "txtNguoiGuiHoTen";

        lblQuanHe.AutoSize = true;
        lblQuanHe.Location = new Point(20, 128);
        lblQuanHe.Text = "Quan hệ";
        txtQuanHe.Location = new Point(160, 125);
        txtQuanHe.Size = new Size(380, 27);
        txtQuanHe.Name = "txtQuanHe";

        lblHinhThuc.AutoSize = true;
        lblHinhThuc.Location = new Point(20, 164);
        lblHinhThuc.Text = "Hình thức (*)";
        cmbHinhThuc.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbHinhThuc.Location = new Point(160, 161);
        cmbHinhThuc.Size = new Size(170, 27);
        cmbHinhThuc.Name = "cmbHinhThuc";
        cmbHinhThuc.SelectedIndexChanged += OnHinhThucThayDoi;

        lblSoTaiKhoan.AutoSize = true;
        lblSoTaiKhoan.Location = new Point(20, 200);
        lblSoTaiKhoan.Text = "Số tài khoản";
        txtSoTaiKhoan.Location = new Point(160, 197);
        txtSoTaiKhoan.Size = new Size(380, 27);
        txtSoTaiKhoan.Name = "txtSoTaiKhoan";

        lblNgayChungTu.AutoSize = true;
        lblNgayChungTu.Location = new Point(20, 236);
        lblNgayChungTu.Text = "Ngày chứng từ (*)";
        dtpNgayChungTu.Format = DateTimePickerFormat.Short;
        dtpNgayChungTu.Location = new Point(160, 233);
        dtpNgayChungTu.Size = new Size(140, 27);
        dtpNgayChungTu.Name = "dtpNgayChungTu";

        lblSoTien.AutoSize = true;
        lblSoTien.Location = new Point(20, 272);
        lblSoTien.Text = "Số tiền (đồng) (*)";
        txtSoTien.Location = new Point(160, 269);
        txtSoTien.Size = new Size(170, 27);
        txtSoTien.Name = "txtSoTien";
        txtSoTien.TextChanged += OnSoTienThayDoi;

        lblBangChu.AutoSize = true;
        lblBangChu.Font = new Font("Segoe UI", 9F, FontStyle.Italic);
        lblBangChu.ForeColor = Color.DimGray;
        lblBangChu.Location = new Point(20, 302);
        lblBangChu.Size = new Size(520, 20);
        lblBangChu.Text = "";

        lblNoiDung.AutoSize = true;
        lblNoiDung.Location = new Point(20, 330);
        lblNoiDung.Text = "Nội dung";
        txtNoiDung.Location = new Point(160, 327);
        txtNoiDung.Multiline = true;
        txtNoiDung.Size = new Size(380, 60);
        txtNoiDung.Name = "txtNoiDung";

        lblTrangThai.Location = new Point(20, 400);
        lblTrangThai.Size = new Size(520, 24);
        lblTrangThai.ForeColor = Color.ForestGreen;
        lblTrangThai.Text = "";

        btnGhiSo.Location = new Point(160, 432);
        btnGhiSo.Size = new Size(140, 34);
        btnGhiSo.Text = "Ghi sổ";
        btnGhiSo.Click += OnGhiSoBam;

        btnIn.Enabled = false;
        btnIn.Location = new Point(310, 432);
        btnIn.Size = new Size(140, 34);
        btnIn.Text = "In biên nhận";
        btnIn.Click += OnInBam;

        btnDong.DialogResult = DialogResult.Cancel;
        btnDong.Location = new Point(460, 432);
        btnDong.Size = new Size(120, 34);
        btnDong.Text = "Đóng";

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(600, 490);
        Controls.Add(lblDoiTuong);
        Controls.Add(cmbDoiTuong);
        Controls.Add(lblNghiepVu);
        Controls.Add(cmbNghiepVu);
        Controls.Add(lblNguoiGui);
        Controls.Add(txtNguoiGuiHoTen);
        Controls.Add(lblQuanHe);
        Controls.Add(txtQuanHe);
        Controls.Add(lblHinhThuc);
        Controls.Add(cmbHinhThuc);
        Controls.Add(lblSoTaiKhoan);
        Controls.Add(txtSoTaiKhoan);
        Controls.Add(lblNgayChungTu);
        Controls.Add(dtpNgayChungTu);
        Controls.Add(lblSoTien);
        Controls.Add(txtSoTien);
        Controls.Add(lblBangChu);
        Controls.Add(lblNoiDung);
        Controls.Add(txtNoiDung);
        Controls.Add(lblTrangThai);
        Controls.Add(btnGhiSo);
        Controls.Add(btnIn);
        Controls.Add(btnDong);
        CancelButton = btnDong;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Lập biên nhận thu";
        ResumeLayout(false);
        PerformLayout();
    }
}
