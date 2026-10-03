using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

/// <summary>The unit-information screen, built for real on an STA thread.</summary>
public class FacilityInfoScreenTests
{
    private static T Find<T>(Control root, string name)
        where T : Control =>
        (T)root.Controls.Find(name, searchAllChildren: true).Single();

    [Fact]
    public void FacilityInfoForm_Mnemonics_AreUnique()
    {
        StaThread.Run(() =>
        {
            using var screen = new FacilityInfoForm();

            MnemonicAssert.HasUniqueMnemonics(screen);
        });
    }

    [Fact]
    public void FacilityInfoForm_ReadOnlyByPermission_MakesTheBoxesReadOnlyAndHidesTheButtons()
    {
        StaThread.Run(() =>
        {
            using var screen = new FacilityInfoForm();
            IFacilityInfoView view = screen;

            view.SetEditingEnabled(canEdit: false);

            Find<TextBox>(screen, "txtParentAgency").ReadOnly.ShouldBeTrue();
            Find<TextBox>(screen, "txtFacilityName").ReadOnly.ShouldBeTrue();
            Find<TextBox>(screen, "txtAddress").ReadOnly.ShouldBeTrue();
            Find<Button>(screen, "btnSave").Visible.ShouldBeFalse();
            Find<Button>(screen, "btnReset").Visible.ShouldBeFalse();

            view.SetEditingEnabled(canEdit: true);

            Find<TextBox>(screen, "txtFacilityName").ReadOnly.ShouldBeFalse();
            Find<Button>(screen, "btnSave").Visible.ShouldBeTrue();
        });
    }

    [Fact]
    public void FacilityInfoForm_SetSaveEnabled_TogglesOnlyTheSaveButton()
    {
        StaThread.Run(() =>
        {
            using var screen = new FacilityInfoForm();
            IFacilityInfoView view = screen;

            view.SetSaveEnabled(false);
            Find<Button>(screen, "btnSave").Enabled.ShouldBeFalse();

            view.SetSaveEnabled(true);
            Find<Button>(screen, "btnSave").Enabled.ShouldBeTrue();
        });
    }

    [Fact]
    public void FacilityInfoForm_ShowFacility_FillsTheBoxesAndThePrintPreview()
    {
        StaThread.Run(() =>
        {
            using var screen = new FacilityInfoForm();
            IFacilityInfoView view = screen;

            view.ShowFacility(new FacilityInfoDto(
                "Công an tỉnh ABC", "Trại tạm giam ABC", "Xã ABC, huyện ABC", IsConfigured: true, [1]));

            Find<TextBox>(screen, "txtParentAgency").Text.ShouldBe("Công an tỉnh ABC");
            Find<TextBox>(screen, "txtFacilityName").Text.ShouldBe("Trại tạm giam ABC");
            Find<Label>(screen, "lblPreview").Text.ShouldBe(
                "CÔNG AN TỈNH ABC" + Environment.NewLine + "Trại tạm giam ABC" + Environment.NewLine + "Địa chỉ: Xã ABC, huyện ABC");
        });
    }

    [Fact]
    public void FacilityInfoForm_WithoutAgency_PreviewStartsWithTheName()
    {
        StaThread.Run(() =>
        {
            using var screen = new FacilityInfoForm();
            IFacilityInfoView view = screen;

            view.ShowFacility(new FacilityInfoDto(null, "Trại tạm giam ABC", "Xã ABC", IsConfigured: true, [1]));

            Find<Label>(screen, "lblPreview").Text.ShouldBe(
                "Trại tạm giam ABC" + Environment.NewLine + "Địa chỉ: Xã ABC");
        });
    }

    [Fact]
    public void FacilityInfoForm_TypingInABox_UpdatesThePreview()
    {
        StaThread.Run(() =>
        {
            using var screen = new FacilityInfoForm();
            IFacilityInfoView view = screen;
            view.ShowFacility(new FacilityInfoDto(null, "Trại tạm giam ABC", "Xã ABC", IsConfigured: true, [1]));

            Find<TextBox>(screen, "txtParentAgency").Text = "Công an tỉnh ABC";
            Find<TextBox>(screen, "txtAddress").Text = "Xã Mới";

            Find<Label>(screen, "lblPreview").Text.ShouldBe(
                "CÔNG AN TỈNH ABC" + Environment.NewLine + "Trại tạm giam ABC" + Environment.NewLine + "Địa chỉ: Xã Mới");
        });
    }

    [Fact]
    public void FacilityInfoForm_FieldErrors_MarkEachFieldAndFocusTheFirstOnScreen()
    {
        StaThread.Run(() =>
        {
            using var host = new Form { Opacity = 0, ShowInTaskbar = false };
            var screen = new FacilityInfoForm();
            host.Controls.Add(screen);
            host.Show();
            IFacilityInfoView view = screen;

            view.ShowFieldErrors(
            [
                new(FacilityInfoField.Address, SaveFacilityInfoRequestValidator.AddressRequiredMessage),
                new(FacilityInfoField.FacilityName, SaveFacilityInfoRequestValidator.FacilityNameRequiredMessage),
            ]);

            Find<InputFrame>(screen, "frmFacilityName").HasError.ShouldBeTrue();
            Find<InputFrame>(screen, "frmAddress").HasError.ShouldBeTrue();
            Find<InputFrame>(screen, "frmParentAgency").HasError.ShouldBeFalse();
            Find<TextBox>(screen, "txtFacilityName").Focused.ShouldBeTrue();
        });
    }

    [Fact]
    public void FacilityInfoForm_Messages_UseTheBanner()
    {
        StaThread.Run(() =>
        {
            using var screen = new FacilityInfoForm();
            IFacilityInfoView view = screen;
            var banner = Find<Banner>(screen, "bnrMessage");

            view.ShowMessage("Đã lưu thông tin đơn vị.");
            banner.Kind.ShouldBe(BannerKind.Warning);
            banner.Message.ShouldBe("Đã lưu thông tin đơn vị.");

            view.ShowError("Bạn không có quyền thực hiện thao tác này");
            banner.Kind.ShouldBe(BannerKind.Error);

            view.ShowMessage("");
            banner.Message.ShouldBe("");
        });
    }
}
