namespace LuuKyCanTin.Application.HeThong;

public sealed record KetQuaDangNhap(bool ThanhCong, string? ThongBao)
{
    public static KetQuaDangNhap Ok() => new(true, null);

    public static KetQuaDangNhap Loi(string thongBao) => new(false, thongBao);
}
