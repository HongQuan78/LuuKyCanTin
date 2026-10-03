using FluentValidation;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The one engine that writes the unit header. It only ever updates the seeded row (Id = 1); the tracked save lets
/// the audit interceptor record the changed fields' before/after values, and the row version turns two
/// administrators editing at once into a conflict instead of a silent overwrite.
/// </summary>
public sealed class FacilityInfoService(
    IAppDbContext db,
    IPermissionChecker permissionChecker,
    IValidator<SaveFacilityInfoRequest> validator) : IFacilityInfoService
{
    // A missing row means the install is broken, not that the user did something wrong.
    internal const string MissingRowMessage =
        "The FacilityInfo row (Id = 1) is missing. The database was not seeded; restore the install.";

    public async Task<FacilityInfoDto> GetAsync(CancellationToken ct = default)
    {
        var facility = await LoadAsync(ct);
        return new FacilityInfoDto(
            facility.ParentAgencyName,
            facility.FacilityName,
            facility.Address,
            IsConfigured: !string.IsNullOrWhiteSpace(facility.FacilityName) && !string.IsNullOrWhiteSpace(facility.Address),
            facility.RowVer);
    }

    public async Task SaveAsync(SaveFacilityInfoRequest request, CancellationToken ct = default)
    {
        // Authorization first, before any read or write, so a refused call writes nothing at all.
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);

        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new RequestValidationException(result.Errors);

        var facility = await db.FacilityInfo.SingleOrDefaultAsync(f => f.Id == 1, ct)
            ?? throw new InvalidOperationException(MissingRowMessage);

        // The UPDATE then only succeeds while the row is still the version the user saw.
        db.Entry(facility).Property(f => f.RowVer).OriginalValue = request.RowVer;
        facility.ParentAgencyName = string.IsNullOrWhiteSpace(request.ParentAgencyName) ? null : request.ParentAgencyName.Trim();
        facility.FacilityName = request.FacilityName.Trim();
        facility.Address = request.Address.Trim();

        await db.SaveChangesAsync(ct);
    }

    private async Task<FacilityInfo> LoadAsync(CancellationToken ct) =>
        await db.FacilityInfo.AsNoTracking().SingleOrDefaultAsync(f => f.Id == 1, ct)
            ?? throw new InvalidOperationException(MissingRowMessage);
}
