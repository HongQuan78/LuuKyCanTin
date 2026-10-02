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
    }

    public event EventHandler? Loaded;

    public event EventHandler? DanhMucCanBoClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string TieuDe
    {
        set => Text = value;
    }

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
