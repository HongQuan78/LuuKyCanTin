using FluentValidation;
using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Application.DanhMuc;

public sealed class ThemDoiTuongValidator : AbstractValidator<ThemDoiTuongRequest>
{
    public ThemDoiTuongValidator(IClock clock)
    {
        RuleFor(x => x.MaSo)
            .NotEmpty().WithMessage("Mã số không được để trống.")
            .MaximumLength(30).WithMessage("Mã số tối đa 30 ký tự.");

        RuleFor(x => x.HoTen)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(100).WithMessage("Họ tên tối đa 100 ký tự.");

        RuleFor(x => x.NgayVao)
            .NotEmpty().WithMessage("Ngày vào không được để trống.")
            .Must(ngayVao => ngayVao <= clock.Today).WithMessage("Ngày vào không được lớn hơn ngày hiện tại.");

        RuleFor(x => x.BuongGiam)
            .MaximumLength(50).WithMessage("Buồng giam tối đa 50 ký tự.");

        RuleFor(x => x.LoaiDoiTuong)
            .IsInEnum().WithMessage("Loại đối tượng không hợp lệ.");
    }
}
