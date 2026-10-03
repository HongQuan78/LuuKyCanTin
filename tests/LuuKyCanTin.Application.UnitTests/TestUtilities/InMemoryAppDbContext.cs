using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.UnitTests.TestUtilities;

/// <summary>
/// An <see cref="IAppDbContext"/> on EF's in-memory provider. Good for service logic only: it has no SQL, no unique
/// indexes and no row versions, so those behaviours are covered by the integration tests.
/// </summary>
public sealed class InMemoryAppDbContext() : DbContext(
    new DbContextOptionsBuilder<InMemoryAppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options), IAppDbContext
{
    public DbSet<Officer> Officer => Set<Officer>();

    public DbSet<Role> Role => Set<Role>();

    public DbSet<Permission> Permission => Set<Permission>();

    public DbSet<RolePermission> RolePermission => Set<RolePermission>();

    public DbSet<UserRole> UserRole => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The join rows need their composite keys, and Permission ids come from the catalogue, not the database.
        modelBuilder.Entity<RolePermission>().HasKey(v => new { v.RoleId, v.PermissionId });
        modelBuilder.Entity<UserRole>().HasKey(v => new { v.UserId, v.RoleId });
        modelBuilder.Entity<Permission>().Property(q => q.Id).ValueGeneratedNever();
    }

    /// <summary>Thrown by the next save instead of saving, to simulate what the real context translates.</summary>
    public Exception? SaveFailure { get; set; }

    public int SaveCount { get; private set; }

    Task<int> IAppDbContext.SaveChangesAsync(CancellationToken ct)
    {
        SaveCount++;
        return SaveFailure is { } error ? Task.FromException<int>(error) : SaveChangesAsync(ct);
    }

    Task<IAppTransaction> IAppDbContext.BeginTransactionAsync(CancellationToken ct) =>
        throw new NotSupportedException("The in-memory test context has no transactions.");
}
