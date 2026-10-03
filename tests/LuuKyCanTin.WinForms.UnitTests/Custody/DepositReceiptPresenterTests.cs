using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.Custody;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Custody;

public class DepositReceiptPresenterTests
{
    private readonly IInmateStore _inmateStore = Substitute.For<IInmateStore>();
    private readonly ICustodyVoucherStore _voucherStore = Substitute.For<ICustodyVoucherStore>();
    private readonly IAppDbContext _db = Substitute.For<IAppDbContext>();
    private readonly INumberingService _numbering = Substitute.For<INumberingService>();
    private readonly ICustodyBalanceWriter _balanceWriter = Substitute.For<ICustodyBalanceWriter>();
    private readonly IPermissionChecker _checker = Substitute.For<IPermissionChecker>();
    private readonly IFacilityInfoStore _facilityInfoStore = Substitute.For<IFacilityInfoStore>();
    private readonly IReportRenderer _renderer = Substitute.For<IReportRenderer>();
    private readonly IDepositReceiptView _view = Substitute.For<IDepositReceiptView>();

    private static Inmate SampleInmate() => new()
    {
        Id = 7,
        InmateCode = "DT-0001",
        FullName = "Nguyễn Văn A",
        InmateType = InmateType.PreTrialDetainee,
        AdmissionDate = new DateOnly(2026, 9, 1),
    };

    public DepositReceiptPresenterTests()
    {
        _db.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(new FakeTransaction());
        _db.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
        _inmateStore.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(SampleInmate());
        _inmateStore.GetInCustodyAsync(Arg.Any<CancellationToken>()).Returns([SampleInmate()]);
        _numbering.AllocateNumberAsync("BNT", 2026, Arg.Any<CancellationToken>()).Returns("BNT-2026-00001");
        _balanceWriter.IncreaseAsync(7, 500_000, Arg.Any<CancellationToken>()).Returns((0m, 500_000m));

        _view.InmateId.Returns(7);
        _view.VoucherDate.Returns(new DateOnly(2026, 10, 1));
        _view.TransactionType.Returns(TransactionType.SentByRelative);
        _view.PaymentMethod.Returns(PaymentMethod.Cash);
        _view.SenderFullName.Returns("Trần Thị B");
        _view.Relationship.Returns("Mẹ");
        _view.Amount.Returns(500_000m);
    }

    private DepositReceiptPresenter CreatePresenter()
    {
        var clock = new FakeClock(new DateTime(2026, 10, 1, 8, 0, 0));
        var ledger = new CustodyLedgerService(_inmateStore, _voucherStore, _db, _numbering, _balanceWriter, _checker, clock);
        var printQuery = new DepositReceiptPrintQuery(_voucherStore, _facilityInfoStore);
        var items = new InmatesInCustodyQuery(_inmateStore);
        return new DepositReceiptPresenter(_view, FakeScopeFactory.Create(ledger, printQuery, items, _renderer));
    }

    [Fact]
    public async Task Loading_FillsTheDetaineeSelector()
    {
        await CreatePresenter().LoadAsync();

        _view.Received(1).Inmates = Arg.Is<IReadOnlyList<InmateOption>>(list => list.Count == 1 && list[0].FullName == "Nguyễn Văn A");
    }

    [Fact]
    public void TheAmountPreview_ReadsTheAmountInWords()
    {
        CreatePresenter().UpdateAmountInWords();

        _view.Received(1).AmountInWords = "Năm trăm nghìn đồng";
    }

    [Fact]
    public async Task Posting_Succeeds_ShowsTheNumberAndEnablesPrint()
    {
        await CreatePresenter().PostAsync();

        _view.Received(1).ShowPosted("BNT-2026-00001", 500_000m);
        _view.DidNotReceive().ShowError(Arg.Any<string>());
    }

    [Fact]
    public async Task Posting_Fails_ShowsTheBusinessMessage()
    {
        _inmateStore.FindByIdAsync(7, Arg.Any<CancellationToken>()).Returns(new Inmate
        {
            Id = 7,
            InmateCode = "DT-0001",
            FullName = "Nguyễn Văn A",
            AdmissionDate = new DateOnly(2026, 9, 1),
            Status = InmateStatus.FacilityTransferred,
            ReleaseDate = new DateOnly(2026, 9, 30),
        });

        await CreatePresenter().PostAsync();

        _view.Received(1).ShowError(Arg.Any<string>());
        _view.DidNotReceive().ShowPosted(Arg.Any<string>(), Arg.Any<decimal>());
    }

