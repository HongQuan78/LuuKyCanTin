using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.DanhMuc;

/// <summary>
/// A staff member. <see cref="ChucVu"/> is the business role as free text; system permissions come from the user
/// account's roles, never from this record.
/// </summary>
/// <remarks>
/// Staff are never deleted: someone who has left gets <see cref="DangCongTac"/> = false, which hides them from
/// selection lists while historical documents keep pointing at them.
/// </remarks>
public sealed class CanBo : AuditableEntity, IAuditable
{
    public CanBo(string maCanBo, string hoTen, string? chucVu, bool laQuanGiao)
    {
        CapNhat(maCanBo, hoTen, chucVu, laQuanGiao);
    }

    private CanBo()
    {
    }

    public int Id { get; private set; }

    /// <summary>Upper-case, so a code typed in either case finds the same person.</summary>
    public string MaCanBo { get; private set; } = "";

    public string HoTen { get; private set; } = "";

    public string? ChucVu { get; private set; }

    public bool LaQuanGiao { get; private set; }

    public bool DangCongTac { get; set; } = true;

    public void CapNhat(string maCanBo, string hoTen, string? chucVu, bool laQuanGiao)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(maCanBo);
        ArgumentException.ThrowIfNullOrWhiteSpace(hoTen);

        MaCanBo = maCanBo.Trim().ToUpperInvariant();
        HoTen = hoTen.Trim();
        ChucVu = string.IsNullOrWhiteSpace(chucVu) ? null : chucVu.Trim();
        LaQuanGiao = laQuanGiao;
    }
}
