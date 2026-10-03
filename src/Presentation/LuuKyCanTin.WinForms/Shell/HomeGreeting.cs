namespace LuuKyCanTin.WinForms.Shell;

/// <summary>The Trang chủ heading lines: the greeting by time of day and the Vietnamese date.</summary>
public static class HomeGreeting
{
    public static string CreateGreeting(DateTime now, string name)
    {
        var partOfDay = now.Hour switch
        {
            < 11 => "sáng",
            < 18 => "chiều",
            _ => "tối",
        };
        return $"Chào buổi {partOfDay}, {name}";
    }

    public static string CreateDateLine(DateOnly date) => $"{ToWeekdayText(date.DayOfWeek)}, {date:dd'/'MM'/'yyyy}";

    private static string ToWeekdayText(DayOfWeek day) => day switch
    {
        DayOfWeek.Sunday => "Chủ nhật",
        DayOfWeek.Monday => "Thứ Hai",
        DayOfWeek.Tuesday => "Thứ Ba",
        DayOfWeek.Wednesday => "Thứ Tư",
        DayOfWeek.Thursday => "Thứ Năm",
        DayOfWeek.Friday => "Thứ Sáu",
        DayOfWeek.Saturday => "Thứ Bảy",
        _ => throw new ArgumentOutOfRangeException(nameof(day), day, "Unknown day of week."),
    };
}
