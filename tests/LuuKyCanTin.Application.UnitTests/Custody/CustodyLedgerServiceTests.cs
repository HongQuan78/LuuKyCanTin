using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.Custody;

public class CustodyLedgerServiceTests
{
    private readonly IInmateStore _inmateStore = Substitute.For<IInmateStore>();
    private readonly ICustodyVoucherStore _voucherStore = Substitute.For<ICustodyVoucherStore>();
    private readonly IAppDbContext _db = Substitute.For<IAppDbContext>();
    private readonly INumberingService _numbering = Substitute.For<INumberingService>();
    private readonly ICustodyBalanceWriter _balanceWriter = Substitute.For<ICustodyBalanceWriter>();
    private readonly IPermissionChecker _checker = Substitute.For<IPermissionChecker>();
    private readonly IAppTransaction _transaction = Substitute.For<IAppTransaction>();
    private readonly FakeClock _clock = new(new DateTime(2026, 10, 1, 8, 0, 0));

    private readonly CustodyLedgerService _service;

    public CustodyLedgerServiceTests()
    {
        _db.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(_transaction);
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _service = new CustodyLedgerService(_inmateStore, _voucherStore, _db, _numbering, _balanceWriter, _checker, _clock);

        _inmateStore.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(new Inmate
        {
            Id = 7,
            InmateCode = "DT-0001",
            FullName = "Nguyễn Văn A",
            InmateType = InmateType.PreTrialDetainee,
            AdmissionDate = new DateOnly(2026, 9, 1),
            Status = InmateStatus.InCustody,
        });
        _numbering.AllocateNumberAsync("BNT", 2026, Arg.Any<CancellationToken>()).Returns("BNT-2026-00001");
        _balanceWriter.IncreaseAsync(7, 500_000, Arg.Any<CancellationToken>()).Returns((0m, 500_000m));
    }

    private static PostDepositReceiptRequest Request(decimal amount = 500_000) => new()
    {
        InmateId = 7,
        VoucherDate = new DateOnly(2026, 10, 1),
        TransactionType = TransactionType.SentByRelative,
        PaymentMethod = PaymentMethod.Cash,
        SenderFullName = "Trần Thị B",
        Relationship = "Mẹ",
        Amount = amount,
    };

