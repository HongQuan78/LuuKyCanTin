using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Administration;

/// <summary>The account screen, hosted in the shell's content area: the list screen of key-02 A.</summary>
public partial class AccountForm : UserControl, IAccountView
{
    private const string ActiveText = "Đang hoạt động";
    private const string InactiveText = "Đã ngừng";
    private const string LockedText = "Đang khoá";

    private bool _canEdit = true;

    public AccountForm()
    {
        InitializeComponent();
        colUserName.DataPropertyName = nameof(AccountDto.UserName);
        colOfficer.DataPropertyName = nameof(AccountDto.OfficerFullName);
        colRoles.DataPropertyName = nameof(AccountDto.RoleNames);
        btnAdd.Click += (_, _) => AddClicked?.Invoke(this, EventArgs.Empty);
        btnRoles.Click += (_, _) => RolesClicked?.Invoke(this, EventArgs.Empty);
        btnToggleActive.Click += (_, _) => ToggleActiveClicked?.Invoke(this, EventArgs.Empty);
        btnUnlock.Click += (_, _) => UnlockClicked?.Invoke(this, EventArgs.Empty);
        btnResetPassword.Click += (_, _) => ResetPasswordClicked?.Invoke(this, EventArgs.Empty);
        grdAccounts.CellDoubleClick += (_, e) =>
        {
            if (e.RowIndex >= 0 && _canEdit)
                RolesClicked?.Invoke(this, EventArgs.Empty);
        };
        grdAccounts.SelectionChanged += (_, _) => UpdateActionCaptions();
        grdAccounts.CellFormatting += OnCellFormatting;
        grdAccounts.CellPainting += OnCellPainting;
        ActiveControl = grdAccounts;
    }

    public event EventHandler? Loaded;

    public event EventHandler? AddClicked;

    public event EventHandler? RolesClicked;

    public event EventHandler? ToggleActiveClicked;

    public event EventHandler? UnlockClicked;

    public event EventHandler? ResetPasswordClicked;

    public AccountDto? SelectedAccount => grdAccounts.CurrentRow?.DataBoundItem as AccountDto;

    public void ShowAccounts(IReadOnlyList<AccountDto> items)
    {
        grdAccounts.DataSource = items.ToList();
        lblCount.Text = $"{items.Count} tài khoản";
        UpdateActionCaptions();
    }

    public void SetEditingEnabled(bool canEdit)
    {
        _canEdit = canEdit;
        btnAdd.Enabled = canEdit;
        btnRoles.Enabled = canEdit;
        btnToggleActive.Enabled = canEdit;
        btnUnlock.Enabled = canEdit;
        btnResetPassword.Enabled = canEdit;
    }

    public bool Confirm(string message) =>
        MessageBox.Show(this, message, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

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

    // List keys (EXPERIENCE.md): Insert adds from anywhere on the screen, Enter on the grid edits the current row.
    // The grid would otherwise use Enter to move down a row, so it is taken before the grid sees it.
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == Keys.Insert && _canEdit)
        {
            AddClicked?.Invoke(this, EventArgs.Empty);
            return true;
        }

        if (keyData == Keys.Enter && grdAccounts.ContainsFocus && _canEdit)
        {
            RolesClicked?.Invoke(this, EventArgs.Empty);
            return true;
        }

        return base.ProcessCmdKey(ref msg, keyData);
    }

    private void UpdateActionCaptions()
    {
        // The toggle action says what it will do, so a deactivated account reads "Kích hoạt lại".
        btnToggleActive.Text = SelectedAccount is { IsActive: false } ? "&Kích hoạt lại" : "&Ngừng hoạt động";
    }

    private AccountDto? RowAccount(int rowIndex) =>
        rowIndex >= 0 ? grdAccounts.Rows[rowIndex].DataBoundItem as AccountDto : null;

    private void OnCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (RowAccount(e.RowIndex) is not { } account)
            return;

        if (e.ColumnIndex == colStatus.Index)
        {
            e.Value = account.IsActive ? ActiveText : InactiveText;
            e.FormattingApplied = true;
        }
        else if (e.ColumnIndex == colLocked.Index)
        {
            e.Value = account.IsLocked ? LockedText : "";
            e.FormattingApplied = true;
        }
    }

    private void OnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (RowAccount(e.RowIndex) is not { } account)
            return;

        if (e.ColumnIndex == colStatus.Index)
            AppTheme.PaintStatusCell(e, account.IsActive ? AppTheme.Success : AppTheme.Danger);
        else if (e.ColumnIndex == colLocked.Index && account.IsLocked)
            AppTheme.PaintStatusCell(e, AppTheme.Warning);
    }
}
