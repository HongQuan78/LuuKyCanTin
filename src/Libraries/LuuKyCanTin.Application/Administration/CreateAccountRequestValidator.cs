using FluentValidation;

namespace LuuKyCanTin.Application.Administration;

/// <summary>Checks the values as they will be stored, that is after trimming.</summary>
public sealed class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public const int MinUserNameLength = 3;
    public const int MaxUserNameLength = 50;
    public const string OfficerRequiredMessage = "Phải chọn cán bộ.";
    public const string RoleRequiredMessage = "Phải chọn ít nhất một vai trò.";

    public CreateAccountRequestValidator()
    {
        RuleFor(r => (r.UserName ?? "").Trim())
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Tên đăng nhập không được để trống.")
            .MinimumLength(MinUserNameLength)
            .WithMessage($"Tên đăng nhập phải có ít nhất {MinUserNameLength} ký tự.")
            .MaximumLength(MaxUserNameLength)
            .WithMessage($"Tên đăng nhập tối đa {MaxUserNameLength} ký tự.")
            .Matches("^[A-Za-z0-9._]+$")
            .WithMessage("Tên đăng nhập chỉ gồm chữ không dấu, chữ số, dấu '.' hoặc '_'.")
            .OverridePropertyName(nameof(CreateAccountRequest.UserName));

        RuleFor(r => r.OfficerId)
            .GreaterThan(0)
            .WithMessage(OfficerRequiredMessage);

        RuleFor(r => r.RoleIds)
            .NotEmpty()
            .WithMessage(RoleRequiredMessage);
    }
}
