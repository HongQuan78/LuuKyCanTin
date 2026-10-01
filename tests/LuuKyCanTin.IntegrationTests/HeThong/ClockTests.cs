using LuuKyCanTin.Infrastructure.Common;
using LuuKyCanTin.IntegrationTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.HeThong;

public class ClockTests
{
    [Fact]
    public void SystemClock_ReturnsLocalTime()
    {
        var before = DateTime.Now;
        var now = new SystemClock().Now;

        now.Kind.ShouldBe(DateTimeKind.Local);
        now.ShouldBeInRange(before, DateTime.Now);
    }

    [Fact]
    public void SystemClock_TodayIsTheDateOfNow()
    {
        var before = DateOnly.FromDateTime(DateTime.Now);
        var today = new SystemClock().Today;

        today.ShouldBeInRange(before, DateOnly.FromDateTime(DateTime.Now));
    }

    [Fact]
    public void FakeClock_MovesOnlyWhenAdvanced()
    {
        var clock = new FakeClock(new DateTime(2026, 12, 31, 23, 59, 0));

        clock.Today.ShouldBe(new DateOnly(2026, 12, 31));
        clock.Advance(TimeSpan.FromMinutes(2));
        clock.Now.ShouldBe(new DateTime(2027, 1, 1, 0, 1, 0));
        clock.Today.ShouldBe(new DateOnly(2027, 1, 1));
    }
}
