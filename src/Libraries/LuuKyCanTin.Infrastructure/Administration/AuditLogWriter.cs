using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence;

namespace LuuKyCanTin.Infrastructure.Administration;

internal sealed class AuditLogWriter(AppDbContext db, AuditLogFactory auditLogFactory) : IAuditLogWriter
{
    public Task WriteAsync(AuditAction action, string? tableName, long? recordId, object? newValues = null, CancellationToken ct = default) =>
        AddAndSaveAsync(auditLogFactory.Create(action, tableName, recordId, oldValues: null, newValues: newValues), ct);

    public Task WriteAsync(AuditAction action, string? tableName, long? recordId, object? oldValues, object? newValues, CancellationToken ct = default) =>
        AddAndSaveAsync(auditLogFactory.Create(action, tableName, recordId, oldValues, newValues), ct);

    private async Task AddAndSaveAsync(AuditLog auditLog, CancellationToken ct)
    {
        db.AuditLog.Add(auditLog);
        await db.SaveChangesAsync(ct);
    }
}
