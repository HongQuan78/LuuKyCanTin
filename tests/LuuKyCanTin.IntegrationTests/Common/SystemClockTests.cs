using LuuKyCanTin.Infrastructure.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Common;

public sealed class SystemClockTests
{
    [Fact]
    public void Now_Always_IsLocalTime()
    {
        var truoc = DateTime.Now;
        var thoiDiem = new SystemClock().Now;

        thoiDiem.Kind.ShouldBe(DateTimeKind.Local);
        thoiDiem.ShouldBeInRange(truoc, DateTime.Now);
    }

    [Fact]
    public void Today_Always_IsTheDateOfNow()
    {
        var truoc = DateOnly.FromDateTime(DateTime.Now);
        var homNay = new SystemClock().Today;

        homNay.ShouldBeInRange(truoc, DateOnly.FromDateTime(DateTime.Now));
    }
}
