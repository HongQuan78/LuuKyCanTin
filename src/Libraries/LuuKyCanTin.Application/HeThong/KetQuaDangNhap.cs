namespace LuuKyCanTin.Application.HeThong;

/// <summary>Why a sign-in ended the way it did. Only <see cref="ThanhCong"/> opens the shell directly.</summary>
public enum TrangThaiDangNhap
{
    ThanhCong = 1,
    PhaiDoiMatKhau = 2,
    SaiThongTin = 3,
    TaiKhoanBiKhoa = 4,
    TaiKhoanNgungHoatDong = 5,
}

public sealed record KetQuaDangNhap(TrangThaiDangNhap TrangThai, string? ThongBao)
{
    /// <summary>True when the credentials were accepted, whether or not a change is forced first.</summary>
    public bool ThanhCong => TrangThai is TrangThaiDangNhap.ThanhCong or TrangThaiDangNhap.PhaiDoiMatKhau;

    public bool PhaiDoiMatKhau => TrangThai == TrangThaiDangNhap.PhaiDoiMatKhau;

    public static KetQuaDangNhap Ok() => new(TrangThaiDangNhap.ThanhCong, null);

    public static KetQuaDangNhap DoiMatKhau() => new(TrangThaiDangNhap.PhaiDoiMatKhau, null);

    public static KetQuaDangNhap Loi(TrangThaiDangNhap trangThai, string thongBao) => new(trangThai, thongBao);
}
