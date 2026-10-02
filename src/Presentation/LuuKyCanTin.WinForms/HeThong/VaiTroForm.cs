using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

public partial class VaiTroForm : Form, IVaiTroView
{
    private readonly List<string> _hanhDong = [.. MaQuyen.CacHanhDong];
    private readonly List<MaQuyen.DinhNghiaQuyen> _quyenDacBiet = [.. MaQuyen.TatCa.Where(MaQuyen.LaQuyenDacBiet)];

    public VaiTroForm()
    {
        InitializeComponent();
        lstVaiTro.DisplayMember = nameof(VaiTroDto.Ten);
        lstVaiTro.SelectedIndexChanged += (_, _) => VaiTroThayDoi?.Invoke(this, EventArgs.Empty);
        btnLuu.Click += (_, _) => LuuClicked?.Invoke(this, EventArgs.Empty);
        btnHuy.Click += (_, _) => HuyThayDoiClicked?.Invoke(this, EventArgs.Empty);
        TaoLuoi();
    }

    public event EventHandler? Loaded;

    public event EventHandler? VaiTroThayDoi;

    public event EventHandler? LuuClicked;

    public event EventHandler? HuyThayDoiClicked;

    public int? VaiTroDangChon => (lstVaiTro.SelectedItem as VaiTroDto)?.Id;

    public IReadOnlyList<string> QuyenDaChon
    {
        get
        {
            // A ticked cell is not committed until the cell loses focus.
            gridQuyen.CommitEdit(DataGridViewDataErrorContexts.Commit);

            var ketQua = new List<string>();
            foreach (DataGridViewRow row in gridQuyen.Rows)
            {
                var module = (string)row.Tag!;
                for (var i = 0; i < _hanhDong.Count; i++)
                {
                    if (row.Cells[i + 1].Value is true)
                        ketQua.Add($"{module}.{_hanhDong[i]}");
                }
            }

            foreach (var index in lstQuyenDacBiet.CheckedIndices)
                ketQua.Add(_quyenDacBiet[(int)index].Ma);

            return ketQua;
        }
    }

    public void HienDanhSachVaiTro(IReadOnlyList<VaiTroDto> danhSach)
    {
        var dangChon = VaiTroDangChon;
        lstVaiTro.DataSource = null;
        lstVaiTro.DataSource = danhSach.ToList();

        if (dangChon is { } id && danhSach.Any(v => v.Id == id))
            lstVaiTro.SelectedItem = danhSach.First(v => v.Id == id);
        else if (danhSach.Count > 0)
            lstVaiTro.SelectedIndex = 0;
    }

    public void HienQuyen(IReadOnlyList<string> maQuyen)
    {
        var tap = maQuyen.ToHashSet(StringComparer.Ordinal);
        foreach (DataGridViewRow row in gridQuyen.Rows)
        {
            var module = (string)row.Tag!;
            for (var i = 0; i < _hanhDong.Count; i++)
                row.Cells[i + 1].Value = tap.Contains($"{module}.{_hanhDong[i]}");
        }

        for (var i = 0; i < _quyenDacBiet.Count; i++)
            lstQuyenDacBiet.SetItemChecked(i, tap.Contains(_quyenDacBiet[i].Ma));
    }

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

    private void TaoLuoi()
    {
        gridQuyen.Columns.Clear();
        gridQuyen.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colNhomQuyen",
            HeaderText = "Nhóm quyền",
            ReadOnly = true,
            FillWeight = 30F,
        });
        foreach (var hanhDong in _hanhDong)
        {
            gridQuyen.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "col" + hanhDong,
                HeaderText = hanhDong,
                FillWeight = 11F,
            });
        }

        foreach (var module in MaQuyen.CacModule)
        {
            var index = gridQuyen.Rows.Add();
            var row = gridQuyen.Rows[index];
            row.Tag = module.Ma;
            row.Cells[0].Value = module.Ten;
        }

        // Special permissions arrive with later stories; the list fills itself from the catalogue.
        foreach (var quyen in _quyenDacBiet)
            lstQuyenDacBiet.Items.Add(quyen.Ten);
        lblQuyenDacBiet.Visible = _quyenDacBiet.Count > 0;
        lstQuyenDacBiet.Visible = _quyenDacBiet.Count > 0;
    }
}
