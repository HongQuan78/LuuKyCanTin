using LuuKyCanTin.Domain.DanhMuc;

namespace LuuKyCanTin.WinForms.DanhMuc;

public interface IThemDoiTuongView
{
    event EventHandler? LuuBam;

    string MaSo { get; }

    string HoTen { get; }

    short? NamSinh { get; }

    LoaiDoiTuong LoaiDoiTuong { get; }

    DateOnly NgayVao { get; }

    string? BuongGiam { get; }

    void HienLoi(string thongBao);

    void DongVoiKetQua(bool thanhCong);
}
