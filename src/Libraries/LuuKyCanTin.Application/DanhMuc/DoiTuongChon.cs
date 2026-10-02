namespace LuuKyCanTin.Application.DanhMuc;

/// <summary>One row of the receipt form's detainee selector. The shared picker (UX-DR2) comes in Story 3.2.</summary>
public sealed record DoiTuongChon(int Id, string MaSo, string HoTen)
{
    public string HienThi => $"{MaSo} — {HoTen}";
}
