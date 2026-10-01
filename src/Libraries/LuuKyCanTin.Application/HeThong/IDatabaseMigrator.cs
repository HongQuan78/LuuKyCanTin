namespace LuuKyCanTin.Application.HeThong;

/// <summary>Applies pending migrations. Only the admin command calls this; workstations never migrate.</summary>
public interface IDatabaseMigrator
{
    /// <returns>The migrations applied by this call, oldest first; empty when the database was already current.</returns>
    Task<IReadOnlyList<string>> MigrateAsync(CancellationToken cancellationToken = default);
}
