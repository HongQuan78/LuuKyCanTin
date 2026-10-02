using LuuKyCanTin.Application.DanhMuc;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.DanhMuc;

/// <summary>The skeleton's minimal detainee form. Search, edit and type history are Epic 3.</summary>
public sealed class ThemDoiTuongPresenter
{
    private readonly IThemDoiTuongView _view;
    private readonly IServiceScopeFactory _scopeFactory;

    public ThemDoiTuongPresenter(IThemDoiTuongView view, IServiceScopeFactory scopeFactory)
    {
        _view = view;
        _scopeFactory = scopeFactory;
        _view.LuuBam += async (_, _) =>
        {
            try
            {
                await LuuAsync();
            }
            catch (Exception ex)
            {
                // An unexpected failure (database down) must still reach the user, not the global handler.
                _view.HienLoi($"Không lưu được đối tượng: {ex.Message}");
            }
        };
    }

    public async Task LuuAsync()
    {
        // Every operation gets a fresh scope; the form never holds a DbContext.
        await using var scope = _scopeFactory.CreateAsyncScope();
        var themDoiTuong = scope.ServiceProvider.GetRequiredService<ThemDoiTuongService>();

        var request = new ThemDoiTuongRequest(
            _view.MaSo, _view.HoTen, _view.NamSinh, _view.LoaiDoiTuong, _view.NgayVao, _view.BuongGiam);

        var ketQua = await themDoiTuong.ThemAsync(request);
        if (ketQua.ThanhCong)
            _view.DongVoiKetQua(true);
        else
            _view.HienLoi(ketQua.ThongBao ?? "Không lưu được đối tượng.");
    }
}
