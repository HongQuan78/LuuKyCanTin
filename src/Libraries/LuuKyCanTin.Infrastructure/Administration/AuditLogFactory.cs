using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Infrastructure.Administration;

/// <summary>Builds audit-log rows, so the save interceptor and explicit events stamp them identically.</summary>
public sealed class AuditLogFactory(IClock clock, ICurrentUser currentUser)
{
    // Relaxed escaping keeps Vietnamese readable on the audit screen instead of \uXXXX sequences, and enums are
    // written by name ("Cancelled", not 3).
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    public AuditLog Create(AuditAction action, string? tableName, long? recordId, object? oldValues, object? newValues) => new()
    {
        OccurredAt = clock.Now,
        UserId = currentUser.UserId,
        Workstation = Environment.MachineName,
        Action = action,
        TableName = tableName,
        RecordId = recordId,
        OldValues = ToJson(oldValues),
        NewValues = ToJson(newValues),
    };

    private static string? ToJson(object? value) => value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
}
