namespace LuuKyCanTin.Domain.Common;

/// <summary>
/// Shared audit columns. They are filled automatically on save; the audit log, not these columns,
/// is the authoritative record of who changed what.
/// </summary>
public abstract class AuditableEntity
{
    public DateTime NgayTao { get; set; }
    public int NguoiTaoId { get; set; }
    public DateTime? NgaySua { get; set; }
    public int? NguoiSuaId { get; set; }

    /// <summary>Optimistic-concurrency token, set by the database on every write.</summary>
    public byte[] RowVer { get; set; } = [];
}
