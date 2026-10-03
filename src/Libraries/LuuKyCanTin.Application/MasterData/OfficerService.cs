using System.Linq.Expressions;
using FluentValidation;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.MasterData;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.MasterData;

public sealed class OfficerService(IAppDbContext db, IValidator<SaveOfficerRequest> validator) : IOfficerService
{
    public const string DuplicateCodeMessage = "Mã cán bộ đã tồn tại";
    public const string NotFoundMessage = "Không tìm thấy cán bộ.";

    private static readonly Expression<Func<Officer, OfficerDto>> ToDtoExpression =
        c => new OfficerDto(c.Id, c.OfficerCode, c.FullName, c.Position, c.IsSupervisingOfficer, c.IsActive, c.RowVer);

    private static readonly Func<Officer, OfficerDto> ToDto = ToDtoExpression.Compile();

    public async Task<OfficerDto> AddAsync(SaveOfficerRequest request, CancellationToken ct = default)
    {
        // TODO: require permission PermissionCodes.MasterData.Create once IPermissionChecker exists.
        await ValidateAsync(request, ct);
        var officer = new Officer(request.OfficerCode, request.FullName, request.Position, request.IsSupervisingOfficer)
        {
            IsActive = request.IsActive,
        };
        await EnsureCodeIsUniqueAsync(officer.OfficerCode, editedId: null, ct);

        db.Officer.Add(officer);
        await SaveAsync(ct);
        return ToDto(officer);
    }

    public async Task<OfficerDto> UpdateAsync(int id, SaveOfficerRequest request, CancellationToken ct = default)
    {
        // TODO: require permission PermissionCodes.MasterData.Update once IPermissionChecker exists.
        if (request.RowVer is null)
            throw new ArgumentException("An edit must carry the row version the user loaded.", nameof(request));
        await ValidateAsync(request, ct);

        var officer = await db.Officer.SingleOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new BusinessRuleException(NotFoundMessage);
        // The UPDATE then only succeeds while the row is still the version the user saw.
        db.Entry(officer).Property(c => c.RowVer).OriginalValue = request.RowVer;
        officer.Update(request.OfficerCode, request.FullName, request.Position, request.IsSupervisingOfficer);
        officer.IsActive = request.IsActive;
        await EnsureCodeIsUniqueAsync(officer.OfficerCode, editedId: id, ct);

        await SaveAsync(ct);
        return ToDto(officer);
    }

    public async Task<IReadOnlyList<OfficerDto>> SearchAsync(string? keyword, bool includeInactive, CancellationToken ct = default)
    {
        var query = db.Officer.AsNoTracking();
        if (!includeInactive)
            query = query.Where(c => c.IsActive);
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            // Two variables give two SQL parameters. A shared one would be typed varchar after OfficerCode and lose the
            // diacritics of a keyword meant for the nvarchar FullName.
            var byCode = keyword.Trim();
            var byName = byCode;
            query = query.Where(c => c.OfficerCode.Contains(byCode) || c.FullName.Contains(byName));
        }

        return await SortForDisplay(query).Select(ToDtoExpression).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OfficerDto>> GetActiveOfficersAsync(bool supervisingOnly, CancellationToken ct = default)
    {
        var query = db.Officer.AsNoTracking().Where(c => c.IsActive);
        if (supervisingOnly)
            query = query.Where(c => c.IsSupervisingOfficer);

        return await SortForDisplay(query).Select(ToDtoExpression).ToListAsync(ct);
    }

    private static IQueryable<Officer> SortForDisplay(IQueryable<Officer> query) => query.OrderBy(c => c.FullName).ThenBy(c => c.OfficerCode);

    private async Task ValidateAsync(SaveOfficerRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new BusinessRuleException(string.Join('\n', result.Errors.Select(e => e.ErrorMessage)));
    }

    // The unique index is the backstop when two workstations save the same code at once.
    private async Task EnsureCodeIsUniqueAsync(string officerCode, int? editedId, CancellationToken ct)
    {
        if (await db.Officer.AnyAsync(c => c.OfficerCode == officerCode && c.Id != editedId, ct))
            throw new BusinessRuleException(DuplicateCodeMessage);
    }

    private async Task SaveAsync(CancellationToken ct)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException ex)
        {
            throw new BusinessRuleException(DuplicateCodeMessage, ex);
        }
    }
}
