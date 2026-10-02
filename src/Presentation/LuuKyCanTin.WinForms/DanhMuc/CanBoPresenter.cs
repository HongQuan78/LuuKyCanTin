using LuuKyCanTin.Application.DanhMuc;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.DanhMuc;

public sealed class CanBoPresenter
{
    private readonly ICanBoView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly Func<ICanBoEditView> _taoHopThoai;
    private int _lanTim;

    public CanBoPresenter(ICanBoView view, IServiceScopeFactory scopes, Func<ICanBoEditView> taoHopThoai)
    {
        _view = view;
        _scopes = scopes;
        _taoHopThoai = taoHopThoai;
        _view.Loaded += async (_, _) => await TaiLaiAsync();
        _view.TimKiemThayDoi += async (_, _) => await TaiLaiAsync();
        _view.ThemClicked += async (_, _) => await MoHopThoaiAsync(canBo: null);
        _view.SuaClicked += async (_, _) =>
        {
            if (_view.CanBoDangChon is { } canBo)
                await MoHopThoaiAsync(canBo);
        };
    }

    private async Task TaiLaiAsync()
    {
        // Searches overlap while the user types; a slow earlier one must not overwrite the latest result.
        var lanNay = ++_lanTim;
        using var scope = _scopes.CreateScope();
        var danhSach = await scope.ServiceProvider.GetRequiredService<ICanBoService>()
            .TimAsync(_view.TuKhoa, _view.HienCaNguoiDaNghi);
        if (lanNay == _lanTim)
            _view.HienDanhSach(danhSach);
    }

    private async Task MoHopThoaiAsync(CanBoDto? canBo)
    {
        bool daLuu;
        using (var hopThoai = _taoHopThoai())
        {
            _ = new CanBoEditPresenter(hopThoai, _scopes, canBo);
            daLuu = hopThoai.HienThi();
        }

        if (daLuu)
            await TaiLaiAsync();
    }
}
