using FluentValidation.Results;

namespace LuuKyCanTin.Application.MasterData;

public sealed record AddInmateResult(bool Succeeded, int Id, string? Message)
{
    /// <summary>Every validation failure with the request property it concerns; empty when the failure is not validation.</summary>
    public IReadOnlyList<ValidationFailure> Errors { get; init; } = [];

    public static AddInmateResult Ok(int id) => new(true, id, null);

    public static AddInmateResult Fail(string message) => new(false, 0, message);

    public static AddInmateResult Fail(IReadOnlyList<ValidationFailure> errors) =>
        new(false, 0, errors[0].ErrorMessage) { Errors = errors };
}
