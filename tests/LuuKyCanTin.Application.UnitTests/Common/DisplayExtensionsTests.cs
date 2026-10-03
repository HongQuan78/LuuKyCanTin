using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Common;

/// <summary>The Vietnamese shown for each enum value, kept identical to the text the forms and receipt printed before.</summary>
public sealed class DisplayExtensionsTests
{
    [Theory]
    [InlineData(InmateType.PreTrialDetainee, "Tạm giữ/tạm giam")]
    [InlineData(InmateType.Prisoner, "Phạm nhân")]
    public void InmateTypeToDisplayText_EveryValue_IsTheVietnameseLabel(InmateType value, string expected)
    {
        value.ToDisplayText().ShouldBe(expected);
    }

    [Theory]
    [InlineData(PaymentMethod.Cash, "Tiền mặt")]
    [InlineData(PaymentMethod.BankTransfer, "Chuyển khoản")]
    public void PaymentMethodToDisplayText_EveryValue_IsTheVietnameseLabel(PaymentMethod value, string expected)
    {
        value.ToDisplayText().ShouldBe(expected);
    }

    [Theory]
    [InlineData(TransactionType.BroughtOnAdmission, "Mang theo khi vào")]
    [InlineData(TransactionType.SentByRelative, "Người thân gửi")]
    [InlineData(TransactionType.GiftSlip, "Phiếu gửi quà")]
    [InlineData(TransactionType.ReceivedFromOtherInmate, "Nhận từ đối tượng khác")]
    [InlineData(TransactionType.CanteenPurchase, "Mua hàng")]
    [InlineData(TransactionType.GivenToOtherInmate, "Cho tiền")]
    [InlineData(TransactionType.ReturnedToRelative, "Chuyển về người thân")]
    [InlineData(TransactionType.FacilityTransfer, "Chuyển trại")]
    [InlineData(TransactionType.SentenceCompleted, "Chấp hành xong án")]
    public void TransactionTypeToDisplayText_EveryValue_IsTheVietnameseLabel(TransactionType value, string expected)
    {
        value.ToDisplayText().ShouldBe(expected);
    }

    [Theory]
    [InlineData(PasswordRule.MinLength, "Ít nhất 8 ký tự")]
    [InlineData(PasswordRule.UpperCase, "Có chữ hoa")]
    [InlineData(PasswordRule.LowerCase, "Có chữ thường")]
    [InlineData(PasswordRule.Digit, "Có chữ số")]
    [InlineData(PasswordRule.DifferentFromCurrent, "Khác mật khẩu hiện tại")]
    public void PasswordRuleToDisplayText_EveryValue_IsTheChecklistLabel(PasswordRule value, string expected)
    {
        value.ToDisplayText().ShouldBe(expected);
    }

    [Fact]
    public void ToDisplayText_EveryDefinedValue_HasText()
    {
        Enum.GetValues<InmateType>().ShouldAllBe(v => v.ToDisplayText().Length > 0);
        Enum.GetValues<PaymentMethod>().ShouldAllBe(v => v.ToDisplayText().Length > 0);
        Enum.GetValues<TransactionType>().ShouldAllBe(v => v.ToDisplayText().Length > 0);
        Enum.GetValues<PasswordRule>().ShouldAllBe(v => v.ToDisplayText().Length > 0);
    }

    [Fact]
    public void ToDisplayText_UndefinedValue_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => ((InmateType)99).ToDisplayText());
        Should.Throw<ArgumentOutOfRangeException>(() => ((PaymentMethod)99).ToDisplayText());
        Should.Throw<ArgumentOutOfRangeException>(() => ((TransactionType)99).ToDisplayText());
        Should.Throw<ArgumentOutOfRangeException>(() => ((PasswordRule)99).ToDisplayText());
    }
}
