using LuuKyCanTin.Domain.DanhMuc;

namespace LuuKyCanTin.Application.DanhMuc;

public sealed record ThemDoiTuongRequest(
    string MaSo,
    string HoTen,
    short? NamSinh,
    LoaiDoiTuong LoaiDoiTuong,
    DateOnly NgayVao,
    string? BuongGiam);