    [Fact]
    public async Task HappyPath_BuildsThePostedReceiptWithSnapshotWordsAndBalances()
    {
        CustodyVoucher? added = null;
        _voucherStore.When(s => s.Add(Arg.Any<CustodyVoucher>())).Do(call => added = call.Arg<CustodyVoucher>());

        var result = await _service.PostDepositReceiptAsync(Request());

        result.Succeeded.ShouldBeTrue();
        result.VoucherNumber.ShouldBe("BNT-2026-00001");
        result.BalanceAfter.ShouldBe(500_000m);

        added.ShouldNotBeNull();
        added.VoucherNumber.ShouldBe("BNT-2026-00001");
        added.Status.ShouldBe(VoucherStatus.Posted);
        added.InmateFullName.ShouldBe("Nguyễn Văn A");
        added.InmateType.ShouldBe(InmateType.PreTrialDetainee);
        added.BalanceBefore.ShouldBe(0m);
        added.BalanceAfter.ShouldBe(500_000m);
        added.AmountInWords.ShouldBe("Năm trăm nghìn đồng");

        await _db.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WithoutPermission_IsRefusedBeforeTheTransactionOpens()
    {
        _checker.RequireAsync(PermissionCodes.CustodyIncrease.Create, Arg.Any<CancellationToken>())
            .ThrowsAsync(new PermissionDeniedException(PermissionCodes.CustodyIncrease.Create));

        await Should.ThrowAsync<PermissionDeniedException>(() => _service.PostDepositReceiptAsync(Request()));

        await _db.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _numbering.DidNotReceive().AllocateNumberAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Numbering_UsesTheWorkingYearNotTheDocumentDate()
    {
        var request = Request() with { VoucherDate = new DateOnly(2025, 12, 31) };

        (await _service.PostDepositReceiptAsync(request)).Succeeded.ShouldBeTrue();

        await _numbering.Received(1).AllocateNumberAsync("BNT", 2026, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BalanceUpdateRefused_ReturnsABusinessErrorAndSavesNothing()
    {
        _balanceWriter.IncreaseAsync(7, 500_000, Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<(decimal BalanceBefore, decimal BalanceAfter)?>(null));

        var result = await _service.PostDepositReceiptAsync(Request());

        result.Succeeded.ShouldBeFalse();
        result.Message.ShouldNotBeNullOrEmpty();
        _voucherStore.DidNotReceive().Add(Arg.Any<CustodyVoucher>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidationFailure_StopsBeforeNumbering()
    {
        var result = await _service.PostDepositReceiptAsync(Request(amount: 0));

        result.Succeeded.ShouldBeFalse();
        await _numbering.DidNotReceive().AllocateNumberAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _transaction.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidationFailure_ReturnsEveryMessageWithItsProperty()
    {
        var result = await _service.PostDepositReceiptAsync(Request(amount: 0) with { SenderFullName = "" });

        result.Message.ShouldBe("Số tiền phải lớn hơn 0.");
        result.Errors.Select(e => e.PropertyName).ShouldBe(
            [nameof(PostDepositReceiptRequest.Amount), nameof(PostDepositReceiptRequest.SenderFullName)]);
    }

    [Fact]
    public async Task ABusinessFailure_NamesNoProperty()
    {
        // No detainee 8 exists, so the post is refused after validation passed.
        var result = await _service.PostDepositReceiptAsync(Request() with { InmateId = 8 });

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ABlankSender_StopsBeforeNumbering(string sender)
    {
        var result = await _service.PostDepositReceiptAsync(Request() with { SenderFullName = sender });

        result.Succeeded.ShouldBeFalse();
        await _numbering.DidNotReceive().AllocateNumberAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task APayoutBusinessCode_StopsBeforeNumbering()
    {
        var result = await _service.PostDepositReceiptAsync(Request() with { TransactionType = TransactionType.GivenToOtherInmate });

        result.Succeeded.ShouldBeFalse();
        await _numbering.DidNotReceive().AllocateNumberAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ATransferWithoutAnAccount_StopsBeforeNumbering()
    {
        var request = Request() with { PaymentMethod = PaymentMethod.BankTransfer, SenderAccountNumber = null };

        var result = await _service.PostDepositReceiptAsync(request);

        result.Succeeded.ShouldBeFalse();
        await _numbering.DidNotReceive().AllocateNumberAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnAmountBeyondDecimal18_StopsBeforeNumbering()
    {
        var result = await _service.PostDepositReceiptAsync(Request(amount: 1_000_000_000_000_000_000m));

        result.Succeeded.ShouldBeFalse();
        await _numbering.DidNotReceive().AllocateNumberAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AnInactiveDetainee_IsRefusedBeforeNumbering()
    {
        var inmate = new Inmate
        {
            Id = 7,
            InmateCode = "DT-0001",
            FullName = "Nguyễn Văn A",
            AdmissionDate = new DateOnly(2026, 9, 1),
            Status = InmateStatus.FacilityTransferred,
            ReleaseDate = new DateOnly(2026, 9, 30),
        };
        _inmateStore.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(inmate);

        var result = await _service.PostDepositReceiptAsync(Request());

        result.Succeeded.ShouldBeFalse();
        await _numbering.DidNotReceive().AllocateNumberAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        await _transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AMissingCounter_RollsBackTheTransactionAndSavesNothing()
    {
        _numbering.AllocateNumberAsync("BNT", 2026, Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Không có bộ đếm cho loại chứng từ BNT năm 2026."));

        await Should.ThrowAsync<InvalidOperationException>(() => _service.PostDepositReceiptAsync(Request()));

        _voucherStore.DidNotReceive().Add(Arg.Any<CustodyVoucher>());
        await _db.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

}
