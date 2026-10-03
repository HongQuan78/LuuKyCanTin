using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.IntegrationTests.TestUtilities;

/// <summary>A clock that only moves when the test says so.</summary>
public sealed class FakeClock(DateTime now) : IClock
{
    public DateTime Now { get; set; } = now;

    public DateOnly Today => DateOnly.FromDateTime(Now);

    public void Advance(TimeSpan interval) => Now += interval;
}
