using FluentValidation;

namespace LuuKyCanTin.Application.HeThong;

/// <summary>Checks the values as they will be stored, that is after trimming.</summary>
public sealed class TaoTaiKhoanRequestValidator : AbstractValidator<TaoTaiKhoanRequest>
{
    public const int DoDaiTenDangNhapToiThieu = 3;
    public const int DoDaiTenDangNhapToiDa = 50;

    public TaoTaiKhoanRequestValidator()
    {
        RuleFor(r => (r.TenDangNhap ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
            .MinimumLength(DoDaiTenDangNhapToiThieu)
            .WithMessage($"Tên đăng nhập phải có ít nhất {DoDaiTenDangNhapToiThieu} ký tự.")
            .MaximumLength(DoDaiTenDangNhapToiDa)
            .WithMessage($"Tên đăng nhập tối đa {DoDaiTenDangNhapToiDa} ký tự.")
            .Matches("^[A-Za-z0-9._]+$")
            .WithMessage("Tên đăng nhập chỉ gồm chữ không dấu, chữ số, dấu '.' hoặc '_'.")
            .OverridePropertyName(nameof(TaoTaiKhoanRequest.TenDangNhap));

        RuleFor(r => r.CanBoId)
            .GreaterThan(0)
            .WithMessage("Phải chọn cán bộ.");

        RuleFor(r => r.VaiTroIds)
            .NotEmpty()
            .WithMessage("Phải chọn ít nhất một vai trò.");
    }
}
