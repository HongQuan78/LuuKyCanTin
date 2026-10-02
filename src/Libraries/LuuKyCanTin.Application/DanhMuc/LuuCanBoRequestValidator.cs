using FluentValidation;

namespace LuuKyCanTin.Application.DanhMuc;

/// <summary>Checks the values as they will be stored, that is after trimming.</summary>
public sealed class LuuCanBoRequestValidator : AbstractValidator<LuuCanBoRequest>
{
    public const int DoDaiMaCanBo = 20;
    public const int DoDaiHoTen = 100;
    public const int DoDaiChucVu = 100;

    public LuuCanBoRequestValidator()
    {
        // MaCanBo is varchar, so a letter with diacritics could not be stored faithfully.
        RuleFor(r => (r.MaCanBo ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Mã cán bộ không được để trống.")
            .MaximumLength(DoDaiMaCanBo).WithMessage($"Mã cán bộ tối đa {DoDaiMaCanBo} ký tự.")
            .Matches("^[A-Za-z0-9_-]+$").WithMessage("Mã cán bộ chỉ gồm chữ không dấu, chữ số, dấu '-' hoặc '_'.")
            .OverridePropertyName(nameof(LuuCanBoRequest.MaCanBo));

        RuleFor(r => (r.HoTen ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Họ tên không được để trống.")
            .MaximumLength(DoDaiHoTen).WithMessage($"Họ tên tối đa {DoDaiHoTen} ký tự.")
            .OverridePropertyName(nameof(LuuCanBoRequest.HoTen));

        RuleFor(r => (r.ChucVu ?? "").Trim())
            .MaximumLength(DoDaiChucVu).WithMessage($"Chức vụ tối đa {DoDaiChucVu} ký tự.")
            .OverridePropertyName(nameof(LuuCanBoRequest.ChucVu));
    }
}
