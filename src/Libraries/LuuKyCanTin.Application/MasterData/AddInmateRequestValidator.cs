using FluentValidation;
using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Application.MasterData;

public sealed class AddInmateRequestValidator : AbstractValidator<AddInmateRequest>
{
    public AddInmateRequestValidator(IClock clock)
    {
        RuleFor(x => x.InmateCode)
            .NotEmpty().WithMessage("Mã số không được để trống.")
            .MaximumLength(30).WithMessage("Mã số tối đa 30 ký tự.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự.");

        RuleFor(x => x.AdmissionDate)
            .NotEmpty().WithMessage("Ngày vào không được để trống.")
            .Must(admissionDate => admissionDate <= clock.Today).WithMessage("Ngày vào không được lớn hơn ngày hiện tại.");

        RuleFor(x => x.Cell)
            .MaximumLength(50).WithMessage("Buồng giam tối đa 50 ký tự.");

        RuleFor(x => x.InmateType)
            .IsInEnum().WithMessage("Loại đối tượng không hợp lệ.");
    }
}
