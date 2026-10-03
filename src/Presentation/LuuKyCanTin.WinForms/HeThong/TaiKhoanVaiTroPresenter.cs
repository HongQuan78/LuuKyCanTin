using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.HeThong;

public sealed class TaiKhoanVaiTroPresenter
{
    private readonly ITaiKhoanVaiTroView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly TaiKhoanDto _taiKhoan;
    private bool _dangLuu;

    public TaiKhoanVaiTroPresenter(ITaiKhoanVaiTroView view, IServiceScopeFactory scopes, TaiKhoanDto taiKhoan)
    {
        _view = view;
        _scopes = scopes;
        _taiKhoan = taiKhoan;
        _view.TieuDe = $"Phân vai trò — {taiKhoan.TenDangNhap}";
        _view.Loaded += async (_, _) => await TaiDanhSachAsync();
        _view.LuuClicked += OnLuuClicked;
    }

    private async Task TaiDanhSachAsync()
    {
        using var scope = _scopes.CreateScope();
        var vaiTro = await scope.ServiceProvider.GetRequiredService<IVaiTroService>().LayDanhSachAsync();
        _view.HienDanhSachVaiTro(vaiTro);
        _view.HienVaiTroDaChon(_taiKhoan.VaiTroIds);
    }

    private async void OnLuuClicked(object? sender, EventArgs e)
    {
        if (_dangLuu)
            return;

        _dangLuu = true;
        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ITaiKhoanService>()
                .CapNhatVaiTroAsync(_taiKhoan.Id, _view.VaiTroDaChon);
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
            _view.HienLoi($"Không lưu được: {ex.Message}");
        }
        finally
        {
            _dangLuu = false;
        }
    }
}
