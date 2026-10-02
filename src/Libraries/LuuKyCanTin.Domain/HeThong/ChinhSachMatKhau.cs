namespace LuuKyCanTin.Domain.HeThong;

/// <summary>
/// The password-strength rules (FR1): at least 8 characters with an upper-case letter, a lower-case letter and a
/// digit. A pure function with no dependencies, so the rules are unit-tested without a UI.
/// </summary>
public static class ChinhSachMatKhau
{
    public const int DoDaiToiThieu = 8;

    public const string LoiQuaNgan = "Mật khẩu phải có ít nhất 8 ký tự";
    public const string LoiThieuChuHoa = "Mật khẩu phải có ít nhất một chữ in hoa";
    public const string LoiThieuChuThuong = "Mật khẩu phải có ít nhất một chữ thường";
    public const string LoiThieuChuSo = "Mật khẩu phải có ít nhất một chữ số";
    public const string LoiTrungMatKhauCu = "Mật khẩu mới phải khác mật khẩu hiện tại";
    public const string LoiXacNhanKhongKhop = "Xác nhận mật khẩu không khớp";

    /// <summary>Returns every rule the new password breaks; an empty list means it is accepted.</summary>
    /// <remarks>Uses <see cref="char.IsUpper"/> and <see cref="char.IsLower"/>, so Đ and ă count as letters.</remarks>
    public static IReadOnlyList<string> KiemTra(string? matKhauMoi)
    {
        var viPham = new List<string>();
        if (matKhauMoi is null || matKhauMoi.Length < DoDaiToiThieu)
            viPham.Add(LoiQuaNgan);
        if (matKhauMoi is null || !matKhauMoi.Any(char.IsUpper))
            viPham.Add(LoiThieuChuHoa);
        if (matKhauMoi is null || !matKhauMoi.Any(char.IsLower))
            viPham.Add(LoiThieuChuThuong);
        if (matKhauMoi is null || !matKhauMoi.Any(char.IsDigit))
            viPham.Add(LoiThieuChuSo);

        return viPham;
    }
}
