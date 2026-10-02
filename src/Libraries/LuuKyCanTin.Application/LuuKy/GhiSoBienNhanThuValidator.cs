using FluentValidation;
using LuuKyCanTin.Domain.LuuKy;

namespace LuuKyCanTin.Application.LuuKy;

public sealed class GhiSoBienNhanThuValidator : AbstractValidator<GhiSoBienNhanThuRequest>
{
    public GhiSoBienNhanThuValidator()
    {
        RuleFor(x => x.DoiTuongId)
            .GreaterThan(0).WithMessage("Phải chọn đối tượng.");

        RuleFor(x => x.NgayChungTu)
            .NotEmpty().WithMessage("Ngày chứng từ không được để trống.");

        RuleFor(x => x.NghiepVu)
            .IsInEnum().WithMessage("Nghiệp vụ không hợp lệ.")
            .Must(nghiepVu => (int)nghiepVu / 10 == (int)LoaiPhieu.Thu)
            .WithMessage("Chỉ ghi sổ được biên nhận thu.");

        RuleFor(x => x.SoTien)
            .GreaterThan(0).WithMessage("Số tiền phải lớn hơn 0.")
            .Must(soTien => soTien == decimal.Truncate(soTien)).WithMessage("Số tiền phải là số nguyên đồng.")
            .LessThanOrEqualTo(999_999_999_999_999_999m).WithMessage("Số tiền vượt quá giới hạn cho phép.");

        RuleFor(x => x.HinhThuc)
            .IsInEnum().WithMessage("Hình thức không hợp lệ.");

        RuleFor(x => x.NguoiGuiHoTen)
            .NotEmpty().WithMessage("Người gửi không được để trống.")
            .MaximumLength(100).WithMessage("Người gửi tối đa 100 ký tự.");

        RuleFor(x => x.QuanHe)
            .MaximumLength(50).WithMessage("Quan hệ tối đa 50 ký tự.");

        RuleFor(x => x.SoPhieuGoc)
            .MaximumLength(30).WithMessage("Số phiếu gốc tối đa 30 ký tự.");

        RuleFor(x => x.SoTaiKhoanNguoiGui)
            .Must((request, soTaiKhoan) =>
                request.HinhThuc != HinhThuc.ChuyenKhoan || !string.IsNullOrWhiteSpace(soTaiKhoan))
            .WithMessage("Chuyển khoản phải có số tài khoản người gửi.")
            .MaximumLength(30).WithMessage("Số tài khoản tối đa 30 ký tự.");

        RuleFor(x => x.NoiDung)
            .MaximumLength(500).WithMessage("Nội dung tối đa 500 ký tự.");
    }
}
