namespace LuuKyCanTin.Domain.Administration;

/// <summary>
/// One audit-log row. The log is append-only: rows are written once and never changed or deleted, which is why
/// this is not an <see cref="Common.AuditableEntity"/> and has no row version.
/// </summary>
public sealed class AuditLog
{
    public long Id { get; private set; }
    public DateTime OccurredAt { get; init; }

    /// <summary>Null for system and pre-sign-in events.</summary>
    public int? UserId { get; init; }

    public string? Workstation { get; init; }
    public AuditAction Action { get; init; }
    public string? TableName { get; init; }
    public long? RecordId { get; init; }

    /// <summary>JSON of the changed columns before the change; null when a record is added.</summary>
    public string? OldValues { get; init; }

    /// <summary>JSON of the changed columns after the change, or the payload of a business event.</summary>
    public string? NewValues { get; init; }
}
