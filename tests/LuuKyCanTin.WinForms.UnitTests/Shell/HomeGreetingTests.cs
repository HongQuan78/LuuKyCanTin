using LuuKyCanTin.WinForms.Shell;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class HomeGreetingTests
{
    [Theory]
    [InlineData(0, 0, "Chào buổi sáng, Nguyễn Thị Lan")]
    [InlineData(10, 59, "Chào buổi sáng, Nguyễn Thị Lan")]
    [InlineData(11, 0, "Chào buổi chiều, Nguyễn Thị Lan")]
    [InlineData(17, 59, "Chào buổi chiều, Nguyễn Thị Lan")]
    [InlineData(18, 0, "Chào buổi tối, Nguyễn Thị Lan")]
    [InlineData(23, 59, "Chào buổi tối, Nguyễn Thị Lan")]
    public void CreateGreeting_TimeOfDay_PicksTheGreeting(int hour, int minute, string expected)
    {
        HomeGreeting.CreateGreeting(new DateTime(2026, 10, 2, hour, minute, 0), "Nguyễn Thị Lan").ShouldBe(expected);
    }

    [Theory]
    [InlineData(2026, 10, 4, "Chủ nhật, 04/10/2026")]
    [InlineData(2026, 10, 5, "Thứ Hai, 05/10/2026")]
    [InlineData(2026, 10, 6, "Thứ Ba, 06/10/2026")]
    [InlineData(2026, 10, 7, "Thứ Tư, 07/10/2026")]
    [InlineData(2026, 10, 8, "Thứ Năm, 08/10/2026")]
    [InlineData(2026, 10, 2, "Thứ Sáu, 02/10/2026")]
    [InlineData(2026, 10, 3, "Thứ Bảy, 03/10/2026")]
    public void CreateDateLine_AnyDay_NamesTheWeekdayInVietnamese(int year, int month, int day, string expected)
    {
        HomeGreeting.CreateDateLine(new DateOnly(year, month, day)).ShouldBe(expected);
    }
}
