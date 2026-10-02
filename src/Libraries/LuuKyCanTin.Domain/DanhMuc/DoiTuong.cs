using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.DanhMuc;

/// <summary>A detainee whose custodial balance the canteen and the ledger work against.</summary>
public sealed class DoiTuong : AuditableEntity
{
    public int Id { get; set; }

    public string MaSo { get; set; } = "";

    public string HoTen { get; set; } = "";

    public short? NamSinh { get; set; }

    public LoaiDoiTuong LoaiDoiTuong { get; set; }

    public DateOnly NgayVao { get; set; }

    public string? BuongGiam { get; set; }

    public TrangThaiDoiTuong TrangThai { get; set; } = TrangThaiDoiTuong.DangQuanLy;

    public DateOnly? NgayRa { get; set; }

    /// <summary>
    /// Only the ledger engine changes a balance, with a conditional UPDATE under a row lock. No C# code
    /// sets this, and EF is configured to never write the column, so a save can't change it by accident.
    /// </summary>
    public decimal SoDuLuuKy { get; private set; }
}
