namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// The persistence operations Application services need, without an EF reference: save the unit of work and
/// start the one transaction that groups a posting. Entity access goes through narrow stores per aggregate.
/// </summary>
public interface IAppDbContext
{
    /// <summary>Writes every pending change. The save interceptor adds the audit rows in the same transaction.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<IAppTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
