using System.Reflection;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.MasterData;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.MasterData;

/// <summary>The staff register and the two edit dialogs, built for real on an STA thread.</summary>
public class MasterDataScreenTests
{
    private static T Find<T>(Control root, string name)
        where T : Control =>
        (T)root.Controls.Find(name, searchAllChildren: true).Single();

    [Fact]
    public void OfficerForm_Mnemonics_AreUnique()
    {
        StaThread.Run(() =>
        {
            using var screen = new OfficerForm();

            MnemonicAssert.HasUniqueMnemonics(screen);
        });
    }

    [Fact]
    public void OfficerForm_Insert_RaisesAdd()
    {
        StaThread.Run(() =>
        {
            using var screen = new TestableOfficerForm();
            var addCount = 0;
            ((IOfficerView)screen).AddClicked += (_, _) => addCount++;

            screen.PressKey(Keys.Insert).ShouldBeTrue();

            addCount.ShouldBe(1);
        });
    }

    [Fact]
    public void OfficerForm_ReadOnlyByPermission_DisablesAddEditAndInsert()
    {
        StaThread.Run(() =>
        {
            using var screen = new TestableOfficerForm();
            IOfficerView view = screen;
            var addCount = 0;
            view.AddClicked += (_, _) => addCount++;

            view.SetEditingEnabled(canAdd: false, canEdit: false);

            Find<Button>(screen, "btnAdd").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnEdit").Enabled.ShouldBeFalse();
            screen.PressKey(Keys.Insert).ShouldBeFalse();
            addCount.ShouldBe(0);
        });
    }

    [Fact]
    public void OfficerForm_ReadOnlyByPermission_EnterAndDoubleClickDoNotOpenTheEditor()
    {
        StaThread.Run(() =>
        {
            using var host = new Form { Opacity = 0, ShowInTaskbar = false };
            var screen = new TestableOfficerForm();
            host.Controls.Add(screen);
            host.Show();
            IOfficerView view = screen;
            var editCount = 0;
            view.EditClicked += (_, _) => editCount++;
            var grid = Find<DataGridView>(screen, "grdOfficers");

            view.SetEditingEnabled(canAdd: false, canEdit: false);

            // Enter on the grid.
            grid.Focus();
            grid.Focused.ShouldBeTrue();
            screen.PressKey(Keys.Enter).ShouldBeFalse();

            // Double-click a row.
            grid.Rows.Add();
            typeof(DataGridView)
                .GetMethod("OnCellDoubleClick", BindingFlags.NonPublic | BindingFlags.Instance)!
                .Invoke(grid, [new DataGridViewCellEventArgs(0, 0)]);

            editCount.ShouldBe(0);
        });
    }

    [Fact]
    public void OfficerForm_StatusFilter_AllShowsInactiveStaff()
    {
        StaThread.Run(() =>
        {
            using var screen = new OfficerForm();
            IOfficerView view = screen;
            var filter = Find<ComboBox>(screen, "cboStatusFilter");
            var searches = 0;
            view.SearchChanged += (_, _) => searches++;

            view.ShowInactive.ShouldBeFalse();
            filter.Text.ShouldBe("Đang công tác");

            filter.SelectedIndex = 1;

            view.ShowInactive.ShouldBeTrue();
            searches.ShouldBe(1);
        });
    }

    [Theory]
    [InlineData(typeof(OfficerEditForm), 480)]
    [InlineData(typeof(AddInmateForm), 560)]
    public void EditDialogs_FollowTheDialogPattern(Type dialogType, int width)
    {
        StaThread.Run(() =>
        {
            using var dialog = (Form)Activator.CreateInstance(dialogType)!;
            dialog.CreateControl();

            MnemonicAssert.HasUniqueMnemonics(dialog);
            dialog.FormBorderStyle.ShouldBe(FormBorderStyle.FixedDialog);
            dialog.StartPosition.ShouldBe(FormStartPosition.CenterParent);
            dialog.ClientSize.Width.ShouldBe(width);
            ((Button)dialog.AcceptButton!).Text.ShouldBe("&Lưu");
            ((Button)dialog.CancelButton!).Text.ShouldBe("&Hủy");
        });
    }

    [Fact]
    public void OfficerEditForm_FieldErrors_MarkEachFieldAndFocusTheFirstOnScreen()
    {
        StaThread.Run(() =>
        {
            using var dialog = new OfficerEditForm { Opacity = 0, ShowInTaskbar = false };
            dialog.Show();
            IOfficerEditView view = dialog;

            view.ShowFieldErrors(
            [
                new(OfficerField.FullName, "Họ tên không được để trống."),
                new(OfficerField.OfficerCode, "Mã cán bộ đã tồn tại"),
            ]);

            Find<InputFrame>(dialog, "frmOfficerCode").HasError.ShouldBeTrue();
            Find<InputFrame>(dialog, "frmFullName").HasError.ShouldBeTrue();
            Find<InputFrame>(dialog, "frmPosition").HasError.ShouldBeFalse();
            Find<TextBox>(dialog, "txtOfficerCode").Focused.ShouldBeTrue();
        });
    }

    [Fact]
    public void OfficerEditForm_TypingInAnInvalidField_ClearsItsError()
    {
        StaThread.Run(() =>
        {
            using var dialog = new OfficerEditForm();
            IOfficerEditView view = dialog;
            view.ShowFieldErrors([new(OfficerField.OfficerCode, "Mã cán bộ đã tồn tại")]);

            view.OfficerCode = "CB02";

            Find<InputFrame>(dialog, "frmOfficerCode").HasError.ShouldBeFalse();
        });
    }

    [Fact]
    public void OfficerEditForm_AnErrorOfNoField_ShowsTheBanner()
    {
        StaThread.Run(() =>
        {
            using var dialog = new OfficerEditForm();

            ((IOfficerEditView)dialog).ShowError("Dữ liệu đã bị người khác thay đổi, vui lòng tải lại");

            Find<Banner>(dialog, "bnrError").Message.ShouldBe("Dữ liệu đã bị người khác thay đổi, vui lòng tải lại");
        });
    }

    [Fact]
    public void OfficerEditForm_Heading_ShowsTitleAndSubtitle()
    {
        StaThread.Run(() =>
        {
            using var dialog = new OfficerEditForm();
            IOfficerEditView view = dialog;

            view.Title = "Sửa cán bộ";
            view.ShowHeading("Lê Thị Bình", "CB05 · Kế toán");

            dialog.Text.ShouldBe("Sửa cán bộ");
            dialog.Controls.Find("pnlHead", true).Single().Controls.Cast<Control>().Select(c => c.Text)
                .ShouldBe(["Lê Thị Bình", "CB05 · Kế toán"]);
        });
    }

    [Fact]
    public void AddInmateForm_DuplicateCode_IsShownUnderTheCode()
    {
        StaThread.Run(() =>
        {
            using var dialog = new AddInmateForm();

            ((IAddInmateView)dialog).ShowFieldErrors([new(InmateField.InmateCode, "Mã số đã tồn tại.")]);

            Find<InputFrame>(dialog, "frmInmateCode").HasError.ShouldBeTrue();
            Find<Button>(dialog, "btnSave").Enabled.ShouldBeTrue();
        });
    }

    private sealed class TestableOfficerForm : OfficerForm
    {
        public bool PressKey(Keys keys)
        {
            var message = new Message();
            return ProcessCmdKey(ref message, keys);
        }
    }
}
