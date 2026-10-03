using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.Custody;
using LuuKyCanTin.WinForms.MasterData;
using LuuKyCanTin.WinForms.Shell;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using NSubstitute;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Shell;

/// <summary>The redesigned shell screens, built for real on an STA thread but never shown.</summary>
public class ShellScreenTests
{
    private static NavigationModel Navigation() =>
        ShellNavigation.Create(Substitute.For<INavigator>(), () => { }, () => { }, () => { });

    [Fact]
    public void LoginForm_Mnemonics_AreUnique()
    {
        StaThread.Run(() =>
        {
            using var form = new LoginForm(new WorkstationInfo("SRV / Db", true, "QUAY-01", "v1.0.0"));

            MnemonicAssert.HasUniqueMnemonics(form);
            form.AcceptButton.ShouldNotBeNull();
            form.CancelButton.ShouldNotBeNull();
        });
    }

    [Fact]
    public void LoginForm_CancelButtonOnTheModelessForm_ClosesIt()
    {
        StaThread.Run(() =>
        {
            using var form = new LoginForm { Opacity = 0, ShowInTaskbar = false };
            form.Show();

            ((Button)form.CancelButton!).PerformClick();

            form.IsDisposed.ShouldBeTrue();
            form.DialogResult.ShouldBe(DialogResult.Cancel);
        });
    }

    [Fact]
    public void LoginForm_IsBusy_DisablesTheButtonAndChangesItsText()
    {
        StaThread.Run(() =>
        {
            using var form = new LoginForm();
            var button = (Button)form.AcceptButton!;

            ((ILoginView)form).IsBusy = true;
            button.Enabled.ShouldBeFalse();
            button.Text.ShouldBe("Đang đăng nhập…");

            ((ILoginView)form).IsBusy = false;
            button.Enabled.ShouldBeTrue();
            button.Text.ShouldBe("Đăng &nhập");
        });
    }

    [Theory]
    [InlineData(true, "Đăng &xuất")]
    [InlineData(false, "&Hủy")]
    public void ChangePasswordForm_EitherMode_HasUniqueMnemonicsAndTheRightSecondaryButton(bool isForced, string cancelText)
    {
        StaThread.Run(() =>
        {
            using var form = new ChangePasswordForm();
            IChangePasswordView view = form;

            view.IsForced = isForced;

            MnemonicAssert.HasUniqueMnemonics(form);
            ((Button)form.CancelButton!).Text.ShouldBe(cancelText);
        });
    }

    [Fact]
    public void ChangePasswordForm_RuleResults_TickEachRule()
    {
        StaThread.Run(() =>
        {
            using var form = new ChangePasswordForm();
            IChangePasswordView view = form;

            view.ShowRuleResults(PasswordPolicy.Evaluate("Moi@2026a", "LuuKy@2026"));

            Should.NotThrow(() => view.ShowRuleResults(PasswordPolicy.Evaluate("", "")));
        });
    }

    [Fact]
    public void LockScreenForm_HasUniqueMnemonicsAndShowsTheUser()
    {
        StaThread.Run(() =>
        {
            using var form = new LockScreenForm();
            ILockScreenView view = form;
            view.Title = "Lưu ký – Căn tin";
            view.ShowUser("NL", "Nguyễn Thị Lan");

            MnemonicAssert.HasUniqueMnemonics(form);
            form.AcceptButton.ShouldNotBeNull();
            form.FormBorderStyle.ShouldBe(FormBorderStyle.None);
        });
    }

    // The overlay's cover/disable behaviour touches Application.OpenForms, which is process-global and shared with
    // every other test running in parallel, so it is verified by the manual check instead of a unit test.

    [Fact]
    public void MainForm_WithTheNavigation_HasUniqueMnemonics()
    {
        StaThread.Run(() =>
        {
            using var form = new MainForm();
            IMainView view = form;
            view.ShowNavigation(Navigation());
            view.ShowHome("Chào buổi sáng, lan.nt", "Thứ Sáu, 02/10/2026");

            MnemonicAssert.HasUniqueMnemonics(form);
        });
    }

