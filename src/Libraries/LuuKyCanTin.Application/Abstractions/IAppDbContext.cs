using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// The persistence operations Application services need. Application may reference EF Core (never a provider):
/// entity access goes through <see cref="DbSet{TEntity}"/> and the save translates provider exceptions into the
/// Application's own types. <see cref="BeginTransactionAsync"/> groups one posting into a single transaction.
/// </summary>
public interface IAppDbContext
{
    DbSet<CanBo> CanBo { get; }

    DbSet<VaiTro> VaiTro { get; }

    DbSet<Quyen> Quyen { get; }

    DbSet<VaiTroQuyen> VaiTroQuyen { get; }

    DbSet<NguoiDungVaiTro> NguoiDungVaiTro { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    /// <exception cref="XungDotDuLieuException">Someone else changed the row after it was loaded.</exception>
    /// <exception cref="TrungGiaTriDuyNhatException">A unique index rejected the save.</exception>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<IAppTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
