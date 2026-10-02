using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.HeThong;

/// <summary>The unit header printed on every template. Exactly one row exists (Id = 1).</summary>
public sealed class ThongTinDonVi : AuditableEntity
{
    public int Id { get; set; }

    public string? TenCoQuanChuQuan { get; set; }

    public string TenDonVi { get; set; } = "";

    public string DiaChi { get; set; } = "";
}
