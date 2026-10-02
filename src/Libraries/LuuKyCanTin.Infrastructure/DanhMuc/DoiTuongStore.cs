using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.DanhMuc;

internal sealed class DoiTuongStore(AppDbContext db) : IDoiTuongStore
{
    public Task<DoiTuong?> TimTheoIdAsync(int id, CancellationToken ct = default) =>
        db.DoiTuong.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<DoiTuong>> LayDangQuanLyAsync(CancellationToken ct = default) =>
        await db.DoiTuong.AsNoTracking()
            .Where(d => d.TrangThai == TrangThaiDoiTuong.DangQuanLy)
            .OrderBy(d => d.MaSo)
            .ToListAsync(ct);

    public Task<bool> MaSoDaTonTaiAsync(string maSo, CancellationToken ct = default) =>
        db.DoiTuong.AnyAsync(d => d.MaSo == maSo, ct);

    public async Task<bool> ThemAsync(DoiTuong doiTuong, CancellationToken ct = default)
    {
        db.DoiTuong.Add(doiTuong);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (LaViPhamDuyNhat(ex))
        {
            // The pre-check lost a race. Detach so the failed insert can't leak into a later save of this scope.
            db.Entry(doiTuong).State = EntityState.Detached;
            return false;
        }
    }

    // SQL Server unique-index/constraint violations: duplicate key (2601) and unique constraint (2627).
    private static bool LaViPhamDuyNhat(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
