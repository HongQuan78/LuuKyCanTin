using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
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
    DbSet<User> User { get; }

    DbSet<FacilityInfo> FacilityInfo { get; }

    DbSet<SignatoryConfiguration> SignatoryConfiguration { get; }

    DbSet<Officer> Officer { get; }

    DbSet<Role> Role { get; }

    DbSet<Permission> Permission { get; }

    DbSet<RolePermission> RolePermission { get; }

    DbSet<UserRole> UserRole { get; }

    EntityEntry<TEntity> Entry<TEntity>(TEntity entity)
        where TEntity : class;

    /// <exception cref="ConcurrencyConflictException">Someone else changed the row after it was loaded.</exception>
    /// <exception cref="UniqueConstraintException">A unique index rejected the save.</exception>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task<IAppTransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>Opens a transaction at the requested isolation, for a guarded read-check-write.</summary>
    Task<IAppTransaction> BeginTransactionAsync(TransactionIsolation isolation, CancellationToken ct = default);

    /// <summary>True while a transaction started by <see cref="BeginTransactionAsync(CancellationToken)"/> is open.</summary>
    bool HasActiveTransaction { get; }
}
