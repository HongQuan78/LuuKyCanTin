using LuuKyCanTin.Application.MasterData;

namespace LuuKyCanTin.WinForms.MasterData;

public partial class OfficerForm : Form, IOfficerView
{
    public OfficerForm()
    {
        InitializeComponent();
        colOfficerCode.DataPropertyName = nameof(OfficerDto.OfficerCode);
        colFullName.DataPropertyName = nameof(OfficerDto.FullName);
        colPosition.DataPropertyName = nameof(OfficerDto.Position);
        colIsSupervisingOfficer.DataPropertyName = nameof(OfficerDto.IsSupervisingOfficer);
        colIsActive.DataPropertyName = nameof(OfficerDto.IsActive);

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
        chkShowInactive.CheckedChanged += (_, _) => SearchChanged?.Invoke(this, EventArgs.Empty);
        btnAdd.Click += (_, _) => AddClicked?.Invoke(this, EventArgs.Empty);
        btnEdit.Click += (_, _) => EditClicked?.Invoke(this, EventArgs.Empty);
        grdOfficers.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0)
                EditClicked?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? Loaded;

    public event EventHandler? SearchChanged;

    public event EventHandler? AddClicked;

    public event EventHandler? EditClicked;

    public string Keyword => txtKeyword.Text;

    public bool ShowInactive => chkShowInactive.Checked;

    public OfficerDto? SelectedOfficer => grdOfficers.CurrentRow?.DataBoundItem as OfficerDto;

    public void ShowList(IReadOnlyList<OfficerDto> items)
    {
        grdOfficers.DataSource = items.ToList();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }
}
