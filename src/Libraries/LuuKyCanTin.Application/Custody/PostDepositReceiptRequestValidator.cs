using FluentValidation;
using LuuKyCanTin.Domain.Custody;

namespace LuuKyCanTin.Application.Custody;

public sealed class PostDepositReceiptRequestValidator : AbstractValidator<PostDepositReceiptRequest>
{
    public PostDepositReceiptRequestValidator()
    {
        RuleFor(x => x.InmateId)
            .GreaterThan(0).WithMessage("Phải chọn đối tượng.");

        RuleFor(x => x.VoucherDate)
            .NotEmpty().WithMessage("Ngày chứng từ không được để trống.");

        RuleFor(x => x.TransactionType)
            .IsInEnum().WithMessage("Nghiệp vụ không hợp lệ.")
            .Must(transactionType => (int)transactionType / 10 == (int)VoucherType.Receipt)
            .WithMessage("Chỉ ghi sổ được biên nhận thu.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Số tiền phải lớn hơn 0.")
            .Must(amount => amount == decimal.Truncate(amount)).WithMessage("Số tiền phải là số nguyên đồng.")
            .LessThanOrEqualTo(999_999_999_999_999_999m).WithMessage("Số tiền vượt quá giới hạn cho phép.");

        RuleFor(x => x.PaymentMethod)
            .IsInEnum().WithMessage("Hình thức không hợp lệ.");

        RuleFor(x => x.SenderFullName)
            .NotEmpty().WithMessage("Người gửi không được để trống.")
            .MaximumLength(100).WithMessage("Người gửi tối đa 100 ký tự.");

        RuleFor(x => x.Relationship)
            .MaximumLength(50).WithMessage("Quan hệ tối đa 50 ký tự.");

        RuleFor(x => x.SourceDocumentNumber)
            .MaximumLength(30).WithMessage("Số phiếu gốc tối đa 30 ký tự.");

        RuleFor(x => x.SenderAccountNumber)
            .Must((request, accountNumber) =>
                request.PaymentMethod != PaymentMethod.BankTransfer || !string.IsNullOrWhiteSpace(accountNumber))
            .WithMessage("Chuyển khoản phải có số tài khoản người gửi.")
            .MaximumLength(30).WithMessage("Số tài khoản tối đa 30 ký tự.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Nội dung tối đa 500 ký tự.");
    }
}
