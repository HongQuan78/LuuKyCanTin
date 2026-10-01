namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// The only source of the current date and time, so tests can fix or move it. Both values are local time: the unit
/// works in a single time zone and the database stores local <c>datetime2(0)</c> values.
/// </summary>
public interface IClock
{
    DateOnly Today { get; }

    DateTime Now { get; }
}
