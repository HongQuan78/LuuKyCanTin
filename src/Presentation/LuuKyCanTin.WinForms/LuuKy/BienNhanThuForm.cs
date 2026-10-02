using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Domain.LuuKy;

namespace LuuKyCanTin.WinForms.LuuKy;

public partial class BienNhanThuForm : Form, IBienNhanThuView
{
    private sealed record NghiepVuItem(NghiepVu GiaTri, string HienThi)
    {
        public override string ToString() => HienThi;
    }

    private sealed record HinhThucItem(HinhThuc GiaTri, string HienThi)
    {
        public override string ToString() => HienThi;
    }

    private bool _daGhiSo;

    public BienNhanThuForm()
    {
        InitializeComponent();

        // Nghiệp vụ 13 (phiếu gửi qua) stays out until DEC-05 is decided (alignment A4).
        cmbNghiepVu.Items.AddRange(
        [
            new NghiepVuItem(NghiepVu.NguoiThanGui, "12 – Người thân gửi"),
            new NghiepVuItem(NghiepVu.MangTheoKhiVao, "11 – Mang theo khi vào"),
            new NghiepVuItem(NghiepVu.NhanTuDoiTuongKhac, "14 – Nhận từ đối tượng khác"),
        ]);
        cmbNghiepVu.SelectedIndex = 0;

        cmbHinhThuc.Items.AddRange(
        [
            new HinhThucItem(HinhThuc.TienMat, "Tiền mặt"),
            new HinhThucItem(HinhThuc.ChuyenKhoan, "Chuyển khoản"),
        ]);
        cmbHinhThuc.SelectedIndex = 0;
        CapNhatTaiKhoan();
        Load += OnFormLoad;
    }

    public event EventHandler? Tai;

    public event EventHandler? SoTienThayDoi;

    public event EventHandler? GhiSoBam;

    public event EventHandler? InBam;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<DoiTuongChon> DanhSachDoiTuong
    {
        set
        {
            cmbDoiTuong.DataSource = value.ToList();
            cmbDoiTuong.DisplayMember = nameof(DoiTuongChon.HienThi);
            cmbDoiTuong.ValueMember = nameof(DoiTuongChon.Id);
            cmbDoiTuong.SelectedIndex = value.Count > 0 ? 0 : -1;
        }
    }

    public int? DoiTuongId => cmbDoiTuong.SelectedItem is DoiTuongChon chon ? chon.Id : null;

    public NghiepVu NghiepVu => ((NghiepVuItem)cmbNghiepVu.SelectedItem!).GiaTri;

    public string? NguoiGuiHoTen => txtNguoiGuiHoTen.Text;

    public string? QuanHe => txtQuanHe.Text;

    public HinhThuc HinhThuc => ((HinhThucItem)cmbHinhThuc.SelectedItem!).GiaTri;

    public string? SoTaiKhoanNguoiGui => txtSoTaiKhoan.Text;

    public DateOnly NgayChungTu => DateOnly.FromDateTime(dtpNgayChungTu.Value);

    public string? NoiDung => txtNoiDung.Text;

    public decimal? SoTien
    {
        get
        {
            var chuSo = txtSoTien.Text.Trim();

            // vi-VN: '.' groups thousands, ',' is the decimal separator. Money is whole đồng, so only a plain
            // integer or well-formed 3-digit groups parse; "12.5" and "1.234,5" return null for the validator.
            if (!Regex.IsMatch(chuSo, @"^\d{1,3}(\.\d{3})*$|^\d+$"))
                return null;

            return decimal.TryParse(chuSo, NumberStyles.AllowThousands, CultureInfo.GetCultureInfo("vi-VN"), out var soTien)
                ? soTien
                : null;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string SoTienBangChu
    {
        set => lblBangChu.Text = value.Length == 0 ? "" : $"Viết bằng chữ: {value}";
    }

    public void HienLoi(string thongBao)
    {
        // The post is over; allow another attempt, unless this form already posted a receipt.
        if (!_daGhiSo)
            btnGhiSo.Enabled = true;

        MessageBox.Show(this, thongBao, "Không ghi sổ được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void DaGhiSo(string soChungTu, decimal soDuSau)
    {
        _daGhiSo = true;
        lblTrangThai.Text = $"Đã ghi sổ {soChungTu} · Số dư mới: {soDuSau.ToString("N0", CultureInfo.GetCultureInfo("vi-VN"))} đồng";
        btnGhiSo.Enabled = false;
        btnIn.Enabled = true;
    }

    public void HienThiBanIn(byte[] pdf, string tenTep)
    {
        using var preview = new Common.PdfPreviewForm(pdf, tenTep);
        preview.ShowDialog(this);
    }

    private void OnFormLoad(object? sender, EventArgs e) => Tai?.Invoke(this, EventArgs.Empty);

    private void OnSoTienThayDoi(object? sender, EventArgs e) => SoTienThayDoi?.Invoke(this, EventArgs.Empty);

    private void OnGhiSoBam(object? sender, EventArgs e)
    {
        // A second click while the post runs would start a second transaction and could post twice.
        btnGhiSo.Enabled = false;
        GhiSoBam?.Invoke(this, EventArgs.Empty);
    }

    private void OnInBam(object? sender, EventArgs e) => InBam?.Invoke(this, EventArgs.Empty);

    private void OnHinhThucThayDoi(object? sender, EventArgs e) => CapNhatTaiKhoan();

    private void CapNhatTaiKhoan()
    {
        var chuyenKhoan = cmbHinhThuc.SelectedItem is HinhThucItem { GiaTri: HinhThuc.ChuyenKhoan };
        txtSoTaiKhoan.Enabled = chuyenKhoan;
        lblSoTaiKhoan.Enabled = chuyenKhoan;
    }
}
