using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Common;

public class SharedControlTests
{
    [Fact]
    public void CardPanel_WithAHeader_PadsTheBodyBelowTheHeaderBand()
    {
        StaThread.Run(() =>
        {
            using var card = new CardPanel();
            card.Padding.Top.ShouldBe(AppTheme.CardPadding);

            card.HeaderText = "Danh sách";

            card.Padding.Top.ShouldBe(AppTheme.CardHeaderHeight + AppTheme.CardPadding);
            card.Padding.Left.ShouldBe(AppTheme.CardPadding);
        });
    }

    [Fact]
    public void InputFrame_Inner_IsHostedBorderlessAndFillsTheFrame()
    {
        StaThread.Run(() =>
        {
            using var frame = new InputFrame { Width = 300 };
            var textBox = new TextBox();

            frame.Inner = textBox;

            frame.Height.ShouldBe(AppTheme.InputHeight);
            textBox.Parent.ShouldBeSameAs(frame);
            textBox.BorderStyle.ShouldBe(BorderStyle.None);
            textBox.Width.ShouldBe(280);
        });
    }

    [Fact]
    public void InputFrame_WithAGlyph_StartsTheTextAfterIt()
    {
        StaThread.Run(() =>
        {
            using var frame = new InputFrame { Width = 300 };
            var textBox = new TextBox();
            frame.Inner = textBox;

            frame.Glyph = Glyphs.Contact;

            textBox.Left.ShouldBe(34);
        });
    }

    [Fact]
    public void InputFrame_ReadOnly_MakesTheTextBoxReadOnlyOnTheSubtleBackground()
    {
        StaThread.Run(() =>
        {
            using var frame = new InputFrame();
            var textBox = new TextBox();
            frame.Inner = textBox;

            frame.ReadOnly = true;

            textBox.ReadOnly.ShouldBeTrue();
            frame.BackColor.ShouldBe(AppTheme.Subtle);
        });
    }

    [Fact]
    public void FieldError_Message_ShowsWhenSetAndHidesWhenCleared()
    {
        StaThread.Run(() =>
        {
            using var error = new FieldError();
            error.Visible.ShouldBeFalse();

            error.Message = "Xác nhận mật khẩu không khớp";
            error.Visible.ShouldBeTrue();

            error.Message = "";
            error.Visible.ShouldBeFalse();
        });
    }

    [Fact]
    public void Banner_Message_ShowsWhenSetAndUsesTheKindColours()
    {
        StaThread.Run(() =>
        {
            using var banner = new Banner { Width = 372 };
            banner.Visible.ShouldBeFalse();

            banner.Message = "Tên đăng nhập hoặc mật khẩu không đúng.";

            banner.Visible.ShouldBeTrue();
            banner.BackColor.ShouldBe(AppTheme.DangerSoft);

            banner.Kind = BannerKind.Warning;
            banner.BackColor.ShouldBe(AppTheme.WarningSoft);
        });
    }

    [Fact]
    public void Banner_ALongMessage_GrowsTallerThanAShortOne()
    {
        StaThread.Run(() =>
        {
            using var banner = new Banner { Width = 200 };
            banner.Message = "Ngắn";
            var shortHeight = banner.Height;

            banner.Message = string.Join(' ', Enumerable.Repeat("Bản sao lưu gần nhất đã quá 24 giờ.", 4));

            banner.Height.ShouldBeGreaterThan(shortHeight);
        });
    }
}
