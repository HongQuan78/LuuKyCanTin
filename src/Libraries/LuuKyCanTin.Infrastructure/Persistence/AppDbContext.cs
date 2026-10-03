using System.Data;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public const string Collation = "Vietnamese_CI_AI";

    /// <summary>
    /// For name columns searched by typing without diacritics. Vietnamese_CI_AI ignores only tone marks: it treats
    /// ă, â, ê, ô, ơ, ư and đ as separate letters, so "van" would never find "Văn". This one folds them all.
    /// </summary>
    public const string SearchCollation = "Latin1_General_100_CI_AI";

    // SQL Server's "Cannot insert duplicate key": 2601 for a unique index, 2627 for a unique constraint.
    private static readonly HashSet<int> DuplicateKeyErrors = [2601, 2627];

    public DbSet<AuditLog> AuditLog => Set<AuditLog>();

    public DbSet<User> User => Set<User>();

    public DbSet<FacilityInfo> FacilityInfo => Set<FacilityInfo>();

    public DbSet<Inmate> Inmate => Set<Inmate>();

    public DbSet<CustodyVoucher> CustodyVoucher => Set<CustodyVoucher>();

    public DbSet<VoucherCounter> VoucherCounter => Set<VoucherCounter>();

    public DbSet<Officer> Officer => Set<Officer>();

    public DbSet<Role> Role => Set<Role>();

    public DbSet<Permission> Permission => Set<Permission>();

    public DbSet<RolePermission> RolePermission => Set<RolePermission>();

    public DbSet<UserRole> UserRole => Set<UserRole>();

    public bool HasActiveTransaction => Database.CurrentTransaction is not null;

    // Application sees only its own exception types; Infrastructure code calling the context directly keeps EF's.
    async Task<int> IAppDbContext.SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            return await SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyConflictException(ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException sql && DuplicateKeyErrors.Contains(sql.Number))
        {
            throw new UniqueConstraintException(ex);
        }
    }

    async Task<IAppTransaction> IAppDbContext.BeginTransactionAsync(CancellationToken ct)
        => new AppTransaction(await Database.BeginTransactionAsync(ct));

    async Task<IAppTransaction> IAppDbContext.BeginTransactionAsync(TransactionIsolation isolation, CancellationToken ct)
        => new AppTransaction(await Database.BeginTransactionAsync(ToIsolationLevel(isolation), ct));

    private static IsolationLevel ToIsolationLevel(TransactionIsolation isolation) => isolation switch
    {
        TransactionIsolation.Serializable => IsolationLevel.Serializable,
        _ => IsolationLevel.ReadCommitted,
    };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Must stay in the first migration: a database collation cannot change once columns depend on it.
        modelBuilder.UseCollation(Collation);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // Money is whole đồng; quantities and average costs override the scale per property.
        configurationBuilder.Properties<decimal>().HavePrecision(18, 0);
        configurationBuilder.Properties<DateOnly>().HaveColumnType("date");
        configurationBuilder.Properties<DateTime>().HavePrecision(0);
        configurationBuilder.Properties<string>().AreUnicode();
        // Enums are declared ': byte', which EF already maps to tinyint; every enum column also needs HasEnumCheck.
    }
}
