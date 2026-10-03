using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Administration;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

public sealed class SchemaVersionGateTests
{
    private const string Expected = "20261001141501_InitialCreate";

    private static readonly SchemaVersionCheckResult Matching = SchemaVersionCheckResult.Create([Expected], [Expected]);
    private static readonly SchemaVersionCheckResult Mismatched = SchemaVersionCheckResult.Create([Expected], []);
    private static readonly SchemaVersionCheckResult ConnectionFailure = SchemaVersionCheckResult.CreateConnectionFailed(Expected);

    private readonly ISchemaVersionChecker _checker = Substitute.For<ISchemaVersionChecker>();
    private readonly List<string> _messages = [];

    private Task<bool> CanOpenAsync(SchemaVersionCheckResult result)
    {
        _checker.CheckAsync(Arg.Any<CancellationToken>()).Returns(result);
        return SchemaVersionGate.CanOpenAsync(_checker, _messages.Add, NullLogger.Instance);
    }

    [Fact]
    public void GetBlockingMessage_Matches_ReturnsNull()
    {
        SchemaVersionGate.GetBlockingMessage(Matching).ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("20260901000000_Older")]
    public void GetBlockingMessage_Mismatch_ReturnsVersionMessage(string? actual)
    {
        string[] applied = actual is null ? [] : [actual];

        SchemaVersionGate.GetBlockingMessage(SchemaVersionCheckResult.Create([Expected], applied))
            .ShouldBe("Phiên bản cơ sở dữ liệu không khớp, vui lòng liên hệ quản trị viên.");
    }

    [Fact]
    public void GetBlockingMessage_ConnectionFailed_ReturnsConnectionMessage()
    {
        SchemaVersionGate.GetBlockingMessage(ConnectionFailure)
            .ShouldBe("Không kết nối được cơ sở dữ liệu, vui lòng liên hệ quản trị viên.");
    }

    [Fact]
    public async Task CanOpen_Matches_ContinuesWithoutMessage()
    {
        (await CanOpenAsync(Matching)).ShouldBeTrue();

        _messages.ShouldBeEmpty();
    }

    [Fact]
    public async Task CanOpen_Mismatch_ShowsVersionMessageAndStops()
    {
        (await CanOpenAsync(Mismatched)).ShouldBeFalse();

        _messages.ShouldBe([SchemaVersionGate.VersionMismatchMessage]);
    }

    [Fact]
    public async Task CanOpen_ConnectionFailed_ShowsConnectionMessageAndStops()
    {
        (await CanOpenAsync(ConnectionFailure)).ShouldBeFalse();

        _messages.ShouldBe([SchemaVersionGate.ConnectionFailedMessage]);
    }
}