    [Fact]
    public void MainForm_ShowPage_CreatesEachScreenOnceAndKeepsItWhileSwitching()
    {
        StaThread.Run(() =>
        {
            // Visible reads back the effective visibility, so the form has to be shown (invisibly) for the check.
            using var form = new MainForm { WindowState = FormWindowState.Normal, Opacity = 0, ShowInTaskbar = false };
            form.Show();
            IContentHost host = form;
            var created = 0;
            var page = new UserControl();
            Control Create()
            {
                created++;
                return page;
            }

            host.ShowPage("custody.list", "Chứng từ", Create);
            ((IMainView)form).ShowHome("Chào buổi sáng, lan.nt", "Thứ Sáu, 02/10/2026");
            page.Visible.ShouldBeFalse();

            host.ShowPage("custody.list", "Chứng từ", Create);

            created.ShouldBe(1);
            page.Visible.ShouldBeTrue();
            page.IsDisposed.ShouldBeFalse();
        });
    }

    [Fact]
    public void MainForm_F2_RaisesTheDepositReceiptItem()
    {
        StaThread.Run(() =>
        {
            using var form = new TestableMainForm();
            ((IMainView)form).ShowNavigation(Navigation());
            NavItem? requested = null;
            ((IMainView)form).NavigationRequested += (_, item) => requested = item;

            form.PressKey(Keys.F2).ShouldBeTrue();

            requested!.Key.ShouldBe(ShellNavigation.DepositReceiptKey);
        });
    }

    [Fact]
    public void Navigator_ShowPage_ForwardsToTheContentHost()
    {
        var host = Substitute.For<IContentHost>();
        Func<Control> create = () => throw new InvalidOperationException("not called by the navigator");

        new Navigator(Substitute.For<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>(), host, Substitute.For<ICurrentUser>())
            .ShowPage("k", "Tiêu đề", create);

        host.Received(1).ShowPage("k", "Tiêu đề", create);
    }

    [Theory]
    [InlineData(ShellNavigation.OfficersKey, "Danh mục cán bộ", typeof(OfficerForm))]
    [InlineData(ShellNavigation.DepositReceiptKey, "Lập biên nhận thu", typeof(DepositReceiptForm))]
    [InlineData(ShellNavigation.RolesKey, "Vai trò và phân quyền", typeof(RoleForm))]
    [InlineData(ShellNavigation.FacilityInfoKey, "Thông tin đơn vị", typeof(FacilityInfoForm))]
    [InlineData(ShellNavigation.SignatoryConfigurationKey, "Cấu hình người ký", typeof(SignatoryConfigurationForm))]
    public void Navigator_ModuleScreens_AreHostedInTheContentArea(string key, string title, Type screenType)
    {
        StaThread.Run(() =>
        {
            var host = Substitute.For<IContentHost>();
            Func<Control>? create = null;
            host.ShowPage(key, title, Arg.Do<Func<Control>>(f => create = f));
            var navigator = new Navigator(
                Substitute.For<Microsoft.Extensions.DependencyInjection.IServiceScopeFactory>(), host, Substitute.For<ICurrentUser>());

            Action open = key switch
            {
                ShellNavigation.OfficersKey => navigator.OpenOfficers,
                ShellNavigation.DepositReceiptKey => navigator.OpenDepositReceipt,
                ShellNavigation.FacilityInfoKey => navigator.OpenFacilityInfo,
                ShellNavigation.SignatoryConfigurationKey => navigator.OpenSignatoryConfiguration,
                _ => navigator.OpenRoles,
            };
            open();

            using var screen = create.ShouldNotBeNull()();
            screen.ShouldBeOfType(screenType);
            screen.ShouldBeAssignableTo<UserControl>();
        });
    }

    private sealed class TestableMainForm : MainForm
    {
        public bool PressKey(Keys keys)
        {
            var message = new Message();
            return ProcessCmdKey(ref message, keys);
        }
    }
}
