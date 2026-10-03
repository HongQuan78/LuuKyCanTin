using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Administration;

internal sealed class FacilityInfoStore(AppDbContext db) : IFacilityInfoStore
{
    public Task<FacilityInfo?> GetAsync(CancellationToken ct = default) =>
        db.FacilityInfo.AsNoTracking().FirstOrDefaultAsync(u => u.Id == 1, ct);
}
