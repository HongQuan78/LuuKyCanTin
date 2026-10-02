using LuuKyCanTin.Application.LuuKy;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.LuuKy;

/// <summary>
/// The only writer of DoiTuong.SoDuLuuKy. One conditional UPDATE under UPDLOCK+ROWLOCK both locks the row and
/// returns the before/after balances, so no read-then-write race can lose a deposit. DoiTuong.SoDuLuuKy is
/// ignored on every EF save, so no other code path can change it.
/// </summary>
internal sealed class SoDuLuuKyWriter(AppDbContext db) : ISoDuLuuKyWriter
{
    public async Task<(decimal SoDuTruoc, decimal SoDuSau)?> CongAsync(int doiTuongId, decimal soTien, CancellationToken ct = default)
    {
        // Materialize before composing LINQ: EF would otherwise wrap the UPDATE in a sub-select, which SQL rejects.
        var dong = await db.Database.SqlQuery<SoDuRow>($"""
            UPDATE DoiTuong WITH (UPDLOCK, ROWLOCK)
            SET SoDuLuuKy = SoDuLuuKy + {soTien}
            OUTPUT deleted.SoDuLuuKy AS SoDuTruoc, inserted.SoDuLuuKy AS SoDuSau
            WHERE Id = {doiTuongId} AND TrangThai = 1
            """).ToListAsync(ct);

        return dong.Count == 0 ? null : (dong[0].SoDuTruoc, dong[0].SoDuSau);
    }

    private sealed class SoDuRow
    {
        public decimal SoDuTruoc { get; set; }

        public decimal SoDuSau { get; set; }
    }
}
