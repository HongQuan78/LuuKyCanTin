using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.HeThong;

public sealed class TaoTaiKhoanPresenter
{
    private readonly ITaoTaiKhoanView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly Action<string> _hienMatKhauTam;
    private bool _dangLuu;

    public TaoTaiKhoanPresenter(ITaoTaiKhoanView view, IServiceScopeFactory scopes, Action<string> hienMatKhauTam)
    {
        _view = view;
        _scopes = scopes;
        _hienMatKhauTam = hienMatKhauTam;
        _view.Loaded += async (_, _) => await TaiDanhSachAsync();
        _view.TaoClicked += OnTaoClicked;
    }

    private async Task TaiDanhSachAsync()
    {
        using var scope = _scopes.CreateScope();
        var canBo = await scope.ServiceProvider.GetRequiredService<ITaiKhoanService>().LayCanBoDeTaoTaiKhoanAsync();
        var vaiTro = await scope.ServiceProvider.GetRequiredService<IVaiTroService>().LayDanhSachAsync();
        _view.HienDanhSachCanBo(canBo);
        _view.HienDanhSachVaiTro(vaiTro);
    }

    private async void OnTaoClicked(object? sender, EventArgs e)
    {
        // A second click while the first save is still running would create the account twice.
        if (_dangLuu || _view.CanBoId is not { } canBoId)
            return;

        _dangLuu = true;
        try
        {
            var request = new TaoTaiKhoanRequest(_view.TenDangNhap, canBoId, _view.VaiTroDaChon);
            using var scope = _scopes.CreateScope();
            var ketQua = await scope.ServiceProvider.GetRequiredService<ITaiKhoanService>().TaoAsync(request);
            // The temporary password is shown once, then only its hash exists.
            _hienMatKhauTam(ketQua.MatKhauTam);
            _view.DongDaLuu();
        }
        catch (LoiNghiepVuException ex)
        {
            _view.HienLoi(ex.Message);
        }
        catch (KhongCoQuyenException ex)
        {
            _view.HienLoi(ex.Message);
        }
        catch (Exception ex)
        {
            _view.HienLoi($"Không tạo được tài khoản: {ex.Message}");
        }
        finally
        {
            _dangLuu = false;
        }
    }
}
