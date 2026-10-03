using Shouldly;

namespace LuuKyCanTin.IntegrationTests.TestUtilities;

public sealed class FakeClockTests
{
    [Fact]
    public void Advance_PastMidnight_MovesNowAndToday()
    {
        var clock = new FakeClock(new DateTime(2026, 12, 31, 23, 59, 0));

        clock.Today.ShouldBe(new DateOnly(2026, 12, 31));
        clock.Advance(TimeSpan.FromMinutes(2));
        clock.Now.ShouldBe(new DateTime(2027, 1, 1, 0, 1, 0));
        clock.Today.ShouldBe(new DateOnly(2027, 1, 1));
    }
}
