using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.DanhMuc;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.DanhMuc;

public sealed class CanBoEditPresenter
{
    private readonly ICanBoEditView _view;
    private readonly IServiceScopeFactory _scopes;
    private readonly CanBoDto? _canBo;
    private bool _dangLuu;

    /// <param name="canBo">The staff member to edit, or null to add one.</param>
    public CanBoEditPresenter(ICanBoEditView view, IServiceScopeFactory scopes, CanBoDto? canBo)
    {
        _view = view;
        _scopes = scopes;
        _canBo = canBo;
        _view.LuuClicked += OnLuuClicked;

        if (canBo is null)
        {
            _view.TieuDe = "Thêm cán bộ";
            _view.DangCongTac = true;
            return;
        }

        _view.TieuDe = "Sửa cán bộ";
        _view.MaCanBo = canBo.MaCanBo;
        _view.HoTen = canBo.HoTen;
        _view.ChucVu = canBo.ChucVu ?? "";
        _view.LaQuanGiao = canBo.LaQuanGiao;
        _view.DangCongTac = canBo.DangCongTac;
    }

    private async void OnLuuClicked(object? sender, EventArgs e)
    {
        // A second click while the first save is still running would add the same person twice.
        if (_dangLuu)
            return;
        _dangLuu = true;
        try
        {
            var request = new LuuCanBoRequest(_view.MaCanBo, _view.HoTen, _view.ChucVu, _view.LaQuanGiao, _view.DangCongTac);
            using var scope = _scopes.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<ICanBoService>();
            if (_canBo is null)
                await service.ThemAsync(request);
            else
                await service.SuaAsync(_canBo.Id, request with { RowVer = _canBo.RowVer });
            _view.DongDaLuu();
        }
        catch (LoiNghiepVuException ex)
        {
            _view.HienLoi(ex.Message);
        }
        finally
        {
            _dangLuu = false;
        }
    }
}
