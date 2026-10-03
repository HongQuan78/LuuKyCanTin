using System.ComponentModel;
using LuuKyCanTin.WinForms.DanhMuc;
using LuuKyCanTin.WinForms.LuuKy;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

public partial class MainForm : Form, IMainView
{
    // The designer needs a parameterless constructor; the app resolves the scope factory one.
    private readonly IServiceScopeFactory? _scopeFactory;

    public MainForm()
        : this(null)
    {
    }

    public MainForm(IServiceScopeFactory? scopeFactory)
    {
        _scopeFactory = scopeFactory;
        InitializeComponent();
        mnuCanBo.Click += (_, _) => DanhMucCanBoClicked?.Invoke(this, EventArgs.Empty);
        mnuTaiKhoan.Click += (_, _) => TaiKhoanClicked?.Invoke(this, EventArgs.Empty);
        mnuVaiTro.Click += (_, _) => VaiTroClicked?.Invoke(this, EventArgs.Empty);
        mnuDoiMatKhau.Click += (_, _) => DoiMatKhauClicked?.Invoke(this, EventArgs.Empty);
        mnuDangXuat.Click += (_, _) => DangXuatClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Loaded;

    public event EventHandler? DanhMucCanBoClicked;

    public event EventHandler? TaiKhoanClicked;

    public event EventHandler? VaiTroClicked;

    public event EventHandler? DoiMatKhauClicked;

    public event EventHandler? DangXuatClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TieuDe
    {
        set => Text = value;
    }

    public void Dong() => Close();

    public void HienLoi(string thongBao) =>
        MessageBox.Show(this, thongBao, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    private void OnThemDoiTuong(object? sender, EventArgs e)
    {
        using var scope = _scopeFactory!.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<ThemDoiTuongForm>();
        ActivatorUtilities.CreateInstance<ThemDoiTuongPresenter>(scope.ServiceProvider, form);
        form.ShowDialog(this);
    }

    private void OnLapBienNhanThu(object? sender, EventArgs e)
    {
        using var scope = _scopeFactory!.CreateScope();
        var form = scope.ServiceProvider.GetRequiredService<BienNhanThuForm>();
        ActivatorUtilities.CreateInstance<BienNhanThuPresenter>(scope.ServiceProvider, form);
        form.ShowDialog(this);
    }
}
