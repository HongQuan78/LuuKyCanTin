namespace LuuKyCanTin.Application.DanhMuc;

/// <param name="RowVer">Required for an edit: the version the user loaded. Ignored when adding.</param>
public sealed record LuuCanBoRequest(
    string MaCanBo,
    string HoTen,
    string? ChucVu,
    bool LaQuanGiao,
    bool DangCongTac = true,
    byte[]? RowVer = null);
