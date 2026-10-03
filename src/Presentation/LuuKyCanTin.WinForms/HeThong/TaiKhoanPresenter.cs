using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.HeThong;

public sealed class TaiKhoanPresenter
{
    private readonly ITaiKhoanView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly Func<ITaoTaiKhoanView> _taoTaoTaiKhoan;
    private readonly Func<ITaiKhoanVaiTroView> _taoPhanVaiTro;
    private readonly Action<string> _hienMatKhauTam;
    private bool _dangXuLy;

    public TaiKhoanPresenter(
        ITaiKhoanView view,
        IServiceScopeFactory scopes,
        Func<ITaoTaiKhoanView> taoTaoTaiKhoan,
        Func<ITaiKhoanVaiTroView> taoPhanVaiTro,
        Action<string> hienMatKhauTam)
    {
        _view = view;
        _scopes = scopes;
        _taoTaoTaiKhoan = taoTaoTaiKhoan;
        _taoPhanVaiTro = taoPhanVaiTro;
        _hienMatKhauTam = hienMatKhauTam;
        _view.Loaded += async (_, _) => await TaiLaiAsync();
        _view.ThemClicked += async (_, _) => await MoTaoTaiKhoanAsync();
        _view.PhanVaiTroClicked += async (_, _) => await MoPhanVaiTroAsync();
        _view.NgungKichHoatClicked += async (_, _) => await NgungKichHoatAsync();
        _view.MoKhoaClicked += async (_, _) => await MoKhoaAsync();
        _view.DatLaiMatKhauClicked += async (_, _) => await DatLaiMatKhauAsync();
    }

    private async Task TaiLaiAsync()
    {
        using var scope = _scopes.CreateScope();
        var danhSach = await scope.ServiceProvider.GetRequiredService<ITaiKhoanService>().LayDanhSachAsync();
        _view.HienDanhSach(danhSach);
    }

    private async Task MoTaoTaiKhoanAsync()
    {
        using var hopThoai = _taoTaoTaiKhoan();
        _ = new TaoTaiKhoanPresenter(hopThoai, _scopes, _hienMatKhauTam);
        if (hopThoai.HienThi())
            await TaiLaiAsync();
    }

    private async Task MoPhanVaiTroAsync()
    {
        if (_view.TaiKhoanDangChon is not { } taiKhoan)
            return;

        using var hopThoai = _taoPhanVaiTro();
        _ = new TaiKhoanVaiTroPresenter(hopThoai, _scopes, taiKhoan);
        if (hopThoai.HienThi())
            await TaiLaiAsync();
    }

    private async Task NgungKichHoatAsync()
    {
        if (_dangXuLy || _view.TaiKhoanDangChon is not { } taiKhoan)
            return;

        if (taiKhoan.DangHoatDong)
        {
            if (!_view.CoDongY($"Ngừng hoạt động tài khoản '{taiKhoan.TenDangNhap}'?"))
                return;
            await ThucHienAsync(
                service => service.NgungHoatDongAsync(taiKhoan.Id),
                "Đã ngừng hoạt động tài khoản.");
        }
        else
        {
            await ThucHienAsync(
                service => service.KichHoatLaiAsync(taiKhoan.Id),
                "Đã kích hoạt lại tài khoản.");
        }
    }

    private async Task MoKhoaAsync()
    {
        if (_dangXuLy || _view.TaiKhoanDangChon is not { } taiKhoan)
            return;

        await ThucHienAsync(service => service.MoKhoaAsync(taiKhoan.Id), "Đã mở khoá tài khoản.");
    }

    private async Task DatLaiMatKhauAsync()
    {
        if (_dangXuLy || _view.TaiKhoanDangChon is not { } taiKhoan)
            return;
        if (!_view.CoDongY($"Đặt lại mật khẩu cho tài khoản '{taiKhoan.TenDangNhap}'?"))
            return;

        _dangXuLy = true;
        _view.HienThongBao("");
        try
        {
            using var scope = _scopes.CreateScope();
            var matKhauTam = await scope.ServiceProvider.GetRequiredService<ITaiKhoanService>()
                .DatLaiMatKhauAsync(taiKhoan.Id);
            await TaiLaiAsync();
            _hienMatKhauTam(matKhauTam);
            _view.HienThongBao("Đã đặt lại mật khẩu.");
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
            _view.HienLoi($"Không đặt lại được mật khẩu: {ex.Message}");
        }
        finally
        {
            _dangXuLy = false;
        }
    }

    private async Task ThucHienAsync(Func<ITaiKhoanService, Task> thaoTac, string thongBaoThanhCong)
    {
        _dangXuLy = true;
        _view.HienThongBao("");
        try
        {
            using var scope = _scopes.CreateScope();
            await thaoTac(scope.ServiceProvider.GetRequiredService<ITaiKhoanService>());
            await TaiLaiAsync();
            _view.HienThongBao(thongBaoThanhCong);
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
            _view.HienLoi($"Không thực hiện được: {ex.Message}");
        }
        finally
        {
            _dangXuLy = false;
        }
    }
}
