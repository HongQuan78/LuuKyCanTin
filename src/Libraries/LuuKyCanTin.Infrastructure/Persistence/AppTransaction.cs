using LuuKyCanTin.Application.Abstractions;
using Microsoft.EntityFrameworkCore.Storage;

namespace LuuKyCanTin.Infrastructure.Persistence;

internal sealed class AppTransaction(IDbContextTransaction transaction) : IAppTransaction
{
    public Task CommitAsync(CancellationToken ct = default) => transaction.CommitAsync(ct);

    public Task RollbackAsync(CancellationToken ct = default) => transaction.RollbackAsync(ct);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
