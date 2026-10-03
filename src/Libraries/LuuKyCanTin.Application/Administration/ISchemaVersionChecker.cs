namespace LuuKyCanTin.Application.Administration;

/// <summary>Compares the migrations this build expects with the ones applied to the database.</summary>
public interface ISchemaVersionChecker
{
    Task<SchemaVersionCheckResult> CheckAsync(CancellationToken ct = default);
}
