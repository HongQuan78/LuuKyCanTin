using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.MasterData;

/// <summary>The staff register, hosted in the shell's content area: the list screen of key-02 A.</summary>
public partial class OfficerForm : UserControl, IOfficerView
{
    private const string ActiveText = "Đang công tác";
    private const string InactiveText = "Đã nghỉ";
    private const int AllStatusesIndex = 1;

    public OfficerForm()
    {
        InitializeComponent();
        colOfficerCode.DataPropertyName = nameof(OfficerDto.OfficerCode);
        colFullName.DataPropertyName = nameof(OfficerDto.FullName);
        colPosition.DataPropertyName = nameof(OfficerDto.Position);
        colIsSupervisingOfficer.DataPropertyName = nameof(OfficerDto.IsSupervisingOfficer);
        ActiveControl = txtKeyword;

        // Restarting the timer on every keystroke searches once typing pauses.
        txtKeyword.TextChanged += (_, _) =>
        {
            tmrSearch.Stop();
            tmrSearch.Start();
        };
        tmrSearch.Tick += (_, _) =>
        {
            tmrSearch.Stop();
            SearchChanged?.Invoke(this, EventArgs.Empty);
        };
        cboStatusFilter.SelectedIndexChanged += (_, _) => SearchChanged?.Invoke(this, EventArgs.Empty);
        btnAdd.Click += (_, _) => AddClicked?.Invoke(this, EventArgs.Empty);
        btnEdit.Click += (_, _) => EditClicked?.Invoke(this, EventArgs.Empty);
        grdOfficers.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                EditClicked?.Invoke(this, EventArgs.Empty);
        };
        grdOfficers.CellFormatting += OnCellFormatting;
        grdOfficers.CellPainting += OnCellPainting;
    }

    public event EventHandler? Loaded;

    public event EventHandler? SearchChanged;

    public event EventHandler? AddClicked;

    public event EventHandler? EditClicked;

    public string Keyword => txtKeyword.Text;

    public bool ShowInactive => cboStatusFilter.SelectedIndex == AllStatusesIndex;

    public OfficerDto? SelectedOfficer => grdOfficers.CurrentRow?.DataBoundItem as OfficerDto;

    public void ShowList(IReadOnlyList<OfficerDto> items)
    {
        grdOfficers.DataSource = items.ToList();
        lblCount.Text = $"{items.Count} cán bộ";
    }

    // A cached screen loads once, the first time the shell shows it.
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    // List keys (EXPERIENCE.md): Insert adds from anywhere on the screen, Enter on the grid edits the current row.
    // The grid would otherwise use Enter to move down a row, so it is taken before the grid sees it.
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Insert)
        {
            AddClicked?.Invoke(this, EventArgs.Empty);
            return true;
        }

        if (keyData == Keys.Enter && grdOfficers.ContainsFocus)
        {
            EditClicked?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private OfficerDto? RowOfficer(int rowIndex) =>
        rowIndex >= 0 ? grdOfficers.Rows[rowIndex].DataBoundItem as OfficerDto : null;

    private void OnCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.ColumnIndex == colStatus.Index && RowOfficer(e.RowIndex) is { } officer)
        {
            e.Value = officer.IsActive ? ActiveText : InactiveText;
            e.FormattingApplied = true;
        }
    }

    private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.ColumnIndex == colStatus.Index && RowOfficer(e.RowIndex) is { } officer)
            AppTheme.PaintStatusCell(e, officer.IsActive ? AppTheme.Success : AppTheme.Danger);
    }
}
