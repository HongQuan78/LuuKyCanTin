using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.WinForms.Administration;
using LuuKyCanTin.WinForms.Common;
using LuuKyCanTin.WinForms.UnitTests.TestUtilities;
using Shouldly;

namespace LuuKyCanTin.WinForms.UnitTests.Administration;

/// <summary>The signer-configuration screen, built for real on an STA thread.</summary>
public class SignatoryConfigurationScreenTests
{
    private static T Find<T>(Control root, string name)
        where T : Control =>
        (T)root.Controls.Find(name, searchAllChildren: true).Single();

    private static List<string> PreviewTexts(Control root) =>
        Find<FlowLayoutPanel>(root, "flpSignature").Controls.Cast<Control>()
            .SelectMany(stack => stack.Controls.Cast<Control>())
            .Select(control => control.Text)
            .ToList();

    [Fact]
    public void Form_Mnemonics_AreUnique()
    {
        StaThread.Run(() =>
        {
            using var screen = new SignatoryConfigurationForm();

            MnemonicAssert.HasUniqueMnemonics(screen);
        });
    }

    [Fact]
    public void Form_ShowTemplates_SelectsTheFirstAndRaisesAChangeOnlyOnAUserPick()
    {
        StaThread.Run(() =>
        {
            using var host = new Form();
            var screen = new SignatoryConfigurationForm();
            host.Controls.Add(screen);
            ISignatoryConfigurationView view = screen;
            var changes = 0;
            view.TemplateChanged += (_, _) => changes++;

            // Binding and re-binding must stay silent: the presenter loads the first template itself.
            view.ShowTemplates(TemplateCodes.All);
            view.ShowTemplates(TemplateCodes.All);
            changes.ShouldBe(0);
            view.SelectedTemplateCode.ShouldBe(TemplateCodes.DepositReceipt);

            Find<DataGridView>(screen, "grdTemplates").CurrentCell =
                Find<DataGridView>(screen, "grdTemplates").Rows[1].Cells[0];

            changes.ShouldBe(1);
            view.SelectedTemplateCode.ShouldBe(TemplateCodes.Payout);
            Find<CardPanel>(screen, "crdSignatories").HeaderText.ShouldBe("Phiếu chi xuất tiền lưu ký");
        });
    }

    [Fact]
    public void Form_ShowSignatories_BindsTheTitlesAndKeepsARetiredDefault()
    {
        StaThread.Run(() =>
        {
            using var host = new Form();
            var screen = new SignatoryConfigurationForm();
            host.Controls.Add(screen);
            ISignatoryConfigurationView view = screen;

            view.ShowSignatories(
            [
                new SignatoryRowDto(1, "Cán bộ căn tin", 7, "Trần Văn Cũ", IsOfficerActive: false),
                new SignatoryRowDto(2, "Lãnh đạo đơn vị", null, null, IsOfficerActive: false),
            ],
            [new OfficerDto(3, "CB001", "Nguyễn Văn Mới", "Cán bộ", IsSupervisingOfficer: false, IsActive: true, [1])]);

            var lines = view.Lines;
            lines.Count.ShouldBe(2);
            lines[0].Title.ShouldBe("Cán bộ căn tin");
            lines[0].OfficerId.ShouldBe(7);
            lines[1].OfficerId.ShouldBeNull();
            view.SelectedLineIndex.ShouldBe(0);
        });
    }

    [Fact]
    public void Form_TypingATitle_UpdatesThePreviewStrip()
    {
        StaThread.Run(() =>
        {
            using var host = new Form { Opacity = 0, ShowInTaskbar = false };
            var screen = new SignatoryConfigurationForm();
            host.Controls.Add(screen);
            host.Show();
            ISignatoryConfigurationView view = screen;
            view.ShowSignatories([new SignatoryRowDto(1, "Cán bộ căn tin", null, null, IsOfficerActive: false)], []);

            var grid = Find<DataGridView>(screen, "grdLines");
            grid.CurrentCell = grid.Rows[0].Cells[grid.Columns["colTitle"]!.Index];
            grid.BeginEdit(true);
            ((TextBox)grid.EditingControl!).Text = "Cán bộ mới";

            PreviewTexts(screen).ShouldContain("Cán bộ mới");
        });
    }

    [Fact]
    public void Form_ShowSignatories_RendersThePreviewStripAndKeepsTheRetiredChoice()
    {
        StaThread.Run(() =>
        {
            using var host = new Form();
            var screen = new SignatoryConfigurationForm();
            host.Controls.Add(screen);
            ISignatoryConfigurationView view = screen;

            view.ShowSignatories(
            [
                new SignatoryRowDto(1, "Cán bộ căn tin", 7, "Trần Văn Cũ", IsOfficerActive: false),
                new SignatoryRowDto(2, "Lãnh đạo đơn vị", null, null, IsOfficerActive: false),
            ],
            [new OfficerDto(3, "CB001", "Nguyễn Văn Mới", "Cán bộ", IsSupervisingOfficer: false, IsActive: true, [1])]);

            // The strip shows the printed order: title on top, default name under it.
            var texts = PreviewTexts(screen);
            texts.ShouldContain("Cán bộ căn tin");
            texts.ShouldContain("Trần Văn Cũ (đã nghỉ)");
            texts.ShouldContain("Lãnh đạo đơn vị");

            // Replacing the retired default and re-binding keeps it selectable for this template.
            view.ShowLines([new SignatoryLine("Cán bộ căn tin", 3), new SignatoryLine("Lãnh đạo đơn vị")], 0);

            var grid = Find<DataGridView>(screen, "grdLines");
            var officerColumn = grid.Columns.OfType<DataGridViewComboBoxColumn>().Single();
            var choices = ((System.Collections.IEnumerable)officerColumn.DataSource!)
                .Cast<object>()
                .Select(choice => choice.ToString()!)
                .ToList();
            choices.ShouldContain(text => text.Contains("Trần Văn Cũ (đã nghỉ)"));
        });
    }

    [Fact]
    public void Form_ReadOnlyByPermission_DisablesTheWriteButtonsAndTheGrid()
    {
        StaThread.Run(() =>
        {
            using var screen = new SignatoryConfigurationForm();
            ISignatoryConfigurationView view = screen;

            view.SetEditingEnabled(canEdit: false);

            Find<DataGridView>(screen, "grdLines").ReadOnly.ShouldBeTrue();
            Find<Button>(screen, "btnAdd").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnRemove").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnUp").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnDown").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnSave").Enabled.ShouldBeFalse();
            Find<Button>(screen, "btnDiscard").Enabled.ShouldBeFalse();

            view.SetEditingEnabled(canEdit: true);

            Find<DataGridView>(screen, "grdLines").ReadOnly.ShouldBeFalse();
            Find<Button>(screen, "btnSave").Enabled.ShouldBeTrue();
        });
    }
}
