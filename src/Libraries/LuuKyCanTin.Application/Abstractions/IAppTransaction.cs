namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// The single database transaction a posting operation commits or rolls back as a unit. Obtained from
/// <see cref="IAppDbContext.BeginTransactionAsync"/>.
/// </summary>
public interface IAppTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct = default);

    Task RollbackAsync(CancellationToken ct = default);
}
