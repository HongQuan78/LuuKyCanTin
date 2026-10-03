using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.Custody;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Custody;

/// <summary>The deposit-receipt screen, built for real on an STA thread.</summary>
public class DepositReceiptScreenTests
{
    private static T Find<T>(Control root, string name)
        where T : Control =>
        (T)root.Controls.Find(name, searchAllChildren: true).Single();

    [Fact]
    public void Mnemonics_AreUnique()
    {
        StaThread.Run(() =>
        {
            using var screen = new DepositReceiptForm();

            MnemonicAssert.HasUniqueMnemonics(screen);
        });
    }

    [Fact]
    public void HostedInTheShell_EnterHasNoDefaultButtonToPost()
    {
        StaThread.Run(() =>
        {
            using var shell = new MainForm();
            ((IContentHost)shell).ShowPage(ShellNavigation.DepositReceiptKey, "Lập biên nhận thu", () => new DepositReceiptForm());

            shell.AcceptButton.ShouldBeNull();
        });
    }

    [Fact]
    public void Escape_RaisesReset()
    {
        StaThread.Run(() =>
        {
            using var screen = new TestableDepositReceiptForm();
            var resets = 0;
            ((IDepositReceiptView)screen).ResetClicked += (_, _) => resets++;

            screen.PressKey(Keys.Escape).ShouldBeTrue();

            resets.ShouldBe(1);
        });
    }

    [Fact]
    public void TypingAnAmount_ShowsItInTheTotalBox()
    {
        StaThread.Run(() =>
        {
            using var screen = new DepositReceiptForm();

            Find<TextBox>(screen, "txtAmount").Text = "1500000";

            Find<Label>(screen, "lblTotal").Text.ShouldBe("1.500.000 đ");
        });
    }

    [Fact]
    public void ShowPosted_ShowsTheNumberInSuccessAndEnablesPrint()
    {
        StaThread.Run(() =>
        {
            using var screen = new DepositReceiptForm();

            ((IDepositReceiptView)screen).ShowPosted("BNT-2026-00001", 500_000m);

            var status = Find<Label>(screen, "lblStatus");
            status.Text.ShouldBe("Đã ghi sổ BNT-2026-00001 · Số dư mới: 500.000 đồng");
            status.ForeColor.ShouldBe(AppTheme.Success);
            Find<Button>(screen, "btnIn").Enabled.ShouldBeTrue();
            Find<Button>(screen, "btnPost").Enabled.ShouldBeFalse();
        });
    }

    [Fact]
    public void Reset_ClearsTheReceiptForTheNextOne()
    {
        StaThread.Run(() =>
        {
            using var screen = new DepositReceiptForm();
            IDepositReceiptView view = screen;
            Find<TextBox>(screen, "txtSenderFullName").Text = "Trần Thị B";
            Find<TextBox>(screen, "txtAmount").Text = "500000";
            view.ShowPosted("BNT-2026-00001", 500_000m);
            view.ShowFieldErrors([new(DepositReceiptField.Relationship, "Quan hệ tối đa 50 ký tự.")]);

            view.Reset();

            view.SenderFullName.ShouldBe("");
            view.Amount.ShouldBeNull();
            Find<Label>(screen, "lblStatus").Text.ShouldBe("");
            Find<Label>(screen, "lblTotal").Text.ShouldBe("0 đ");
            Find<InputFrame>(screen, "frmRelationship").HasError.ShouldBeFalse();
            Find<Button>(screen, "btnPost").Enabled.ShouldBeTrue();
            Find<Button>(screen, "btnIn").Enabled.ShouldBeFalse();
        });
    }

    [Fact]
    public void AccountNumber_IsReadOnlyUnlessBankTransfer()
    {
        StaThread.Run(() =>
        {
            using var screen = new DepositReceiptForm();
            var frame = Find<InputFrame>(screen, "frmAccountNumber");
            frame.ReadOnly.ShouldBeTrue();

            Find<ComboBox>(screen, "cmbPaymentMethod").SelectedIndex = 1;

            frame.ReadOnly.ShouldBeFalse();
        });
    }

    [Fact]
    public void AccountNumberError_ClearsWhenCashMakesTheFieldReadOnly()
    {
        StaThread.Run(() =>
        {
            using var screen = new DepositReceiptForm();
            Find<ComboBox>(screen, "cmbPaymentMethod").SelectedIndex = 1;
            ((IDepositReceiptView)screen).ShowFieldErrors(
                [new(DepositReceiptField.AccountNumber, "Chuyển khoản phải có số tài khoản người gửi.")]);

            Find<ComboBox>(screen, "cmbPaymentMethod").SelectedIndex = 0;

            Find<InputFrame>(screen, "frmAccountNumber").HasError.ShouldBeFalse();
        });
    }

    private sealed class TestableDepositReceiptForm : DepositReceiptForm
    {
        public bool PressKey(Keys keys)
        {
            var message = new Message();
            return ProcessCmdKey(ref message, keys);
        }
    }
}
