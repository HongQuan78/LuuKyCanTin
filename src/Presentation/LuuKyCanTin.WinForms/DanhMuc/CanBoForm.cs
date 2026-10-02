using LuuKyCanTin.Application.DanhMuc;

namespace LuuKyCanTin.WinForms.DanhMuc;

public partial class CanBoForm : Form, ICanBoView
{
    public CanBoForm()
    {
        InitializeComponent();
        colMaCanBo.DataPropertyName = nameof(CanBoDto.MaCanBo);
        colHoTen.DataPropertyName = nameof(CanBoDto.HoTen);
        colChucVu.DataPropertyName = nameof(CanBoDto.ChucVu);
        colLaQuanGiao.DataPropertyName = nameof(CanBoDto.LaQuanGiao);
        colDangCongTac.DataPropertyName = nameof(CanBoDto.DangCongTac);

        // Restarting the timer on every keystroke searches once typing pauses.
        txtTuKhoa.TextChanged += (_, _) =>
        {
            timerTimKiem.Stop();
            timerTimKiem.Start();
        };
        timerTimKiem.Tick += (_, _) =>
        {
            timerTimKiem.Stop();
            TimKiemThayDoi?.Invoke(this, EventArgs.Empty);
        };
        chkHienCaNguoiDaNghi.CheckedChanged += (_, _) => TimKiemThayDoi?.Invoke(this, EventArgs.Empty);
        btnThem.Click += (_, _) => ThemClicked?.Invoke(this, EventArgs.Empty);
        btnSua.Click += (_, _) => SuaClicked?.Invoke(this, EventArgs.Empty);
        gridCanBo.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                SuaClicked?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? Loaded;

    public event EventHandler? TimKiemThayDoi;

    public event EventHandler? ThemClicked;

    public event EventHandler? SuaClicked;

    public string TuKhoa => txtTuKhoa.Text;

    public bool HienCaNguoiDaNghi => chkHienCaNguoiDaNghi.Checked;

    public CanBoDto? CanBoDangChon => gridCanBo.CurrentRow?.DataBoundItem as CanBoDto;

    public void HienDanhSach(IReadOnlyList<CanBoDto> danhSach)
    {
        gridCanBo.DataSource = danhSach.ToList();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }
}
