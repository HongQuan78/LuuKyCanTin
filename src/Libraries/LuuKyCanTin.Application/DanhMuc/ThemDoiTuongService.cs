using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.DanhMuc;

namespace LuuKyCanTin.Application.DanhMuc;

/// <summary>
/// The walking skeleton's minimal "add a detainee". Search, edit and the type history are Epic 3.
/// </summary>
public sealed class ThemDoiTuongService(IDoiTuongStore doiTuongStore, IClock clock)
{
    public const string MaSoDaTonTai = "Mã số đã tồn tại.";

    private readonly ThemDoiTuongValidator _validator = new(clock);

    public async Task<KetQuaThemDoiTuong> ThemAsync(ThemDoiTuongRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return KetQuaThemDoiTuong.Loi(validation.Errors[0].ErrorMessage);

        var maSo = request.MaSo.Trim();
        if (await doiTuongStore.MaSoDaTonTaiAsync(maSo, ct))
            return KetQuaThemDoiTuong.Loi(MaSoDaTonTai);

        var doiTuong = new DoiTuong
        {
            MaSo = maSo,
            HoTen = request.HoTen.Trim(),
            NamSinh = request.NamSinh,
            LoaiDoiTuong = request.LoaiDoiTuong,
            NgayVao = request.NgayVao,
            BuongGiam = string.IsNullOrWhiteSpace(request.BuongGiam) ? null : request.BuongGiam.Trim(),
            TrangThai = TrangThaiDoiTuong.DangQuanLy,
        };

        // The pre-check above is the friendly path; the store still maps a lost race to the same message.
        if (!await doiTuongStore.ThemAsync(doiTuong, ct))
            return KetQuaThemDoiTuong.Loi(MaSoDaTonTai);

        return KetQuaThemDoiTuong.Ok(doiTuong.Id);
    }
}
