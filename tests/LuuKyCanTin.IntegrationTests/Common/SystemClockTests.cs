using LuuKyCanTin.Infrastructure.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Common;

public sealed class SystemClockTests
{
    [Fact]
    public void Now_Always_IsLocalTime()
    {
        var before = DateTime.Now;
        var now = new SystemClock().Now;

        now.Kind.ShouldBe(DateTimeKind.Local);
        now.ShouldBeInRange(before, DateTime.Now);
    }

    [Fact]
    public void Today_Always_IsTheDateOfNow()
    {
        var before = DateOnly.FromDateTime(DateTime.Now);
        var today = new SystemClock().Today;

        today.ShouldBeInRange(before, DateOnly.FromDateTime(DateTime.Now));
    }
}
