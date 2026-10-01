namespace LuuKyCanTin.Application.HeThong;

/// <summary>Compares the migration this build expects with the one last applied to the database.</summary>
public interface ISchemaVersionChecker
{
    Task<SchemaVersionCheckResult> CheckAsync(CancellationToken cancellationToken = default);
}
