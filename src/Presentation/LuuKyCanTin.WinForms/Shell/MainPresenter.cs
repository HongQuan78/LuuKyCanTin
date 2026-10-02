using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LuuKyCanTin.WinForms.Shell;

public sealed class MainPresenter
{
    private readonly IMainView _view;
    private readonly AppOptions _options;
    private readonly IServiceScopeFactory _scopes;
    private bool _dangXuat;

    public MainPresenter(IMainView view, IOptions<AppOptions> options, IDieuHuong dieuHuong, IServiceScopeFactory scopes)
    {
        _view = view;
        _options = options.Value;
        _scopes = scopes;
        _view.Loaded += OnLoaded;
        _view.DanhMucCanBoClicked += (_, _) => dieuHuong.MoDanhMucCanBo();
        _view.VaiTroClicked += (_, _) => dieuHuong.MoVaiTro();
        _view.DoiMatKhauClicked += (_, _) => dieuHuong.MoDoiMatKhau();
        _view.DangXuatClicked += OnDangXuatClicked;
    }

    /// <summary>True after a successful sign-out, so the application context returns to the login form.</summary>
    public bool DaDangXuat => _dangXuat;

    private void OnLoaded(object? sender, EventArgs e)
    {
        _view.TieuDe = _options.TieuDe;
    }

    private async void OnDangXuatClicked(object? sender, EventArgs e)
    {
        if (_dangXuat)
            return;

        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<DangNhapService>().DangXuatAsync();
            _dangXuat = true;
            _view.Dong();
        }
        catch (Exception ex)
        {
            _view.HienLoi($"Không đăng xuất được: {ex.Message}");
        }
    }
}
