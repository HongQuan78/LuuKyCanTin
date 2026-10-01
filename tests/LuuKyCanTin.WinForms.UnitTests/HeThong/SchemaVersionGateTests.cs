using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.WinForms.HeThong;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public class SchemaVersionGateTests
{
    private const string Expected = "20261001141501_InitialCreate";

    [Fact]
    public void Match_DoesNotBlock()
    {
        SchemaVersionGate.BlockingMessage(SchemaVersionCheckResult.Compare(Expected, Expected)).ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("20260901000000_Older")]
    public void Mismatch_BlocksWithVersionMessage(string? actual)
    {
        SchemaVersionGate.BlockingMessage(SchemaVersionCheckResult.Compare(Expected, actual))
            .ShouldBe("Phiên bản cơ sở dữ liệu không khớp, vui lòng liên hệ quản trị viên.");
    }

    [Fact]
    public void ConnectionFailure_BlocksWithItsOwnMessage()
    {
        SchemaVersionGate.BlockingMessage(SchemaVersionCheckResult.ConnectionFailed(Expected))
            .ShouldBe("Không kết nối được cơ sở dữ liệu, vui lòng liên hệ quản trị viên.");
    }
}
