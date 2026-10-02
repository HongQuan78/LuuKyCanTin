using LuuKyCanTin.Application.LuuKy;
using LuuKyCanTin.Domain.LuuKy;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.LuuKy;

/// <summary>
/// Adding only queues the entity; the posting service calls SaveChanges inside its transaction so the audit
/// interceptor sees the voucher. Never use ExecuteUpdate/raw SQL for vouchers, or the log loses them.
/// </summary>
internal sealed class ChungTuLuuKyStore(AppDbContext db) : IChungTuLuuKyStore
{
    public void Them(ChungTuLuuKy chungTu) => db.ChungTuLuuKy.Add(chungTu);

    public Task<ChungTuLuuKy?> TimTheoIdAsync(long id, CancellationToken ct = default) =>
        db.ChungTuLuuKy.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
}
