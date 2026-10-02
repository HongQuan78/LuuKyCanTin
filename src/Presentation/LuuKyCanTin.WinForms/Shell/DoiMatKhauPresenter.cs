using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

public sealed class DoiMatKhauPresenter
{
    private readonly IDoiMatKhauView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly bool _batBuoc;
    private bool _dangLuu;

    /// <param name="batBuoc">Forced mode: cancel signs the temporary session out and returns to login.</param>
    public DoiMatKhauPresenter(IDoiMatKhauView view, IServiceScopeFactory scopes, bool batBuoc)
    {
        _view = view;
        _scopes = scopes;
        _batBuoc = batBuoc;
        _view.TieuDe = batBuoc ? "Đổi mật khẩu lần đầu" : "Đổi mật khẩu";
        _view.BatBuoc = batBuoc;
        _view.LuuClicked += OnLuuClicked;
        _view.HuyClicked += OnHuyClicked;
    }

    private async void OnLuuClicked(object? sender, EventArgs e)
    {
        // A second click while the first save is still running would hash twice for nothing.
        if (_dangLuu)
            return;
        _dangLuu = true;
        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<DoiMatKhauService>()
                .DoiMatKhauAsync(_view.MatKhauHienTai, _view.MatKhauMoi, _view.XacNhan);
            _view.DongVoiKetQua(true);
        }
        catch (LoiNghiepVuException ex)
        {
            _view.HienLoi(ex.Message);
        }
        catch (Exception ex)
        {
            _view.HienLoi($"Không đổi được mật khẩu: {ex.Message}");
        }
        finally
        {
            _dangLuu = false;
        }
    }

    private async void OnHuyClicked(object? sender, EventArgs e)
    {
        try
        {
            if (_batBuoc)
            {
                using var scope = _scopes.CreateScope();
                await scope.ServiceProvider.GetRequiredService<DangNhapService>().DangXuatAsync();
            }
        }
        catch (Exception ex)
        {
            _view.HienLoi($"Không đăng xuất được: {ex.Message}");
            return;
        }

        _view.DongVoiKetQua(false);
    }
}
