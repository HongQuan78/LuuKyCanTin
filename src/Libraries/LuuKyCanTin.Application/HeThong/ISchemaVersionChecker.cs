namespace LuuKyCanTin.Application.HeThong;

/// <summary>Compares the migrations this build expects with the ones applied to the database.</summary>
public interface ISchemaVersionChecker
{
    Task<SchemaVersionCheckResult> KiemTraAsync(CancellationToken ct = default);
}
