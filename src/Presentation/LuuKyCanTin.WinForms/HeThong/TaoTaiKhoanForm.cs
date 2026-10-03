using System.ComponentModel;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

public partial class TaoTaiKhoanForm : Form, ITaoTaiKhoanView
{
    private readonly List<CanBoDto> _danhSachCanBo = [];
    private readonly List<VaiTroDto> _danhSachVaiTro = [];

    public TaoTaiKhoanForm()
    {
        InitializeComponent();
        btnTao.Click += (_, _) => TaoClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Loaded;

    public event EventHandler? TaoClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TenDangNhap => txtTenDangNhap.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? CanBoId => cboCanBo.SelectedIndex >= 0 ? _danhSachCanBo[cboCanBo.SelectedIndex].Id : null;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<int> VaiTroDaChon =>
        lstVaiTro.CheckedIndices.Cast<int>().Select(index => _danhSachVaiTro[index].Id).ToList();

    public void HienDanhSachCanBo(IReadOnlyList<CanBoDto> danhSach)
    {
        _danhSachCanBo.Clear();
        _danhSachCanBo.AddRange(danhSach);
        cboCanBo.Items.Clear();
        foreach (var canBo in danhSach)
            cboCanBo.Items.Add($"{canBo.HoTen} ({canBo.MaCanBo})");

        if (cboCanBo.Items.Count > 0)
            cboCanBo.SelectedIndex = 0;
    }

    public void HienDanhSachVaiTro(IReadOnlyList<VaiTroDto> danhSach)
    {
        _danhSachVaiTro.Clear();
        _danhSachVaiTro.AddRange(danhSach);
        lstVaiTro.Items.Clear();
        foreach (var vaiTro in danhSach)
            lstVaiTro.Items.Add(vaiTro.Ten);
    }

    public bool HienThi() => ShowDialog() == DialogResult.OK;

    public void HienLoi(string thongBao)
    {
        lblLoi.ForeColor = Color.Firebrick;
        lblLoi.Text = thongBao;
    }

    public void DongDaLuu()
    {
        DialogResult = DialogResult.OK;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }
}
