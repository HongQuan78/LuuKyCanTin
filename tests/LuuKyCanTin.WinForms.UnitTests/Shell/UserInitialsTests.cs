using LuuKyCanTin.WinForms.Shell;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

public class UserInitialsTests
{
    [Theory]
    [InlineData("Nguyễn Thị Lan", "NL")]
    [InlineData("đỗ  văn   hùng", "ĐH")]
    [InlineData("admin", "AD")]
    [InlineData("lan.nt", "LA")]
    [InlineData("x", "X")]
    [InlineData("   ", "")]
    public void Create_AName_ReturnsFirstAndLastInitialsInUpperCase(string name, string expected)
    {
        UserInitials.Create(name).ShouldBe(expected);
    }
}
