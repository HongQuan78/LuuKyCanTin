using FluentValidation.Results;

namespace LuuKyCanTin.Application.Common;

/// <summary>
/// A request failed validation. The message lists every failure, one per line; <see cref="Errors"/> keeps the request
/// property of each, so a form can show it under the field it concerns.
/// </summary>
public sealed class RequestValidationException(IReadOnlyList<ValidationFailure> errors)
    : BusinessRuleException(string.Join('\n', errors.Select(e => e.ErrorMessage)))
{
    public IReadOnlyList<ValidationFailure> Errors { get; } = errors;
}
