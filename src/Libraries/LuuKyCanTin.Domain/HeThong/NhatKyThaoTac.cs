namespace LuuKyCanTin.Domain.HeThong;

/// <summary>
/// One audit-log row. The log is append-only: rows are written once and never changed or deleted, which is why
/// this is not an <see cref="Common.AuditableEntity"/> and has no row version.
/// </summary>
public sealed class NhatKyThaoTac
{
    public long Id { get; private set; }
    public DateTime ThoiDiem { get; init; }

    /// <summary>Null for system and pre-sign-in events.</summary>
    public int? NguoiDungId { get; init; }

    public string? MayTram { get; init; }
    public HanhDong HanhDong { get; init; }
    public string? TenBang { get; init; }
    public long? BanGhiId { get; init; }

    /// <summary>JSON of the changed columns before the change; null when a record is added.</summary>
    public string? DuLieuCu { get; init; }

    /// <summary>JSON of the changed columns after the change, or the payload of a business event.</summary>
    public string? DuLieuMoi { get; init; }
}
