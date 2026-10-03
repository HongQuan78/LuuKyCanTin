using FluentValidation;

namespace LuuKyCanTin.Application.Administration;

/// <summary>Checks the values as they will be stored, that is after trimming. Whitespace-only counts as empty.</summary>
public sealed class SaveFacilityInfoRequestValidator : AbstractValidator<SaveFacilityInfoRequest>
{
    public const int ParentAgencyNameMaxLength = 200;
    public const int FacilityNameMaxLength = 200;
    public const int AddressMaxLength = 300;

    public const string ParentAgencyNameMaxLengthMessage = "Cơ quan chủ quản tối đa 200 ký tự.";
    public const string FacilityNameRequiredMessage = "Tên đơn vị không được để trống.";
    public const string FacilityNameMaxLengthMessage = "Tên đơn vị tối đa 200 ký tự.";
    public const string AddressRequiredMessage = "Địa chỉ không được để trống.";
    public const string AddressMaxLengthMessage = "Địa chỉ tối đa 300 ký tự.";

    public SaveFacilityInfoRequestValidator()
    {
        RuleFor(r => (r.ParentAgencyName ?? "").Trim())
            .MaximumLength(ParentAgencyNameMaxLength).WithMessage(ParentAgencyNameMaxLengthMessage)
            .OverridePropertyName(nameof(SaveFacilityInfoRequest.ParentAgencyName));

        RuleFor(r => (r.FacilityName ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(FacilityNameRequiredMessage)
            .MaximumLength(FacilityNameMaxLength).WithMessage(FacilityNameMaxLengthMessage)
            .OverridePropertyName(nameof(SaveFacilityInfoRequest.FacilityName));

        RuleFor(r => (r.Address ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage(AddressRequiredMessage)
            .MaximumLength(AddressMaxLength).WithMessage(AddressMaxLengthMessage)
            .OverridePropertyName(nameof(SaveFacilityInfoRequest.Address));
    }
}
