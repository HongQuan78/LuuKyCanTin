using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.WinForms.HeThong;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.HeThong;

public sealed class SchemaVersionGateTests
{
    private const string Expected = "20261001141501_InitialCreate";

    private static readonly SchemaVersionCheckResult Khop = SchemaVersionCheckResult.Tao([Expected], [Expected]);
    private static readonly SchemaVersionCheckResult KhongKhop = SchemaVersionCheckResult.Tao([Expected], []);
    private static readonly SchemaVersionCheckResult LoiKetNoi = SchemaVersionCheckResult.TaoLoiKetNoi(Expected);

    private readonly ISchemaVersionChecker _checker = Substitute.For<ISchemaVersionChecker>();
    private readonly List<string> _danhSachThongBao = [];

    private Task<bool> DuocPhepMoAsync(SchemaVersionCheckResult result)
    {
        _checker.KiemTraAsync(Arg.Any<CancellationToken>()).Returns(result);
        return SchemaVersionGate.DuocPhepMoAsync(_checker, _danhSachThongBao.Add, NullLogger.Instance);
    }

    [Fact]
    public void LayThongBaoChan_Matches_ReturnsNull()
    {
        SchemaVersionGate.LayThongBaoChan(Khop).ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("20260901000000_Older")]
    public void LayThongBaoChan_Mismatch_ReturnsVersionMessage(string? actual)
    {
        string[] daApDung = actual is null ? [] : [actual];

        SchemaVersionGate.LayThongBaoChan(SchemaVersionCheckResult.Tao([Expected], daApDung))
            .ShouldBe("Phiên bản cơ sở dữ liệu không khớp, vui lòng liên hệ quản trị viên.");
    }

    [Fact]
    public void LayThongBaoChan_ConnectionFailed_ReturnsConnectionMessage()
    {
        SchemaVersionGate.LayThongBaoChan(LoiKetNoi)
            .ShouldBe("Không kết nối được cơ sở dữ liệu, vui lòng liên hệ quản trị viên.");
    }

    [Fact]
    public async Task DuocPhepMo_Matches_ContinuesWithoutMessage()
    {
        (await DuocPhepMoAsync(Khop)).ShouldBeTrue();

        _danhSachThongBao.ShouldBeEmpty();
    }

    [Fact]
    public async Task DuocPhepMo_Mismatch_ShowsVersionMessageAndStops()
    {
        (await DuocPhepMoAsync(KhongKhop)).ShouldBeFalse();

        _danhSachThongBao.ShouldBe([SchemaVersionGate.LoiPhienBanKhongKhop]);
    }

    [Fact]
    public async Task DuocPhepMo_ConnectionFailed_ShowsConnectionMessageAndStops()
    {
        (await DuocPhepMoAsync(LoiKetNoi)).ShouldBeFalse();

        _danhSachThongBao.ShouldBe([SchemaVersionGate.LoiKhongKetNoiDuoc]);
    }
}
