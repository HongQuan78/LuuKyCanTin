using LuuKyCanTin.Application.HeThong;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Sign-in with lockout and the forced first change (FR1). The shell opens only on a successful result.</summary>
public sealed class LoginPresenter
{
    private readonly ILoginView _view;
    private readonly IServiceScopeFactory _scopeFactory;

    public LoginPresenter(ILoginView view, IServiceScopeFactory scopeFactory)
    {
        _view = view;
        _scopeFactory = scopeFactory;
        _view.DangNhapBam += async (_, _) =>
        {
            try
            {
                await DangNhapAsync();
            }
            catch (Exception ex)
            {
                // An unexpected failure (database down) must still reach the user, not the global handler.
                _view.HienLoi($"Không đăng nhập được: {ex.Message}");
            }
        };
    }

    /// <summary>Set after a successful sign-in; true when the shell must wait for a password change.</summary>
    public bool PhaiDoiMatKhau { get; private set; }

    public async Task DangNhapAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dangNhap = scope.ServiceProvider.GetRequiredService<DangNhapService>();

        var ketQua = await dangNhap.DangNhapAsync(_view.TenDangNhap, _view.MatKhau);
        if (ketQua.ThanhCong)
        {
            PhaiDoiMatKhau = ketQua.PhaiDoiMatKhau;
            _view.DongVoiKetQua(true);
            return;
        }

        _view.HienLoi(ketQua.ThongBao ?? DangNhapService.SaiThongTin);
        if (ketQua.TrangThai == TrangThaiDangNhap.TaiKhoanBiKhoa)
            _view.XoaMatKhau();
    }
}
