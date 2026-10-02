namespace LuuKyCanTin.Application.LuuKy;

public sealed record KetQuaGhiSo(bool ThanhCong, long Id, string SoChungTu, decimal SoDuSau, string? ThongBao)
{
    public static KetQuaGhiSo Ok(long id, string soChungTu, decimal soDuSau) => new(true, id, soChungTu, soDuSau, null);

    public static KetQuaGhiSo Loi(string thongBao) => new(false, 0, "", 0, thongBao);
}
