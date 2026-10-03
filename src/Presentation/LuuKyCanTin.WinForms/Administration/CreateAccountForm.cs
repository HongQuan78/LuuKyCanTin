using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

public partial class CreateAccountForm : Form, ICreateAccountView
{
    private readonly FieldErrorDisplay<CreateAccountField> _fieldErrors = new();

    public CreateAccountForm()
    {
        InitializeComponent();
        _fieldErrors.Add(CreateAccountField.UserName, frmUserName, errUserName);
        _fieldErrors.Add(CreateAccountField.Officer, frmOfficer, errOfficer);
        btnSave.Click += (_, _) =>
        {
            // The new attempt replaces the old messages; the presenter shows whatever is still wrong.
            _fieldErrors.ClearAll();
            bnrError.Message = "";
            CreateClicked?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? Loaded;

    public event EventHandler? CreateClicked;

    public string UserName => txtUserName.Text;

    public int? OfficerId => (cboOfficer.SelectedItem as OfficerItem)?.Officer.Id;

    public IReadOnlyList<int> SelectedRoleIds => [.. lstRoles.CheckedItems.Cast<RoleItem>().Select(item => item.Role.Id)];

    public void ShowOfficers(IReadOnlyList<OfficerDto> items)
    {
        cboOfficer.Items.Clear();
        foreach (var officer in items)
            cboOfficer.Items.Add(new OfficerItem(officer));
        if (cboOfficer.Items.Count > 0)
            cboOfficer.SelectedIndex = 0;
    }

    public void ShowRoles(IReadOnlyList<RoleDto> items)
    {
        lstRoles.Items.Clear();
        foreach (var role in items)
            lstRoles.Items.Add(new RoleItem(role));
    }

    public bool ShowModal() => ShowDialog() == DialogResult.OK;

    public void ShowFieldErrors(IReadOnlyList<FieldMessage<CreateAccountField>> errors) => _fieldErrors.Show(errors);

    public void ShowError(string message) => bnrError.Message = message;

    public void CloseAsSaved() => DialogResult = DialogResult.OK;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    // The combo shows "Họ tên (Mã)" as the old screen did; the item keeps the DTO for its id.
    private sealed record OfficerItem(OfficerDto Officer)
    {
        public override string ToString() => $"{Officer.FullName} ({Officer.OfficerCode})";
    }

    private sealed record RoleItem(RoleDto Role)
    {
        public override string ToString() => Role.Name;
    }
}
