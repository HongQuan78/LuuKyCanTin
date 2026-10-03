using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

public partial class TaiKhoanForm : Form, ITaiKhoanView
{
    public TaiKhoanForm()
    {
        InitializeComponent();
        colTenDangNhap.DataPropertyName = nameof(TaiKhoanDto.TenDangNhap);
        colCanBo.DataPropertyName = nameof(TaiKhoanDto.HoTenCanBo);
        colVaiTro.DataPropertyName = nameof(TaiKhoanDto.TenVaiTro);
        colDangHoatDong.DataPropertyName = nameof(TaiKhoanDto.DangHoatDong);
        colDangBiKhoa.DataPropertyName = nameof(TaiKhoanDto.DangBiKhoa);

        btnThem.Click += (_, _) => ThemClicked?.Invoke(this, EventArgs.Empty);
        btnPhanVaiTro.Click += (_, _) => PhanVaiTroClicked?.Invoke(this, EventArgs.Empty);
        btnNgungKichHoat.Click += (_, _) => NgungKichHoatClicked?.Invoke(this, EventArgs.Empty);
        btnMoKhoa.Click += (_, _) => MoKhoaClicked?.Invoke(this, EventArgs.Empty);
        btnDatLaiMatKhau.Click += (_, _) => DatLaiMatKhauClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Loaded;

    public event EventHandler? ThemClicked;

    public event EventHandler? PhanVaiTroClicked;

    public event EventHandler? NgungKichHoatClicked;

    public event EventHandler? MoKhoaClicked;

    public event EventHandler? DatLaiMatKhauClicked;

    public TaiKhoanDto? TaiKhoanDangChon => gridTaiKhoan.CurrentRow?.DataBoundItem as TaiKhoanDto;

    public void HienDanhSach(IReadOnlyList<TaiKhoanDto> danhSach)
    {
        gridTaiKhoan.DataSource = danhSach.ToList();
    }

    public bool CoDongY(string thongBao) =>
        MessageBox.Show(this, thongBao, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

    public void HienThongBao(string thongBao)
    {
        lblThongBao.ForeColor = Color.ForestGreen;
        lblThongBao.Text = thongBao;
    }

    public void HienLoi(string thongBao)
    {
        lblThongBao.ForeColor = Color.Firebrick;
        lblThongBao.Text = thongBao;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }
}
