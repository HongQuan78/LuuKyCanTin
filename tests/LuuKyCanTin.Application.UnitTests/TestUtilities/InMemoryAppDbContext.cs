using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
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

    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();

    public DbSet<VaiTro> VaiTro => Set<VaiTro>();

    public DbSet<Quyen> Quyen => Set<Quyen>();

    public DbSet<VaiTroQuyen> VaiTroQuyen => Set<VaiTroQuyen>();

    public DbSet<NguoiDungVaiTro> NguoiDungVaiTro => Set<NguoiDungVaiTro>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // The join rows need their composite keys, and Quyen ids come from the catalogue, not the database.
        modelBuilder.Entity<VaiTroQuyen>().HasKey(v => new { v.VaiTroId, v.QuyenId });
        modelBuilder.Entity<NguoiDungVaiTro>().HasKey(v => new { v.NguoiDungId, v.VaiTroId });
        modelBuilder.Entity<Quyen>().Property(q => q.Id).ValueGeneratedNever();
    }

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

    Task<IAppTransaction> IAppDbContext.BeginTransactionAsync(MucDoCoLapGiaoDich mucDoCoLap, CancellationToken ct) =>
        throw new NotSupportedException("The in-memory test context has no transactions.");
}
