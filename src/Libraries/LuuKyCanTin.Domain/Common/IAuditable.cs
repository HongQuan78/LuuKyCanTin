namespace LuuKyCanTin.Domain.Common;

/// <summary>
/// Marks an entity whose every insert and change is written to the audit log (every voucher table). Separate from
/// <see cref="AuditableEntity"/>, which only carries the who/when columns.
/// </summary>
public interface IAuditable;
