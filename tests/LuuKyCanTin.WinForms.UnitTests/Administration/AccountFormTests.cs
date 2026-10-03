using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

/// <summary>The account screen's permission-driven read-only state.</summary>
public class AccountFormTests
{
    private static T Find<T>(Control root, string name)
        where T : Control =>
        (T)root.Controls.Find(name, searchAllChildren: true).Single();

    [Fact]
    public void AccountForm_ReadOnlyByPermission_DisablesEveryWriteAction()
    {
        StaThread.Run(() =>
        {
            using var screen = new AccountForm();
            IAccountView view = screen;

            view.SetEditingEnabled(canEdit: false);

            Find<Button>(screen, "btnAdd").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnRoles").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnToggleActive").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnUnlock").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnResetPassword").Enabled.ShouldBeFalse();
        });
    }
}
