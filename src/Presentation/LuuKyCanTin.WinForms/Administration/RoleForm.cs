using LuuKyCanTin.Application.Administration;

namespace LuuKyCanTin.WinForms.Administration;

public partial class RoleForm : Form, IRoleView
{
    private readonly List<string> _actions = [.. PermissionCodes.Actions];
    private readonly List<PermissionCodes.PermissionDefinition> _specialPermissions = [.. PermissionCodes.All.Where(PermissionCodes.IsSpecial)];

    public RoleForm()
    {
        InitializeComponent();
        lstRoles.DisplayMember = nameof(RoleDto.Name);
        lstRoles.SelectedIndexChanged += (_, _) => RoleChanged?.Invoke(this, EventArgs.Empty);
        btnSave.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        btnCancel.Click += (_, _) => DiscardClicked?.Invoke(this, EventArgs.Empty);
        BuildGrid();
    }

    public event EventHandler? Loaded;

    public event EventHandler? RoleChanged;

    public event EventHandler? SaveClicked;

    public event EventHandler? DiscardClicked;

    public int? SelectedRoleId => (lstRoles.SelectedItem as RoleDto)?.Id;

    public IReadOnlyList<string> SelectedPermissions
    {
        get
        {
            // A ticked cell is not committed until the cell loses focus.
            grdPermissions.CommitEdit(DataGridViewDataErrorContexts.Commit);

            var result = new List<string>();
            foreach (DataGridViewRow row in grdPermissions.Rows)
            {
                var module = (string)row.Tag!;
                for (var i = 0; i < _actions.Count; i++)
                {
                    if (row.Cells[i + 1].Value is true)
                        result.Add($"{module}.{_actions[i]}");
                }
            }

            foreach (var index in lstSpecialPermissions.CheckedIndices)
                result.Add(_specialPermissions[(int)index].Code);

            return result;
        }
    }

    public void ShowRoles(IReadOnlyList<RoleDto> items)
    {
        var selectedId = SelectedRoleId;
        lstRoles.DataSource = null;
        lstRoles.DataSource = items.ToList();

        if (selectedId is { } id && items.Any(v => v.Id == id))
            lstRoles.SelectedItem = items.First(v => v.Id == id);
        else if (items.Count > 0)
            lstRoles.SelectedIndex = 0;
    }

    public void ShowPermissions(IReadOnlyList<string> permissionCode)
    {
        var granted = permissionCode.ToHashSet(StringComparer.Ordinal);
        foreach (DataGridViewRow row in grdPermissions.Rows)
        {
            var module = (string)row.Tag!;
            for (var i = 0; i < _actions.Count; i++)
                row.Cells[i + 1].Value = granted.Contains($"{module}.{_actions[i]}");
        }

        for (var i = 0; i < _specialPermissions.Count; i++)
            lstSpecialPermissions.SetItemChecked(i, granted.Contains(_specialPermissions[i].Code));
    }

    public void ShowMessage(string message)
    {
        lblMessage.ForeColor = Color.ForestGreen;
        lblMessage.Text = message;
    }

    public void ShowError(string message)
    {
        lblMessage.ForeColor = Color.Firebrick;
        lblMessage.Text = message;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    private void BuildGrid()
    {
        grdPermissions.Columns.Clear();
        grdPermissions.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colNhomQuyen",
            HeaderText = "Nhóm quyền",
            ReadOnly = true,
            FillWeight = 30F,
        });
        foreach (var action in _actions)
        {
            grdPermissions.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = "col" + action,
                HeaderText = action,
                FillWeight = 11F,
            });
        }

        foreach (var module in PermissionCodes.Modules)
        {
            var index = grdPermissions.Rows.Add();
            var row = grdPermissions.Rows[index];
            row.Tag = module.Code;
            row.Cells[0].Value = module.Name;
        }

        // Special permissions arrive with later stories; the list fills itself from the catalogue.
        foreach (var permission in _specialPermissions)
            lstSpecialPermissions.Items.Add(permission.Name);
        lblSpecialPermissions.Visible = _specialPermissions.Count > 0;
        lstSpecialPermissions.Visible = _specialPermissions.Count > 0;
    }
}
