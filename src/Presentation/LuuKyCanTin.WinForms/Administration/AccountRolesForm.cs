using System.ComponentModel;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

public partial class AccountRolesForm : Form, IAccountRolesView
{
    public AccountRolesForm()
    {
        InitializeComponent();
        btnSave.Click += (_, _) =>
        {
            bnrError.Message = "";
            SaveClicked?.Invoke(this, EventArgs.Empty);
        };
    }

    public event EventHandler? Loaded;

    public event EventHandler? SaveClicked;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        set => Text = value;
    }

    public IReadOnlyList<int> SelectedRoleIds => [.. lstRoles.CheckedItems.Cast<RoleItem>().Select(item => item.Role.Id)];

    public void ShowRoles(IReadOnlyList<RoleDto> items)
    {
        lstRoles.Items.Clear();
        foreach (var role in items)
            lstRoles.Items.Add(new RoleItem(role));
    }

    public void ShowSelectedRoles(IReadOnlyList<int> roleIds)
    {
        var selected = roleIds.ToHashSet();
        for (var i = 0; i < lstRoles.Items.Count; i++)
            lstRoles.SetItemChecked(i, selected.Contains(((RoleItem)lstRoles.Items[i]).Role.Id));
    }

    public bool ShowModal() => ShowDialog() == DialogResult.OK;

    public void ShowError(string message) => bnrError.Message = message;

    public void CloseAsSaved() => DialogResult = DialogResult.OK;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    private sealed record RoleItem(RoleDto Role)
    {
        public override string ToString() => Role.Name;
    }
}
