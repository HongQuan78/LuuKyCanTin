using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.HeThong;

public sealed class VaiTroPresenter
{
    private readonly IVaiTroView _view;
    private readonly IServiceScopeFactory _scopes;
    private IReadOnlyList<VaiTroDto> _danhSach = [];
    private byte[]? _rowVerDangChon;
    private bool _dangLuu;

    public VaiTroPresenter(IVaiTroView view, IServiceScopeFactory scopes)
    {
        _view = view;
        _scopes = scopes;
        _view.Loaded += async (_, _) => await TaiDanhSachAsync();
        _view.VaiTroThayDoi += async (_, _) => await TaiQuyenAsync();
        _view.LuuClicked += OnLuuClicked;
        _view.HuyThayDoiClicked += OnHuyThayDoiClicked;
    }

    private async void OnHuyThayDoiClicked(object? sender, EventArgs e)
    {
        _view.HienThongBao("");
        await TaiQuyenAsync();
    }

    private async Task TaiDanhSachAsync()
    {
        using var scope = _scopes.CreateScope();
        _danhSach = await scope.ServiceProvider.GetRequiredService<IVaiTroService>().LayDanhSachAsync();
        _view.HienDanhSachVaiTro(_danhSach);

        // Selecting a role raises VaiTroThayDoi; only load here if it didn't (for example an empty list).
        if (_view.VaiTroDangChon is not null)
            await TaiQuyenAsync();
    }

    private async Task TaiQuyenAsync()
    {
        if (_view.VaiTroDangChon is not { } vaiTroId)
            return;

        _rowVerDangChon = _danhSach.FirstOrDefault(v => v.Id == vaiTroId)?.RowVer;
        using var scope = _scopes.CreateScope();
        var maQuyen = await scope.ServiceProvider.GetRequiredService<IVaiTroService>()
            .LayQuyenCuaVaiTroAsync(vaiTroId);
        _view.HienQuyen(maQuyen);
    }

    private async void OnLuuClicked(object? sender, EventArgs e)
    {
        if (_dangLuu || _view.VaiTroDangChon is not { } vaiTroId || _rowVerDangChon is null)
            return;

        _dangLuu = true;
        _view.HienThongBao("");
        try
        {
            using var scope = _scopes.CreateScope();
            await scope.ServiceProvider.GetRequiredService<IVaiTroService>()
                .CapNhatQuyenAsync(vaiTroId, _view.QuyenDaChon, _rowVerDangChon);
            // Reload so the next save carries the new row version, then confirm (the reload clears the message).
            await TaiDanhSachAsync();
            _view.HienThongBao("Đã lưu quyền của vai trò.");
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
