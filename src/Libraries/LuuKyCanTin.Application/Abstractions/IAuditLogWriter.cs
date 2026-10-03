using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// Writes an audit-log row for a business event that changes no voucher row (sign-in, print, approval).
/// Voucher changes are logged automatically on save; don't log them again here.
/// </summary>
public interface IAuditLogWriter
{
    /// <param name="newValues">Serialized to JSON as the row's <c>NewValues</c>. Never pass secrets.</param>
    /// <remarks>Saves the current unit of work, so any pending changes in it are saved too.</remarks>
    Task WriteAsync(AuditAction action, string? tableName, long? recordId, object? newValues = null, CancellationToken ct = default);

    /// <summary>Logs a change with its before and after values, for join tables the interceptor can't follow.</summary>
    Task WriteAsync(AuditAction action, string? tableName, long? recordId, object? oldValues, object? newValues, CancellationToken ct = default);
}
