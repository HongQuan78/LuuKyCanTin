using LuuKyCanTin.Domain.DanhMuc;

namespace LuuKyCanTin.WinForms.DanhMuc;

public partial class ThemDoiTuongForm : Form, IThemDoiTuongView
{
    private sealed record LoaiItem(LoaiDoiTuong GiaTri, string HienThi)
    {
        public override string ToString() => HienThi;
    }

    public ThemDoiTuongForm()
    {
        InitializeComponent();
        cmbLoaiDoiTuong.Items.AddRange(
        [
            new LoaiItem(LoaiDoiTuong.TamGiuTamGiam, "Tạm giữ/tạm giam"),
            new LoaiItem(LoaiDoiTuong.PhamNhan, "Phạm nhân"),
        ]);
        cmbLoaiDoiTuong.SelectedIndex = 0;
    }

    public event EventHandler? LuuBam;

    public string MaSo => txtMaSo.Text;

    public string HoTen => txtHoTen.Text;

    public short? NamSinh => numNamSinh.Value == 0 ? null : (short)numNamSinh.Value;

    public LoaiDoiTuong LoaiDoiTuong => ((LoaiItem)cmbLoaiDoiTuong.SelectedItem!).GiaTri;

    public DateOnly NgayVao => DateOnly.FromDateTime(dtpNgayVao.Value);

    public string? BuongGiam => txtBuongGiam.Text;

    public void HienLoi(string thongBao)
    {
        // The save is over; the user may correct the form and try again.
        btnLuu.Enabled = true;
        MessageBox.Show(this, thongBao, "Không lưu được", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    public void DongVoiKetQua(bool thanhCong)
    {
        DialogResult = thanhCong ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }

    private void OnLuuBam(object? sender, EventArgs e)
    {
        // Closing the window is instant; without this a double-click starts a second save.
        btnLuu.Enabled = false;
        LuuBam?.Invoke(this, EventArgs.Empty);
    }
}
