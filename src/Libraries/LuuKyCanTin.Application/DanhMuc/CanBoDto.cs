namespace LuuKyCanTin.Application.DanhMuc;

/// <param name="RowVer">Sent back with an edit, so a change someone else made in the meantime is detected.</param>
public sealed record CanBoDto(
    int Id,
    string MaCanBo,
    string HoTen,
    string? ChucVu,
    bool LaQuanGiao,
    bool DangCongTac,
    byte[] RowVer);
