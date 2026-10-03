using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.TestUtilities;

public class MnemonicAssertTests
{
    [Theory]
    [InlineData("&Lưu", 'L')]
    [InlineData("Đăng &xuất", 'X')]
    [InlineData("A && B &Cancel", 'C')]
    public void GetMnemonic_TextWithAnAmpersand_ReturnsTheUpperCaseLetter(string text, char expected)
    {
        MnemonicAssert.GetMnemonic(text).ShouldBe(expected);
    }

    [Theory]
    [InlineData("Lưu")]
    [InlineData("A && B")]
    [InlineData("Trailing&")]
    public void GetMnemonic_TextWithoutMnemonic_ReturnsNull(string text)
    {
        MnemonicAssert.GetMnemonic(text).ShouldBeNull();
    }

    [Fact]
    public void HasUniqueMnemonics_TwoButtonsSharingALetter_Fails()
    {
        StaThread.Run(() =>
        {
            using var form = new Form();
            var panel = new Panel();
            panel.Controls.Add(new Button { Name = "btnSave", Text = "&Lưu" });
            form.Controls.Add(panel);
            form.Controls.Add(new Label { Name = "lblList", Text = "&Liệt kê" });

            Should.Throw<ShouldAssertException>(() => MnemonicAssert.HasUniqueMnemonics(form));
        });
    }

    [Fact]
    public void HasUniqueMnemonics_ALabelWithMnemonicsOff_IsIgnored()
    {
        StaThread.Run(() =>
        {
            using var form = new Form();
            form.Controls.Add(new Button { Name = "btnSave", Text = "&Lưu" });
            form.Controls.Add(new Label { Name = "lblUser", Text = "&Lan", UseMnemonic = false });

            MnemonicAssert.HasUniqueMnemonics(form);
        });
    }
}
