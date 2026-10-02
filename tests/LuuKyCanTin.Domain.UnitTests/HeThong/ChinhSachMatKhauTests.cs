using LuuKyCanTin.Domain.HeThong;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.HeThong;

public class ChinhSachMatKhauTests
{
    [Theory]
    [InlineData("Abc123")]          // 6 characters: too short
    [InlineData("Abcdefgh")]        // no digit
    [InlineData("abcdefg1")]        // no upper-case letter
    [InlineData("ABCDEFG1")]        // no lower-case letter
    public void KiemTra_APasswordBreakingOneRule_ReturnsThatViolation(string matKhau)
    {
        ChinhSachMatKhau.KiemTra(matKhau).ShouldNotBeEmpty();
    }

    [Theory]
    [InlineData("Abcdef12")]
    [InlineData("ĐổiMới2026")]      // Đ is upper-case, ổ/ớ are lower-case
    [InlineData("Đăc biệt 1")]      // Vietnamese letters count through char.IsUpper/IsLower
    public void KiemTra_AStrongPassword_IsAccepted(string matKhau)
    {
        ChinhSachMatKhau.KiemTra(matKhau).ShouldBeEmpty();
    }

    [Fact]
    public void KiemTra_Null_ReportsEveryViolation()
    {
        ChinhSachMatKhau.KiemTra(null).Count.ShouldBe(4);
    }
}
