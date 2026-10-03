using FluentValidation.Results;

namespace LuuKyCanTin.Application.Custody;

public sealed record PostingResult(bool Succeeded, long Id, string VoucherNumber, decimal BalanceAfter, string? Message)
{
    /// <summary>Every validation failure with the request property it concerns; empty when the failure is not validation.</summary>
    public IReadOnlyList<ValidationFailure> Errors { get; init; } = [];

    public static PostingResult Ok(long id, string voucherNumber, decimal balanceAfter) => new(true, id, voucherNumber, balanceAfter, null);

    public static PostingResult Fail(string message) => new(false, 0, "", 0, message);

    public static PostingResult Fail(IReadOnlyList<ValidationFailure> errors) =>
        new(false, 0, "", 0, errors[0].ErrorMessage) { Errors = errors };
}
