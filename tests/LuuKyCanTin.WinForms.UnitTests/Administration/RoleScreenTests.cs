using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

/// <summary>The role screen and the print preview, built for real on an STA thread.</summary>
public class RoleScreenTests
{
    private static T Find<T>(Control root, string name)
        where T : Control =>
        (T)root.Controls.Find(name, searchAllChildren: true).Single();

    [Fact]
    public void RoleForm_Mnemonics_AreUnique()
    {
        StaThread.Run(() =>
        {
            using var screen = new RoleForm();

            MnemonicAssert.HasUniqueMnemonics(screen);
        });
    }

    [Fact]
    public void RoleForm_Messages_UseTheBannerInsteadOfColouredText()
    {
        StaThread.Run(() =>
        {
            using var screen = new RoleForm();
            IRoleView view = screen;
            var banner = Find<Banner>(screen, "bnrMessage");

            view.ShowMessage("Đã lưu quyền của vai trò.");
            banner.Kind.ShouldBe(BannerKind.Warning);
            banner.Message.ShouldBe("Đã lưu quyền của vai trò.");

            view.ShowError("Bạn không có quyền thực hiện thao tác này");
            banner.Kind.ShouldBe(BannerKind.Error);

            view.ShowMessage("");
            banner.Message.ShouldBe("");
        });
    }

    [Fact]
    public void RoleForm_ReadOnlyByPermission_DisablesSaveCancelAndTheMatrix()
    {
        StaThread.Run(() =>
        {
            using var screen = new RoleForm();
            IRoleView view = screen;

            view.SetEditingEnabled(canEdit: false);

            Find<Button>(screen, "btnSave").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnCancel").Enabled.ShouldBeFalse();
            Find<DataGridView>(screen, "grdPermissions").ReadOnly.ShouldBeTrue();
        });
    }

    [Fact]
    public void RoleForm_ShowRoles_SelectsTheFirstAndHeadsTheMatrixWithItsName()
    {
        StaThread.Run(() =>
        {
            using var host = new Form();
            var screen = new RoleForm();
            host.Controls.Add(screen);
            IRoleView view = screen;
            var changes = 0;
            view.RoleChanged += (_, _) => changes++;

            view.ShowRoles([new RoleDto(1, RoleCodes.Administrator, "Quản trị hệ thống", [1])]);
            view.ShowRoles([new RoleDto(1, RoleCodes.Administrator, "Quản trị hệ thống", [2])]);

            view.SelectedRoleId.ShouldBe(1);
            changes.ShouldBe(1);
            Find<CardPanel>(screen, "crdPermissions").HeaderText.ShouldBe("Quản trị hệ thống");
        });
    }

    [Fact]
    public void PdfPreviewForm_Toolbar_HasOnePrimaryPrintButtonAndUniqueMnemonics()
    {
        StaThread.Run(() =>
        {
            using var preview = new PdfPreviewForm([0x25, 0x50, 0x44, 0x46], "test");

            MnemonicAssert.HasUniqueMnemonics(preview);
            var print = Find<Button>(preview, "btnPrint");
            var save = Find<Button>(preview, "btnSave");
            print.Text.ShouldBe("&In ra máy in…");
            print.Font.ShouldBeSameAs(AppTheme.BodySemiboldFont);
            save.Text.ShouldBe("&Lưu thành tệp PDF…");
            save.Font.ShouldBeSameAs(AppTheme.BodyFont);
            // Both wait for the preview to load.
            print.Enabled.ShouldBeFalse();
            save.Enabled.ShouldBeFalse();
        });
    }
}
