using LuuKyCanTin.Domain.Common;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.Common;

public class AmountInWordsTests
{
    private const decimal MaxDecimal18 = 999_999_999_999_999_999m;

    [Theory]
    [InlineData(0, "Không đồng")]
    [InlineData(1, "Một đồng")]
    [InlineData(5, "Năm đồng")]
    [InlineData(10, "Mười đồng")]
    [InlineData(11, "Mười một đồng")]
    [InlineData(14, "Mười bốn đồng")]
    [InlineData(15, "Mười lăm đồng")]
    [InlineData(20, "Hai mươi đồng")]
    [InlineData(21, "Hai mươi mốt đồng")]
    [InlineData(24, "Hai mươi bốn đồng")]
    [InlineData(25, "Hai mươi lăm đồng")]
    [InlineData(31, "Ba mươi mốt đồng")]
    [InlineData(55, "Năm mươi lăm đồng")]
    [InlineData(100, "Một trăm đồng")]
    [InlineData(101, "Một trăm lẻ một đồng")]
    [InlineData(105, "Một trăm lẻ năm đồng")]
    [InlineData(110, "Một trăm mười đồng")]
    [InlineData(115, "Một trăm mười lăm đồng")]
    [InlineData(1000, "Một nghìn đồng")]
    [InlineData(1005, "Một nghìn không trăm lẻ năm đồng")]
    [InlineData(1050, "Một nghìn không trăm năm mươi đồng")]
    [InlineData(10000, "Mười nghìn đồng")]
    [InlineData(21000, "Hai mươi mốt nghìn đồng")]
    [InlineData(105000, "Một trăm lẻ năm nghìn đồng")]
    [InlineData(500000, "Năm trăm nghìn đồng")]
    [InlineData(1000000, "Một triệu đồng")]
    [InlineData(1000005, "Một triệu không trăm lẻ năm đồng")]
    [InlineData(1200500, "Một triệu hai trăm nghìn năm trăm đồng")]
    [InlineData(1000000000, "Một tỷ đồng")]
    [InlineData(1000000005, "Một tỷ không trăm lẻ năm đồng")]
    [InlineData(2000001000, "Hai tỷ không trăm lẻ một nghìn đồng")]
    [InlineData(1000000000000, "Một nghìn tỷ đồng")]
    [InlineData(5000000000000000, "Năm triệu tỷ đồng")]
    [InlineData(1001000000000, "Một nghìn không trăm lẻ một tỷ đồng")]
    public void ToWords_ReadsAmountInWords(long amount, string expected)
    {
        AmountInWords.ToWords(amount).ShouldBe(expected);
    }

    [Theory]
    [InlineData(101, "Một trăm linh một đồng")]
    [InlineData(1000005, "Một triệu không trăm linh năm đồng")]
    public void ToWords_NorthernStyle_UsesTheNorthernZeroTensWord(long amount, string expected)
    {
        AmountInWords.ToWords(amount, ZeroTensStyle.Northern).ShouldBe(expected);
    }

    [Fact]
    public void ToWords_MaxDecimal18_ReadsEveryGroup()
    {
        AmountInWords.ToWords(MaxDecimal18).ShouldBe(
            "Chín trăm chín mươi chín triệu chín trăm chín mươi chín nghìn chín trăm chín mươi chín tỷ " +
            "chín trăm chín mươi chín triệu chín trăm chín mươi chín nghìn chín trăm chín mươi chín đồng");
    }

    [Fact]
    public void ToWords_MaxDecimal18_FitsTheStoredColumn()
    {
        // CustodyVoucher.AmountInWords is nvarchar(300).
        AmountInWords.ToWords(MaxDecimal18).Length.ShouldBeLessThanOrEqualTo(300);
    }

    [Fact]
    public void ToWords_TrailingZeroScale_IsStillWholeDong()
    {
        AmountInWords.ToWords(10.00m).ShouldBe("Mười đồng");
    }

    [Fact]
    public void ToWords_Negative_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => AmountInWords.ToWords(-1m));
    }

    [Fact]
    public void ToWords_Fractional_Throws()
    {
        // Money is whole đồng.
        Should.Throw<ArgumentOutOfRangeException>(() => AmountInWords.ToWords(10.5m));
    }
}
