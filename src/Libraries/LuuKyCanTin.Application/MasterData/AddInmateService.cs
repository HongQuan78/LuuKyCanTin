using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.MasterData;

namespace LuuKyCanTin.Application.MasterData;

/// <summary>
/// The walking skeleton's minimal "add a detainee". Search, edit and the type history are Epic 3.
/// </summary>
public sealed class AddInmateService(IInmateStore inmateStore, IClock clock)
{
    public const string DuplicateCodeMessage = "Mã số đã tồn tại.";

    private readonly AddInmateRequestValidator _validator = new(clock);

    public async Task<AddInmateResult> AddAsync(AddInmateRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return AddInmateResult.Fail(validation.Errors);

        var inmateCode = request.InmateCode.Trim();
        if (await inmateStore.CodeExistsAsync(inmateCode, ct))
            return AddInmateResult.Fail(DuplicateCodeMessage);

        var inmate = new Inmate
        {
            InmateCode = inmateCode,
            FullName = request.FullName.Trim(),
            BirthYear = request.BirthYear,
            InmateType = request.InmateType,
            AdmissionDate = request.AdmissionDate,
            Cell = string.IsNullOrWhiteSpace(request.Cell) ? null : request.Cell.Trim(),
            Status = InmateStatus.InCustody,
        };

        // The pre-check above is the friendly path; the store still maps a lost race to the same message.
        if (!await inmateStore.AddAsync(inmate, ct))
            return AddInmateResult.Fail(DuplicateCodeMessage);

        return AddInmateResult.Ok(inmate.Id);
    }
}
