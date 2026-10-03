using LuuKyCanTin.Domain.Custody;

namespace LuuKyCanTin.Application.Custody;

public static class PaymentMethodDisplayExtensions
{
    public static string ToDisplayText(this PaymentMethod value) => value switch
    {
        PaymentMethod.Cash => "Tiền mặt",
        PaymentMethod.BankTransfer => "Chuyển khoản",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown payment method."),
    };
}
