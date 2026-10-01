using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Infrastructure.Common;

internal sealed class SystemClock : IClock
{
#pragma warning disable RS0030 // The one sanctioned read of the system clock; everything else goes through IClock.
    public DateTime Now => DateTime.Now;
#pragma warning restore RS0030

    public DateOnly Today => DateOnly.FromDateTime(Now);
}
