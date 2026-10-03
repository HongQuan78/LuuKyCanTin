using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The role screen, hosted in the shell's content area: the role list card on the left, the permission matrix on the right.</summary>
public partial class RoleForm : UserControl, IRoleView
{
    private readonly List<string> _actions = [.. PermissionCodes.Actions];
    private readonly List<PermissionCodes.PermissionDefinition> _specialPermissions = [.. PermissionCodes.All.Where(PermissionCodes.IsSpecial)];
    private bool _isBindingRoles;
    private int? _shownRoleId;

    public RoleForm()
    {
        InitializeComponent();
        colRoleName.DataPropertyName = nameof(RoleDto.Name);
        grdRoles.SelectionChanged += (_, _) => OnRoleSelectionChanged();
        btnSave.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        btnCancel.Click += (_, _) => DiscardClicked?.Invoke(this, EventArgs.Empty);
        BuildGrid();
        ActiveControl = grdRoles;
    }

    public event EventHandler? Loaded;

    public event EventHandler? RoleChanged;

    public event EventHandler? SaveClicked;

    public event EventHandler? DiscardClicked;

    public int? SelectedRoleId => SelectedRole?.Id;

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

    private RoleDto? SelectedRole => grdRoles.CurrentRow?.DataBoundItem as RoleDto;

    public void ShowRoles(IReadOnlyList<RoleDto> items)
    {
        var selectedId = SelectedRoleId;
        _isBindingRoles = true;
        try
        {
            grdRoles.DataSource = items.ToList();
            var index = selectedId is { } id ? items.ToList().FindIndex(v => v.Id == id) : -1;
            if (index < 0 && items.Count > 0)
                index = 0;
            if (index >= 0 && index < grdRoles.Rows.Count)
                grdRoles.CurrentCell = grdRoles.Rows[index].Cells[0];
        }
        finally
        {
            _isBindingRoles = false;
        }

        OnRoleSelectionChanged();
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

    // Rebinding the list moves the selection several times; the presenter only hears about a different role.
    private void OnRoleSelectionChanged()
    {
        if (_isBindingRoles)
            return;

        var role = SelectedRole;
        crdPermissions.HeaderText = role?.Name ?? " ";
        if (role?.Id == _shownRoleId)
            return;

        _shownRoleId = role?.Id;
        RoleChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BuildGrid()
    {
        grdPermissions.Columns.Clear();
        grdPermissions.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "colPermissionGroup",
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
        pnlSpecialPermissions.Visible = _specialPermissions.Count > 0;
    }
}
