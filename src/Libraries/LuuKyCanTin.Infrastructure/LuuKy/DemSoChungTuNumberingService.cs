using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.LuuKy;

/// <summary>
/// Allocates a number with one atomic UPDATE. UPDLOCK+ROWLOCK holds the counter row until the posting's
/// transaction ends, and because the raise is inside that transaction a rollback returns the number, leaving no gap.
/// </summary>
internal sealed class DemSoChungTuNumberingService(AppDbContext db) : INumberingService
{
    public async Task<string> CapSoAsync(string loaiChungTu, int nam, CancellationToken ct = default)
    {
        // Materialize before composing LINQ: EF would otherwise wrap the UPDATE in a sub-select, which SQL rejects.
        var dong = await db.Database.SqlQuery<CapSoRow>($"""
            UPDATE DemSoChungTu WITH (UPDLOCK, ROWLOCK)
            SET SoHienTai = SoHienTai + 1
            OUTPUT inserted.TienTo AS TienTo, inserted.SoHienTai AS SoHienTai
            WHERE LoaiChungTu = {loaiChungTu} AND Nam = {nam}
            """).ToListAsync(ct);

        if (dong.Count == 0)
            throw new InvalidOperationException($"Không có bộ đếm cho loại chứng từ {loaiChungTu} năm {nam}.");

        return $"{dong[0].TienTo}-{nam}-{dong[0].SoHienTai:D5}";
    }

    private sealed class CapSoRow
    {
        public string TienTo { get; set; } = "";

        public int SoHienTai { get; set; }
    }
}
