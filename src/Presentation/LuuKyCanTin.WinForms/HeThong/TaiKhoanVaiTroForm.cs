using System.ComponentModel;
using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

public partial class TaiKhoanVaiTroForm : Form, ITaiKhoanVaiTroView
{
    private readonly List<VaiTroDto> _danhSachVaiTro = [];

    public TaiKhoanVaiTroForm()
    {
        InitializeComponent();
        btnLuu.Click += (_, _) => LuuClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Loaded;

    public event EventHandler? LuuClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TieuDe
    {
        set => Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<int> VaiTroDaChon =>
        lstVaiTro.CheckedIndices.Cast<int>().Select(index => _danhSachVaiTro[index].Id).ToList();

    public void HienDanhSachVaiTro(IReadOnlyList<VaiTroDto> danhSach)
    {
        _danhSachVaiTro.Clear();
        _danhSachVaiTro.AddRange(danhSach);
        lstVaiTro.Items.Clear();
        foreach (var vaiTro in danhSach)
            lstVaiTro.Items.Add(vaiTro.Ten);
    }

    public void HienVaiTroDaChon(IReadOnlyList<int> vaiTroIds)
    {
        var tap = vaiTroIds.ToHashSet();
        for (var i = 0; i < _danhSachVaiTro.Count; i++)
            lstVaiTro.SetItemChecked(i, tap.Contains(_danhSachVaiTro[i].Id));
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