    [Fact]
    public async Task Posting_InvalidFields_ShowsEachUnderItsFieldAndNoMessageBox()
    {
        _view.Amount.Returns((decimal?)null);
        _view.SenderFullName.Returns("");
        _view.PaymentMethod.Returns(PaymentMethod.BankTransfer);

        await CreatePresenter().PostAsync();

        _view.Received(1).ShowFieldErrors(Arg.Is<IReadOnlyList<FieldMessage<DepositReceiptField>>>(e => e.SequenceEqual(new FieldMessage<DepositReceiptField>[]
        {
            new(DepositReceiptField.Amount, "Số tiền phải lớn hơn 0."),
            new(DepositReceiptField.SenderFullName, "Người gửi không được để trống."),
            new(DepositReceiptField.AccountNumber, "Chuyển khoản phải có số tài khoản người gửi."),
        })));
        _view.DidNotReceive().ShowError(Arg.Any<string>());
        _view.DidNotReceive().ShowPosted(Arg.Any<string>(), Arg.Any<decimal>());
    }

    [Fact]
    public async Task Posting_WithoutADetainee_ShowsTheErrorUnderTheDetainee()
    {
        _view.InmateId.Returns((int?)null);

        await CreatePresenter().PostAsync();

        _view.Received(1).ShowFieldErrors(Arg.Is<IReadOnlyList<FieldMessage<DepositReceiptField>>>(e =>
            e.Single() == new FieldMessage<DepositReceiptField>(DepositReceiptField.Inmate, "Phải chọn đối tượng.")));
    }

    [Fact]
    public async Task Reset_ClearsTheViewAndReloadsTheDetainees()
    {
        await CreatePresenter().ResetAsync();

        Received.InOrder(() =>
        {
            _view.Reset();
            _view.Inmates = Arg.Any<IReadOnlyList<InmateOption>>();
        });
    }

    [Fact]
    public async Task Reset_AfterPosting_ForgetsTheVoucherSoPrintHasNothingToPrint()
    {
        var presenter = CreatePresenter();
        await presenter.PostAsync();

        await presenter.ResetAsync();
        await presenter.PrintAsync();

        _view.Received(1).ShowError("Chưa có chứng từ để in.");
        _renderer.DidNotReceive().Render(Arg.Any<DepositReceiptModel>());
    }

    [Fact]
    public void ResetClicked_ResetsTheView()
    {
        CreatePresenter();

        _view.ResetClicked += Raise.Event();

        _view.Received(1).Reset();
    }

    [Fact]
    public async Task AMissingCounter_ThrowsInsteadOfPostingSilently()
    {
        _numbering.AllocateNumberAsync("BNT", 2026, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(new InvalidOperationException("Không có bộ đếm.")));

        await Should.ThrowAsync<InvalidOperationException>(() => CreatePresenter().PostAsync());

        _view.DidNotReceive().ShowPosted(Arg.Any<string>(), Arg.Any<decimal>());
    }

    [Fact]
    public async Task Printing_RendersTheSnapshotModelAndShowsThePreview()
    {
        var voucher = CustodyVoucher.CreatePostedDepositReceipt(
            "BNT-2026-00001",
            new DateOnly(2026, 10, 1),
            SampleInmate(),
            VoucherType.Receipt,
            TransactionType.SentByRelative,
            PaymentMethod.Cash,
            "Trần Thị B",
            "Mẹ",
            null,
            null,
            null,
            "Tiền gửi lưu ký",
            500_000,
            "Năm trăm nghìn đồng",
            0,
            500_000);
        _voucherStore.FindByIdAsync(Arg.Any<long>(), Arg.Any<CancellationToken>()).Returns(voucher);
        _facilityInfoStore.GetAsync(Arg.Any<CancellationToken>()).Returns(new FacilityInfo { Id = 1, FacilityName = "Trại …", Address = "…" });
        var pdf = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _renderer.Render(Arg.Any<DepositReceiptModel>()).Returns(pdf);

        var presenter = CreatePresenter();
        await presenter.PostAsync();
        await presenter.PrintAsync();

        _view.Received(1).ShowPrintPreview(pdf, "BienNhanThu-BNT-2026-00001");
    }

    private sealed class FakeTransaction : IAppTransaction
    {
        public Task CommitAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task RollbackAsync(CancellationToken ct = default) => Task.CompletedTask;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
