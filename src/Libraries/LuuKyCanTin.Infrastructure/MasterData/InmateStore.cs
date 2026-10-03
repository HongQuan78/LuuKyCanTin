using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.MasterData;

internal sealed class InmateStore(AppDbContext db) : IInmateStore
{
    public Task<Inmate?> FindByIdAsync(int id, CancellationToken ct = default) =>
        db.Inmate.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task<IReadOnlyList<Inmate>> GetInCustodyAsync(CancellationToken ct = default) =>
        await db.Inmate.AsNoTracking()
            .Where(d => d.Status == InmateStatus.InCustody)
            .OrderBy(d => d.InmateCode)
            .ToListAsync(ct);

    public Task<bool> CodeExistsAsync(string inmateCode, CancellationToken ct = default) =>
        db.Inmate.AnyAsync(d => d.InmateCode == inmateCode, ct);

    public async Task<bool> AddAsync(Inmate inmate, CancellationToken ct = default)
    {
        db.Inmate.Add(inmate);
        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // The pre-check lost a race. Detach so the failed insert can't leak into a later save of this scope.
            db.Entry(inmate).State = EntityState.Detached;
            return false;
        }
    }

    // SQL Server unique-index/constraint violations: duplicate key (2601) and unique constraint (2627).
    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 };
}
