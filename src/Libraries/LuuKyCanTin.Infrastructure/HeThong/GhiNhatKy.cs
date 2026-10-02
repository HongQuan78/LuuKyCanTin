using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;

namespace LuuKyCanTin.Infrastructure.HeThong;

internal sealed class GhiNhatKy(AppDbContext db, NhatKyFactory nhatKyFactory) : IGhiNhatKy
{
    public Task GhiAsync(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieuMoi = null, CancellationToken ct = default) =>
        ThemVaLuuAsync(nhatKyFactory.Tao(hanhDong, tenBang, banGhiId, duLieuCu: null, duLieuMoi: duLieuMoi), ct);

    public Task GhiAsync(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieuCu, object? duLieuMoi, CancellationToken ct = default) =>
        ThemVaLuuAsync(nhatKyFactory.Tao(hanhDong, tenBang, banGhiId, duLieuCu, duLieuMoi), ct);

    private async Task ThemVaLuuAsync(NhatKyThaoTac nhatKy, CancellationToken ct)
    {
        db.NhatKyThaoTac.Add(nhatKy);
        await db.SaveChangesAsync(ct);
    }
}
