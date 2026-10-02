using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.DanhMuc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LuuKyCanTin.Application.Abstractions;

/// <summary>The unit of work a use-case service writes through. One per DI scope, so one per operation.</summary>
public interface IAppDbContext
{
    DbSet<CanBo> CanBo { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    /// <exception cref="XungDotDuLieuException">Someone else changed the row after it was loaded.</exception>
    /// <exception cref="TrungGiaTriDuyNhatException">A unique index rejected the save.</exception>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
