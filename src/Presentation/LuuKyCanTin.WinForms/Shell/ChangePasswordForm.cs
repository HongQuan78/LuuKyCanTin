using System.ComponentModel;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

public partial class ChangePasswordForm : Form, IChangePasswordView
{
    private const string CancelText = "&Hủy";
    private const string SignOutText = "Đăng &xuất";

    private readonly Dictionary<PasswordRule, (Label Glyph, Label Text)> _ruleRows = [];

    public ChangePasswordForm()
    {
        InitializeComponent();
        BuildRuleRows();
        btnSave.Click += (_, _) => SaveClicked?.Invoke(this, EventArgs.Empty);
        btnCancel.Click += (_, _) => CancelClicked?.Invoke(this, EventArgs.Empty);
        WatchInput(txtCurrentPassword, frmCurrentPassword, errCurrentPassword);
        WatchInput(txtNewPassword, frmNewPassword, errNewPassword);
        WatchInput(txtConfirmation, frmConfirmation, errConfirmation);
    }

    public event EventHandler? SaveClicked;

    public event EventHandler? CancelClicked;

    public event EventHandler? InputChanged;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Title
    {
        set => Text = value;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsForced
    {
        set
        {
            lblSubtitle.Text = value
                ? "Đây là lần đăng nhập đầu tiên, vui lòng đổi mật khẩu trước khi tiếp tục."
                : "Ít nhất 8 ký tự, gồm chữ in hoa, chữ thường và chữ số.";
            btnCancel.Text = value ? SignOutText : CancelText;
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string CurrentPassword => txtCurrentPassword.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string NewPassword => txtNewPassword.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Confirmation => txtConfirmation.Text;

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool CanSave
    {
        set => btnSave.Enabled = value;
    }

    public void ShowRuleResults(IReadOnlyList<PasswordRuleResult> results)
    {
        foreach (var result in results)
        {
            var (glyph, text) = _ruleRows[result.Rule];
            glyph.Text = result.IsSatisfied ? Glyphs.CheckMark : Glyphs.Cancel;
            glyph.ForeColor = text.ForeColor = result.IsSatisfied ? AppTheme.Success : AppTheme.Placeholder;
        }
    }

    public void ShowFieldError(PasswordField field, string message)
    {
        var (frame, textBox, error) = field switch
        {
            PasswordField.Current => (frmCurrentPassword, txtCurrentPassword, errCurrentPassword),
            PasswordField.New => (frmNewPassword, txtNewPassword, errNewPassword),
            PasswordField.Confirmation => (frmConfirmation, txtConfirmation, errConfirmation),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown password field."),
        };
        frame.HasError = true;
        error.Message = message;
        textBox.SelectAll();
        textBox.Focus();
    }

    public void ShowError(string message)
    {
        bnrError.Message = message;
        txtCurrentPassword.SelectAll();
        txtCurrentPassword.Focus();
    }

    public void CloseWithResult(bool succeeded)
    {
        DialogResult = succeeded ? DialogResult.OK : DialogResult.Cancel;
        Close();
    }

    public bool ShowModal() => ShowDialog() == DialogResult.OK;

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        txtCurrentPassword.Focus();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        // The X behaves like the secondary button, so a forced change signs out before the dialog really closes.
        // When the presenter closes the dialog it has already set DialogResult, so this branch is skipped.
        if (DialogResult == DialogResult.None && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            CancelClicked?.Invoke(this, EventArgs.Empty);
        }
    }

    // Typing in a field clears its own error: the message described the old value.
    private void WatchInput(TextBox textBox, InputFrame frame, FieldError error) =>
        textBox.TextChanged += (_, _) =>
        {
            frame.HasError = false;
            error.Message = "";
            InputChanged?.Invoke(this, EventArgs.Empty);
        };

    private void BuildRuleRows()
    {
        foreach (var rule in Enum.GetValues<PasswordRule>())
        {
            var glyph = new Label
            {
                AutoSize = true,
                Font = AppTheme.IconFont(9F),
                Margin = new Padding(0, 4, 8, 0),
                UseMnemonic = false,
            };
            var text = new Label
            {
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 3),
                Text = rule.ToDisplayText(),
                UseMnemonic = false,
            };
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = Padding.Empty };
            row.Controls.Add(glyph);
            row.Controls.Add(text);
            pnlRules.Controls.Add(row);
            _ruleRows[rule] = (glyph, text);
        }
    }
}
