namespace LuuKyCanTin.Application.Administration;

/// <summary>Reads and edits the single unit-information row (Id = 1) that every printed template heads with.</summary>
public interface IFacilityInfoService
{
    Task<FacilityInfoDto> GetAsync(CancellationToken ct = default);

    /// <exception cref="LuuKyCanTin.Domain.Administration.PermissionDeniedException">The caller lacks <c>PermissionCodes.Administration.Update</c>.</exception>
    Task SaveAsync(SaveFacilityInfoRequest request, CancellationToken ct = default);
}
