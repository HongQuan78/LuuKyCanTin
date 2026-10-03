namespace LuuKyCanTin.WinForms.HeThong;

partial class TaiKhoanForm
{
    private System.ComponentModel.IContainer components = null!;
    private DataGridView gridTaiKhoan = null!;
    private DataGridViewTextBoxColumn colTenDangNhap = null!;
    private DataGridViewTextBoxColumn colCanBo = null!;
    private DataGridViewTextBoxColumn colVaiTro = null!;
    private DataGridViewCheckBoxColumn colDangHoatDong = null!;
    private DataGridViewCheckBoxColumn colDangBiKhoa = null!;
    private Button btnThem = null!;
    private Button btnPhanVaiTro = null!;
    private Button btnNgungKichHoat = null!;
    private Button btnMoKhoa = null!;
    private Button btnDatLaiMatKhau = null!;
    private Label lblThongBao = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        gridTaiKhoan = new DataGridView();
        colTenDangNhap = new DataGridViewTextBoxColumn();
        colCanBo = new DataGridViewTextBoxColumn();
        colVaiTro = new DataGridViewTextBoxColumn();
        colDangHoatDong = new DataGridViewCheckBoxColumn();
        colDangBiKhoa = new DataGridViewCheckBoxColumn();
        btnThem = new Button();
        btnPhanVaiTro = new Button();
        btnNgungKichHoat = new Button();
        btnMoKhoa = new Button();
        btnDatLaiMatKhau = new Button();
        lblThongBao = new Label();
        ((System.ComponentModel.ISupportInitialize)gridTaiKhoan).BeginInit();
        SuspendLayout();

        gridTaiKhoan.AllowUserToAddRows = false;
        gridTaiKhoan.AllowUserToDeleteRows = false;
        gridTaiKhoan.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        gridTaiKhoan.AutoGenerateColumns = false;
        gridTaiKhoan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridTaiKhoan.Columns.AddRange(
            [colTenDangNhap, colCanBo, colVaiTro, colDangHoatDong, colDangBiKhoa]);
        gridTaiKhoan.Location = new Point(12, 45);
        gridTaiKhoan.MultiSelect = false;
        gridTaiKhoan.Name = "gridTaiKhoan";
        gridTaiKhoan.ReadOnly = true;
        gridTaiKhoan.RowHeadersVisible = false;
        gridTaiKhoan.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridTaiKhoan.Size = new Size(876, 415);
        gridTaiKhoan.TabIndex = 5;

        colTenDangNhap.FillWeight = 20F;
        colTenDangNhap.HeaderText = "Tên đăng nhập";
        colTenDangNhap.Name = "colTenDangNhap";

        colCanBo.FillWeight = 30F;
        colCanBo.HeaderText = "Cán bộ";
        colCanBo.Name = "colCanBo";

        colVaiTro.FillWeight = 34F;
        colVaiTro.HeaderText = "Vai trò";
        colVaiTro.Name = "colVaiTro";

        colDangHoatDong.FillWeight = 8F;
        colDangHoatDong.HeaderText = "Hoạt động";
        colDangHoatDong.Name = "colDangHoatDong";

        colDangBiKhoa.FillWeight = 8F;
        colDangBiKhoa.HeaderText = "Bị khoá";
        colDangBiKhoa.Name = "colDangBiKhoa";

        btnThem.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnThem.Location = new Point(12, 12);
        btnThem.Name = "btnThem";
        btnThem.Size = new Size(90, 25);
        btnThem.TabIndex = 0;
        btnThem.Text = "&Thêm";

        btnPhanVaiTro.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnPhanVaiTro.Location = new Point(108, 12);
        btnPhanVaiTro.Name = "btnPhanVaiTro";
        btnPhanVaiTro.Size = new Size(110, 25);
        btnPhanVaiTro.TabIndex = 1;
        btnPhanVaiTro.Text = "Phân &vai trò";

        btnNgungKichHoat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnNgungKichHoat.Location = new Point(224, 12);
        btnNgungKichHoat.Name = "btnNgungKichHoat";
        btnNgungKichHoat.Size = new Size(140, 25);
        btnNgungKichHoat.TabIndex = 2;
        btnNgungKichHoat.Text = "&Ngừng/Kích hoạt";

        btnMoKhoa.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnMoKhoa.Location = new Point(370, 12);
        btnMoKhoa.Name = "btnMoKhoa";
        btnMoKhoa.Size = new Size(90, 25);
        btnMoKhoa.TabIndex = 3;
        btnMoKhoa.Text = "&Mở khoá";

        btnDatLaiMatKhau.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnDatLaiMatKhau.Location = new Point(466, 12);
        btnDatLaiMatKhau.Name = "btnDatLaiMatKhau";
        btnDatLaiMatKhau.Size = new Size(130, 25);
        btnDatLaiMatKhau.TabIndex = 4;
        btnDatLaiMatKhau.Text = "Đặt lại &mật khẩu";

        lblThongBao.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        lblThongBao.Location = new Point(12, 468);
        lblThongBao.Size = new Size(876, 40);
        lblThongBao.Text = "";

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 520);
        Controls.Add(lblThongBao);
        Controls.Add(btnDatLaiMatKhau);
        Controls.Add(btnMoKhoa);
        Controls.Add(btnNgungKichHoat);
        Controls.Add(btnPhanVaiTro);
        Controls.Add(btnThem);
        Controls.Add(gridTaiKhoan);
        MinimumSize = new Size(760, 400);
        Name = "TaiKhoanForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Tài khoản";
        ((System.ComponentModel.ISupportInitialize)gridTaiKhoan).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
