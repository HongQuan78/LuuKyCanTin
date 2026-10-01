using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;

namespace LuuKyCanTin.Infrastructure.HeThong;

internal sealed class GhiNhatKy(AppDbContext db, NhatKyFactory nhatKyFactory) : IGhiNhatKy
{
    public async Task GhiAsync(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieu = null, CancellationToken ct = default)
    {
        db.NhatKyThaoTac.Add(nhatKyFactory.Tao(hanhDong, tenBang, banGhiId, duLieuCu: null, duLieuMoi: duLieu));
        await db.SaveChangesAsync(ct);
    }
}
