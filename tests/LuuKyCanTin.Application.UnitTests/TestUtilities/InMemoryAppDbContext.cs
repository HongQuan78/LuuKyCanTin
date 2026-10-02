using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.DanhMuc;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.UnitTests.TestUtilities;

/// <summary>
/// An <see cref="IAppDbContext"/> on EF's in-memory provider. Good for service logic only: it has no SQL, no unique
/// indexes and no row versions, so those behaviours are covered by the integration tests.
/// </summary>
public sealed class InMemoryAppDbContext() : DbContext(
    new DbContextOptionsBuilder<InMemoryAppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options), IAppDbContext
{
    public DbSet<CanBo> CanBo => Set<CanBo>();

    /// <summary>Thrown by the next save instead of saving, to simulate what the real context translates.</summary>
    public Exception? LoiKhiLuu { get; set; }

    public int SoLanLuu { get; private set; }

    Task<int> IAppDbContext.SaveChangesAsync(CancellationToken ct)
    {
        SoLanLuu++;
        return LoiKhiLuu is { } loi ? Task.FromException<int>(loi) : SaveChangesAsync(ct);
    }

    Task<IAppTransaction> IAppDbContext.BeginTransactionAsync(CancellationToken ct) =>
        throw new NotSupportedException("The in-memory test context has no transactions.");
}
