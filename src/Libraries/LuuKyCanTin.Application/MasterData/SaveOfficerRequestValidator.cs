using FluentValidation;

namespace LuuKyCanTin.Application.MasterData;

/// <summary>Checks the values as they will be stored, that is after trimming.</summary>
public sealed class SaveOfficerRequestValidator : AbstractValidator<SaveOfficerRequest>
{
    public const int OfficerCodeMaxLength = 20;
    public const int FullNameMaxLength = 100;
    public const int PositionMaxLength = 100;

    public SaveOfficerRequestValidator()
    {
        // OfficerCode is varchar, so a letter with diacritics could not be stored faithfully.
        RuleFor(r => (r.OfficerCode ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Mã cán bộ không được để trống.")
            .MaximumLength(OfficerCodeMaxLength).WithMessage($"Mã cán bộ tối đa {OfficerCodeMaxLength} ký tự.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Mã cán bộ chỉ gồm chữ không dấu, chữ số, dấu '-' hoặc '_'.")
            .OverridePropertyName(nameof(SaveOfficerRequest.OfficerCode));

        RuleFor(r => (r.FullName ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(FullNameMaxLength).WithMessage($"Họ tên tối đa {FullNameMaxLength} ký tự.")
            .OverridePropertyName(nameof(SaveOfficerRequest.FullName));

        RuleFor(r => (r.Position ?? "").Trim())
            .MaximumLength(PositionMaxLength).WithMessage($"Chức vụ tối đa {PositionMaxLength} ký tự.")
            .OverridePropertyName(nameof(SaveOfficerRequest.Position));
    }
}
