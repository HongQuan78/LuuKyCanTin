using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.Custody;

public class CustodyVoucherTests
{
    private static Inmate Inmate() => new()
    {
        Id = 7,
        InmateCode = "DT-0001",
        FullName = "Nguyễn Văn A",
        InmateType = InmateType.PreTrialDetainee,
        AdmissionDate = new DateOnly(2026, 9, 1),
    };

    private static CustodyVoucher Create(
        decimal amount = 500_000,
        VoucherType voucherType = VoucherType.Receipt,
        TransactionType transactionType = TransactionType.SentByRelative,
        PaymentMethod paymentMethod = PaymentMethod.Cash,
        string? senderAccountNumber = null) =>
        CustodyVoucher.CreatePostedDepositReceipt(
            "BNT-2026-00001",
            new DateOnly(2026, 10, 1),
            Inmate(),
            voucherType,
            transactionType,
            paymentMethod,
            "Trần Thị B",
            "Mẹ",
            sourceDocumentNumber: null,
            senderAccountNumber,
            receivedDate: null,
            "Tiền gửi lưu ký",
            amount,
            "Năm trăm nghìn đồng",
            balanceBefore: 0,
            balanceAfter: amount);

    [Fact]
    public void Factory_PostsTheReceiptWithTheSnapshotAndBalances()
    {
        var voucher = Create();

        voucher.VoucherNumber.ShouldBe("BNT-2026-00001");
        voucher.VoucherDate.ShouldBe(new DateOnly(2026, 10, 1));
        voucher.Status.ShouldBe(VoucherStatus.Posted);
        voucher.IsCancelled.ShouldBeFalse();
        voucher.VoucherType.ShouldBe(VoucherType.Receipt);
        voucher.TransactionType.ShouldBe(TransactionType.SentByRelative);
        voucher.InmateId.ShouldBe(7);
        voucher.InmateFullName.ShouldBe("Nguyễn Văn A");
        voucher.InmateType.ShouldBe(InmateType.PreTrialDetainee);
        voucher.Amount.ShouldBe(500_000);
        voucher.BalanceBefore.ShouldBe(0);
        voucher.BalanceAfter.ShouldBe(500_000);
        voucher.PrintCount.ShouldBe((short)0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Factory_RejectsNonPositiveAmounts(decimal amount)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Create(amount));
    }

    [Fact]
    public void Factory_RejectsABusinessCodeThatDoesNotMatchTheVoucherType()
    {
        Should.Throw<ArgumentException>(() => Create(voucherType: VoucherType.Payout));
    }

    [Fact]
    public void Factory_RejectsAnUnknownBusinessCode()
    {
        Should.Throw<ArgumentException>(() => Create(transactionType: (TransactionType)99));
    }

    [Fact]
    public void Factory_RequiresAnAccountForATransfer()
    {
        Should.Throw<ArgumentException>(() => Create(paymentMethod: PaymentMethod.BankTransfer));
    }

    [Fact]
    public void Factory_AcceptsATransferWithAnAccount()
    {
        var voucher = Create(paymentMethod: PaymentMethod.BankTransfer, senderAccountNumber: "123456789");

        voucher.PaymentMethod.ShouldBe(PaymentMethod.BankTransfer);
        voucher.SenderAccountNumber.ShouldBe("123456789");
    }
}
