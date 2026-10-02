using LuuKyCanTin.Application.HeThong;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Thin sign-in for the skeleton; the full rules (lockout, forced change) are Epic 2.</summary>
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

    public async Task DangNhapAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var dangNhap = scope.ServiceProvider.GetRequiredService<DangNhapService>();

        var ketQua = await dangNhap.DangNhapAsync(_view.TenDangNhap, _view.MatKhau);
        if (ketQua.ThanhCong)
            _view.DongVoiKetQua(true);
        else
            _view.HienLoi(ketQua.ThongBao ?? DangNhapService.SaiThongTin);
    }
}
