using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>
/// The signer-configuration screen, hosted in the shell's content area: the print-template list on the left, the
/// signer grid, tools and printed-order preview on the right. Follows the role screen's master-detail pattern.
/// </summary>
public partial class SignatoryConfigurationForm : UserControl, ISignatoryConfigurationView
{
    private const int NoOfficerId = 0;

    private IReadOnlyList<OfficerDto> _activeOfficers = [];
    private Dictionary<int, string> _retiredNames = [];
    private bool _canEdit;
    private bool _isBindingTemplates;
    private bool _isBindingLines;

    public SignatoryConfigurationForm()
    {
        InitializeComponent();
        colTemplateName.DataPropertyName = nameof(TemplateCodes.TemplateDefinition.DisplayName);
        colOrdinal.ReadOnly = true;
        colOfficer.ValueMember = nameof(OfficerChoice.Id);
        colOfficer.DisplayMember = nameof(OfficerChoice.DisplayName);

        grdTemplates.CurrentCellChanged += (_, _) =>
        {
            if (_isBindingTemplates)
                return;

            crdSignatories.HeaderText = SelectedTemplate?.DisplayName ?? "Người ký";
            TemplateChanged?.Invoke(this, EventArgs.Empty);
        };
        // A cell edit (combo pick or typed text) is only committed on focus loss; commit it as soon as the cell
        // turns dirty, so CellValueChanged fires and the preview strip follows every change live.
        grdLines.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (grdLines.IsCurrentCellDirty)
                grdLines.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        grdLines.CellValueChanged += (_, _) => UpdatePreview();

        btnAdd.Click += (_, _) => AddLineClicked?.Invoke(this, EventArgs.Empty);
        btnRemove.Click += (_, _) => RemoveLineClicked?.Invoke(this, EventArgs.Empty);
        btnUp.Click += (_, _) => MoveUpClicked?.Invoke(this, EventArgs.Empty);
        btnDown.Click += (_, _) => MoveDownClicked?.Invoke(this, EventArgs.Empty);
        btnSave.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        btnDiscard.Click += (_, _) => DiscardClicked?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? Loaded;

    public event EventHandler? TemplateChanged;

    public event EventHandler? AddLineClicked;

    public event EventHandler? RemoveLineClicked;

    public event EventHandler? MoveUpClicked;

    public event EventHandler? MoveDownClicked;

    public event EventHandler? SaveClicked;

    public event EventHandler? DiscardClicked;

    public string? SelectedTemplateCode => SelectedTemplate?.Code;

    public int? SelectedLineIndex => grdLines.CurrentRow?.Index;

    public IReadOnlyList<SignatoryLine> Lines
    {
        get
        {
            // A typed cell is not committed until the cell loses focus.
            grdLines.CommitEdit(DataGridViewDataErrorContexts.Commit);
            grdLines.EndEdit();

            return grdLines.Rows.Cast<DataGridViewRow>()
                .Select(row => new SignatoryLine(
                    (row.Cells[colTitle.Index].Value as string ?? "").Trim(),
                    row.Cells[colOfficer.Index].Value is int officerId && officerId != NoOfficerId ? officerId : null))
                .ToList();
        }
    }

    private TemplateCodes.TemplateDefinition? SelectedTemplate =>
        grdTemplates.CurrentRow?.DataBoundItem as TemplateCodes.TemplateDefinition;

    public void ShowTemplates(IReadOnlyList<TemplateCodes.TemplateDefinition> templates)
    {
        _isBindingTemplates = true;
        try
        {
            grdTemplates.DataSource = templates.ToList();
            if (grdTemplates.Rows.Count > 0)
                grdTemplates.CurrentCell = grdTemplates.Rows[0].Cells[0];
        }
        finally
        {
            _isBindingTemplates = false;
        }

        crdSignatories.HeaderText = SelectedTemplate?.DisplayName ?? "Người ký";
    }

    public void ShowSignatories(IReadOnlyList<SignatoryRowDto> rows, IReadOnlyList<OfficerDto> activeOfficers)
    {
        _activeOfficers = activeOfficers;
        // A default who has left still shows, so the admin can keep or replace that name.
        _retiredNames = rows
            .Where(row => row.OfficerId is not null && !row.IsOfficerActive)
            .GroupBy(row => row.OfficerId!.Value)
            .ToDictionary(group => group.Key, group => group.First().OfficerFullName ?? "");
        BindLines(rows.Select(row => new SignatoryLine(row.Title, row.OfficerId)).ToList(), selectedIndex: rows.Count > 0 ? 0 : -1);
    }

    public void ShowLines(IReadOnlyList<SignatoryLine> lines, int selectedIndex) => BindLines(lines, selectedIndex);

    public void SetEditingEnabled(bool canEdit)
    {
        _canEdit = canEdit;
        grdLines.ReadOnly = !canEdit;
        btnAdd.Enabled = canEdit;
        btnRemove.Enabled = canEdit;
        btnUp.Enabled = canEdit;
        btnDown.Enabled = canEdit;
        btnSave.Enabled = canEdit;
        btnDiscard.Enabled = canEdit;
    }

    public void SetSaveEnabled(bool isEnabled) => btnSave.Enabled = isEnabled && _canEdit;

    public void ShowMessage(string message)
    {
        bnrMessage.Kind = BannerKind.Warning;
        bnrMessage.Message = message;
    }

    public void ShowError(string message)
    {
        bnrMessage.Kind = BannerKind.Error;
        bnrMessage.Message = message;
    }

    // A cached screen loads once, the first time the shell shows it.
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    private void BindLines(IReadOnlyList<SignatoryLine> lines, int selectedIndex)
    {
        _isBindingLines = true;
        try
        {
            grdLines.Rows.Clear();
            colOfficer.DataSource = BuildOfficerChoices();
            for (var i = 0; i < lines.Count; i++)
            {
                var rowIndex = grdLines.Rows.Add();
                var row = grdLines.Rows[rowIndex];
                row.Cells[colOrdinal.Index].Value = i + 1;
                row.Cells[colTitle.Index].Value = lines[i].Title;
                row.Cells[colOfficer.Index].Value = lines[i].OfficerId ?? NoOfficerId;
            }

            if (selectedIndex >= 0 && selectedIndex < grdLines.Rows.Count)
                grdLines.CurrentCell = grdLines.Rows[selectedIndex].Cells[colTitle.Index];
        }
        finally
        {
            _isBindingLines = false;
        }

        UpdatePreview();
    }

    // "(để trống)", the active staff, then every retired default of the loaded snapshot. The snapshot stays in the
    // list even after a line is replaced or removed, so the admin can still pick that person back.
    private List<OfficerChoice> BuildOfficerChoices()
    {
        var choices = new List<OfficerChoice> { new(NoOfficerId, "(để trống)") };
        foreach (var officer in _activeOfficers)
            choices.Add(new OfficerChoice(officer.Id, officer.FullName));

        foreach (var (officerId, name) in _retiredNames)
            choices.Add(new OfficerChoice(officerId, name.Length > 0 ? $"{name} (đã nghỉ)" : "(đã nghỉ)"));

        return choices;
    }

    /// <summary>The live preview strip: one column per signer, title on top and default name under it.</summary>
    private void UpdatePreview()
    {
        if (_isBindingLines)
            return;

        flpSignature.SuspendLayout();
        while (flpSignature.Controls.Count > 0)
        {
            var control = flpSignature.Controls[0];
            flpSignature.Controls.RemoveAt(0);
            control.Dispose();
        }

        foreach (var line in Lines)
        {
            var stack = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 1,
                Margin = new Padding(0, 0, AppTheme.Gap, 0),
                RowCount = 2,
                TabStop = false,
            };
            stack.Controls.Add(new Label
            {
                AutoSize = true,
                Font = AppTheme.LabelFont,
                ForeColor = AppTheme.Label,
                Margin = Padding.Empty,
                Text = line.Title.Length > 0 ? line.Title : "(chưa có chức danh)",
                UseMnemonic = false,
            }, 0, 0);
            stack.Controls.Add(new Label
            {
                AutoSize = true,
                ForeColor = AppTheme.Text,
                Margin = new Padding(0, 2, 0, 0),
                Text = OfficerDisplayName(line.OfficerId) ?? "",
                UseMnemonic = false,
            }, 0, 1);
            flpSignature.Controls.Add(stack);
        }

        flpSignature.ResumeLayout();
    }

    private string? OfficerDisplayName(int? officerId)
    {
        if (officerId is not { } id)
            return null;

        var active = _activeOfficers.FirstOrDefault(officer => officer.Id == id);
        if (active is not null)
            return active.FullName;

        var name = _retiredNames.GetValueOrDefault(id, "");
        return name.Length > 0 ? $"{name} (đã nghỉ)" : "(đã nghỉ)";
    }

    private sealed record OfficerChoice(int Id, string DisplayName);
}
