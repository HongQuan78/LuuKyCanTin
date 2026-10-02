using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.BaoCao;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.LuuKy;
using LuuKyCanTin.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.LuuKy;

/// <summary>
/// The skeleton's receipt form: pick a detainee, enter the receipt, post through the ledger engine, print.
/// The full form (A.I.4 fields, money control UX-DR1, pre-posting confirmation UX-DR4) is Epic 4.
/// </summary>
public sealed class BienNhanThuPresenter
{
    private readonly IBienNhanThuView _view;
    private readonly IServiceScopeFactory _scopeFactory;

    private long? _chungTuId;

    public BienNhanThuPresenter(IBienNhanThuView view, IServiceScopeFactory scopeFactory)
    {
        _view = view;
        _scopeFactory = scopeFactory;
        _view.Tai += async (_, _) => await BaoLoiAsync("Không tải được danh sách đối tượng", TaiAsync);
        _view.SoTienThayDoi += (_, _) => CapNhatBangChu();
        _view.GhiSoBam += async (_, _) => await BaoLoiAsync("Không ghi sổ được", GhiSoAsync);
        _view.InBam += async (_, _) => await BaoLoiAsync("Không in được", InAsync);
    }

    // An unexpected failure (database down, render error) must still reach the user, not the global handler.
    private async Task BaoLoiAsync(string thongDiep, Func<Task> viec)
    {
        try
        {
            await viec();
        }
        catch (Exception ex)
        {
            _view.HienLoi($"{thongDiep}: {ex.Message}");
        }
    }

    public async Task TaiAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var query = scope.ServiceProvider.GetRequiredService<LayDoiTuongDangQuanLyQuery>();
        _view.DanhSachDoiTuong = await query.LayAsync();
    }

    public void CapNhatBangChu()
    {
        var soTien = _view.SoTien;
        _view.SoTienBangChu = soTien is > 0 ? SoTienBangChu.Doc(soTien.Value) : "";
    }

    public async Task GhiSoAsync()
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var ghiSo = scope.ServiceProvider.GetRequiredService<GhiSoLuuKyService>();

        var request = new GhiSoBienNhanThuRequest
        {
            DoiTuongId = _view.DoiTuongId ?? 0,
            NgayChungTu = _view.NgayChungTu,
            NghiepVu = _view.NghiepVu,
            HinhThuc = _view.HinhThuc,
            NguoiGuiHoTen = _view.NguoiGuiHoTen,
            QuanHe = _view.QuanHe,
            SoTaiKhoanNguoiGui = _view.SoTaiKhoanNguoiGui,
            NoiDung = _view.NoiDung,
            SoTien = _view.SoTien ?? 0,
        };

        var ketQua = await ghiSo.GhiSoBienNhanThuAsync(request);
        if (!ketQua.ThanhCong)
        {
            _view.HienLoi(ketQua.ThongBao ?? "Không ghi sổ được biên nhận.");
            return;
        }

        _chungTuId = ketQua.Id;
        _view.DaGhiSo(ketQua.SoChungTu, ketQua.SoDuSau);
    }

    public async Task InAsync()
    {
        if (_chungTuId is not { } id)
        {
            _view.HienLoi("Chưa có chứng từ để in.");
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var query = scope.ServiceProvider.GetRequiredService<LayBienNhanThuDeInQuery>();
        var model = await query.LayAsync(id);
        if (model is null)
        {
            _view.HienLoi("Không tìm thấy chứng từ để in.");
            return;
        }

        var renderer = scope.ServiceProvider.GetRequiredService<IReportRenderer>();
        _view.HienThiBanIn(renderer.Render(model), $"BienNhanThu-{model.SoChungTu}");
    }
}
