using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.MasterData;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace LuuKyCanTin.WinForms.UnitTests.MasterData;

public class AddInmatePresenterTests
{
    private readonly IInmateStore _store = Substitute.For<IInmateStore>();
    private readonly IAddInmateView _view = Substitute.For<IAddInmateView>();

    public AddInmatePresenterTests()
    {
        _store.AddAsync(Arg.Any<Inmate>(), Arg.Any<CancellationToken>()).Returns(true);
        _view.InmateCode.Returns("DT-0001");
        _view.FullName.Returns("Nguyễn Văn A");
        _view.BirthYear.Returns((short)1990);
        _view.InmateType.Returns(InmateType.PreTrialDetainee);
        _view.AdmissionDate.Returns(new DateOnly(2026, 9, 30));
    }

    private AddInmatePresenter CreatePresenter()
    {
        var clock = new FakeClock(new DateTime(2026, 10, 1, 8, 0, 0));
        var service = new AddInmateService(_store, clock);
        return new AddInmatePresenter(_view, FakeScopeFactory.Create(service));
    }

    [Fact]
    public async Task Success_ClosesTheViewAsOk()
    {
        await CreatePresenter().SaveAsync();

        _view.Received(1).CloseWithResult(true);
        await _store.Received(1).AddAsync(Arg.Any<Inmate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ADuplicateCode_IsShownUnderTheCodeAndKeepsTheFormOpen()
    {
        _store.CodeExistsAsync("DT-0001", Arg.Any<CancellationToken>()).Returns(true);

        await CreatePresenter().SaveAsync();

        _view.Received(1).ShowFieldErrors(Arg.Is<IReadOnlyList<FieldMessage<InmateField>>>(e =>
            e.Single() == new FieldMessage<InmateField>(InmateField.InmateCode, AddInmateService.DuplicateCodeMessage)));
        _view.DidNotReceive().ShowError(Arg.Any<string>());
        _view.DidNotReceive().CloseWithResult(Arg.Any<bool>());
    }

    [Fact]
    public async Task InvalidFields_AreEachShownUnderTheirField()
    {
        _view.InmateCode.Returns("");
        _view.FullName.Returns("");
        _view.AdmissionDate.Returns(new DateOnly(2026, 10, 2));
        _view.Cell.Returns(new string('A', 51));

        await CreatePresenter().SaveAsync();

        _view.Received(1).ShowFieldErrors(Arg.Is<IReadOnlyList<FieldMessage<InmateField>>>(e => e.SequenceEqual(new FieldMessage<InmateField>[]
        {
            new(InmateField.InmateCode, "Mã số không được để trống."),
            new(InmateField.FullName, "Họ tên không được để trống."),
            new(InmateField.AdmissionDate, "Ngày vào không được lớn hơn ngày hiện tại."),
            new(InmateField.Cell, "Buồng giam tối đa 50 ký tự."),
        })));
        _view.DidNotReceive().CloseWithResult(Arg.Any<bool>());
    }

    [Fact]
    public async Task AnUnexpectedFailure_IsShownInTheBanner()
    {
        _store.CodeExistsAsync("DT-0001", Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("Mất kết nối."));
        CreatePresenter();

        _view.SaveClicked += Raise.Event();
        await Task.Yield();

        _view.Received(1).ShowError("Không lưu được đối tượng: Mất kết nối.");
        _view.DidNotReceive().CloseWithResult(Arg.Any<bool>());
    }
}
