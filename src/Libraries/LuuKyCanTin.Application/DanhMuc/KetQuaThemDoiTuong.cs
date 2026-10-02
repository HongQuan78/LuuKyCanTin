namespace LuuKyCanTin.Application.DanhMuc;

public sealed record KetQuaThemDoiTuong(bool ThanhCong, int Id, string? ThongBao)
{
    public static KetQuaThemDoiTuong Ok(int id) => new(true, id, null);

    public static KetQuaThemDoiTuong Loi(string thongBao) => new(false, 0, thongBao);
}
