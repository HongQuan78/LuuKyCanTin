using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.UnitTests.TestUtilities;
using LuuKyCanTin.Domain.MasterData;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.MasterData;

public class AddInmateServiceTests
{
    private readonly IInmateStore _store = Substitute.For<IInmateStore>();
    private readonly FakeClock _clock = new(new DateTime(2026, 10, 1, 8, 0, 0));
    private readonly AddInmateService _service;

    public AddInmateServiceTests()
    {
        _store.AddAsync(Arg.Any<Inmate>(), Arg.Any<CancellationToken>()).Returns(true);
        _service = new AddInmateService(_store, _clock);
    }

    private static AddInmateRequest Request(string inmateCode = "DT-0001") =>
        new(inmateCode, "Nguyễn Văn A", 1990, InmateType.PreTrialDetainee, new DateOnly(2026, 9, 30), "A3");

    [Fact]
    public async Task HappyPath_SavesAManagedDetaineeWithAZeroBalance()
    {
        Inmate? added = null;
        _store.When(s => s.AddAsync(Arg.Any<Inmate>(), Arg.Any<CancellationToken>()))
            .Do(call => added = call.Arg<Inmate>());

        var result = await _service.AddAsync(Request());

        result.Succeeded.ShouldBeTrue();
        added.ShouldNotBeNull();
        added.Status.ShouldBe(InmateStatus.InCustody);
        added.CustodyBalance.ShouldBe(0m);
        added.FullName.ShouldBe("Nguyễn Văn A");
        await _store.Received(1).AddAsync(Arg.Any<Inmate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ADuplicateCode_ReturnsTheFriendlyMessageAndSavesNothing()
    {
        _store.CodeExistsAsync("DT-0001", Arg.Any<CancellationToken>()).Returns(true);

        var result = await _service.AddAsync(Request());

        result.Succeeded.ShouldBeFalse();
        result.Message.ShouldBe(AddInmateService.DuplicateCodeMessage);
        await _store.DidNotReceive().AddAsync(Arg.Any<Inmate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AConcurrentInsertThatWinsTheRace_ReturnsTheSameFriendlyMessage()
    {
        // The pre-check saw nothing; the unique index rejected the insert inside the store.
        _store.CodeExistsAsync("DT-0001", Arg.Any<CancellationToken>()).Returns(false);
        _store.AddAsync(Arg.Any<Inmate>(), Arg.Any<CancellationToken>()).Returns(false);

        var result = await _service.AddAsync(Request());

        result.Succeeded.ShouldBeFalse();
        result.Message.ShouldBe(AddInmateService.DuplicateCodeMessage);
    }

    [Fact]
    public async Task ABlankCode_FailsValidationBeforeTouchingTheStore()
    {
        var result = await _service.AddAsync(Request(inmateCode: "  "));

        result.Succeeded.ShouldBeFalse();
        await _store.DidNotReceive().CodeExistsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _store.DidNotReceive().AddAsync(Arg.Any<Inmate>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AFutureEntryDate_FailsValidation()
    {
        var request = Request() with { AdmissionDate = _clock.Today.AddDays(1) };

        (await _service.AddAsync(request)).Succeeded.ShouldBeFalse();
    }
}
