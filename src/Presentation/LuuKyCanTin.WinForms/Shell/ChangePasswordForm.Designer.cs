using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

partial class ChangePasswordForm
{
    private const int DialogWidth = 480;
    private const int FieldWidth = DialogWidth - 2 * AppTheme.DialogPadding;

    private System.ComponentModel.IContainer components = null!;
    private TableLayoutPanel pnlRoot = null!;
    private FlowLayoutPanel pnlHead = null!;
    private Label lblHeading = null!;
    private Label lblSubtitle = null!;
    private FlowLayoutPanel pnlBody = null!;
    private Banner bnrError = null!;
    private Label lblCurrentPassword = null!;
    private InputFrame frmCurrentPassword = null!;
    private TextBox txtCurrentPassword = null!;
    private FieldError errCurrentPassword = null!;
    private Label lblNewPassword = null!;
    private InputFrame frmNewPassword = null!;
    private TextBox txtNewPassword = null!;
    private FieldError errNewPassword = null!;
    private Label lblConfirmation = null!;
    private InputFrame frmConfirmation = null!;
    private TextBox txtConfirmation = null!;
    private FieldError errConfirmation = null!;
    private FlowLayoutPanel pnlRules = null!;
    private FlowLayoutPanel pnlFooter = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        pnlRoot = new TableLayoutPanel();
        pnlHead = new FlowLayoutPanel();
        lblHeading = new Label();
        lblSubtitle = new Label();
        pnlBody = new FlowLayoutPanel();
        bnrError = new Banner();
        lblCurrentPassword = new Label();
        frmCurrentPassword = new InputFrame();
        txtCurrentPassword = new TextBox();
        errCurrentPassword = new FieldError();
        lblNewPassword = new Label();
        frmNewPassword = new InputFrame();
        txtNewPassword = new TextBox();
        errNewPassword = new FieldError();
        lblConfirmation = new Label();
        frmConfirmation = new InputFrame();
        txtConfirmation = new TextBox();
        errConfirmation = new FieldError();
        pnlRules = new FlowLayoutPanel();
        pnlFooter = new FlowLayoutPanel();
        btnSave = new Button();
        btnCancel = new Button();
        pnlRoot.SuspendLayout();
        pnlHead.SuspendLayout();
        pnlBody.SuspendLayout();
        pnlFooter.SuspendLayout();
        SuspendLayout();

        // Head, body and footer stacked; the dialog sizes itself to them, so an error under a field never clips.
        pnlRoot.AutoSize = true;
        pnlRoot.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        pnlRoot.ColumnCount = 1;
        pnlRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, DialogWidth));
        pnlRoot.RowCount = 3;
        pnlRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pnlRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pnlRoot.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        pnlRoot.Location = Point.Empty;
        pnlRoot.Margin = Padding.Empty;
        pnlRoot.Controls.Add(pnlHead, 0, 0);
        pnlRoot.Controls.Add(pnlBody, 0, 1);
        pnlRoot.Controls.Add(pnlFooter, 0, 2);

        pnlHead.AutoSize = true;
        pnlHead.FlowDirection = FlowDirection.TopDown;
        pnlHead.WrapContents = false;
        pnlHead.Margin = Padding.Empty;
        pnlHead.Padding = new Padding(AppTheme.DialogPadding, 18, AppTheme.DialogPadding, 4);
        pnlHead.Controls.Add(lblHeading);
        pnlHead.Controls.Add(lblSubtitle);

        lblHeading.AutoSize = true;
        lblHeading.Font = AppTheme.DialogTitleFont;
        lblHeading.ForeColor = AppTheme.Text;
        lblHeading.Margin = Padding.Empty;
        lblHeading.Text = "Đặt mật khẩu mới";
        lblHeading.UseMnemonic = false;

        lblSubtitle.AutoSize = true;
        lblSubtitle.ForeColor = AppTheme.Muted;
        lblSubtitle.Margin = new Padding(0, 4, 0, 0);
        lblSubtitle.MaximumSize = new Size(FieldWidth, 0);
        lblSubtitle.UseMnemonic = false;

        pnlBody.AutoSize = true;
        pnlBody.FlowDirection = FlowDirection.TopDown;
        pnlBody.WrapContents = false;
        pnlBody.Margin = Padding.Empty;
        pnlBody.Padding = new Padding(AppTheme.DialogPadding, AppTheme.Gap, AppTheme.DialogPadding, AppTheme.Gap);
        pnlBody.Controls.Add(bnrError);
        txtCurrentPassword.Name = "txtCurrentPassword";
        txtNewPassword.Name = "txtNewPassword";
        txtConfirmation.Name = "txtConfirmation";
        AddField(lblCurrentPassword, "Mật khẩu hiện &tại", frmCurrentPassword, txtCurrentPassword, errCurrentPassword, 0, isFirst: true);
        AddField(lblNewPassword, "Mật khẩu &mới", frmNewPassword, txtNewPassword, errNewPassword, 2, isFirst: false);
        AddField(lblConfirmation, "Xác &nhận mật khẩu mới", frmConfirmation, txtConfirmation, errConfirmation, 4, isFirst: false);
        pnlBody.Controls.Add(pnlRules);

        bnrError.Margin = new Padding(0, 0, 0, 14);
        bnrError.Name = "bnrError";
        bnrError.Width = FieldWidth;

        pnlRules.AutoSize = true;
        pnlRules.FlowDirection = FlowDirection.TopDown;
        pnlRules.WrapContents = false;
        pnlRules.Margin = new Padding(0, 14, 0, 0);

        // Footer band: secondary then primary, right-aligned; RightToLeft flow puts the first added at the right.
        pnlFooter.AutoSize = true;
        pnlFooter.BackColor = AppTheme.Subtle;
        pnlFooter.Dock = DockStyle.Fill;
        pnlFooter.FlowDirection = FlowDirection.RightToLeft;
        pnlFooter.Margin = Padding.Empty;
        pnlFooter.Padding = new Padding(AppTheme.DialogPadding, 12, AppTheme.DialogPadding - AppTheme.GapSmall, 12);
        pnlFooter.Controls.Add(btnSave);
        pnlFooter.Controls.Add(btnCancel);
        pnlFooter.Paint += (_, e) =>
        {
            using var line = new SolidBrush(AppTheme.Border);
            e.Graphics.FillRectangle(line, 0, 0, pnlFooter.Width, 1);
        };

        btnSave.AutoSize = true;
        btnSave.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnSave.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnSave.Name = "btnSave";
        btnSave.TabIndex = 7;
        btnSave.Text = "&Lưu";
        AppTheme.StylePrimary(btnSave);

        btnCancel.AutoSize = true;
        btnCancel.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        btnCancel.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        btnCancel.Name = "btnCancel";
        btnCancel.TabIndex = 6;
        btnCancel.Text = CancelText;
        AppTheme.StyleSecondary(btnCancel);

        AcceptButton = btnSave;
        CancelButton = btnCancel;
        AutoScaleMode = AutoScaleMode.Font;
        AutoSize = true;
        AutoSizeMode = AutoSizeMode.GrowAndShrink;
        BackColor = AppTheme.Card;
        Controls.Add(pnlRoot);
        Font = AppTheme.BodyFont;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ChangePasswordForm";
        ShowIcon = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Đổi mật khẩu";
        pnlFooter.ResumeLayout(false);
        pnlFooter.PerformLayout();
        pnlBody.ResumeLayout(false);
        pnlBody.PerformLayout();
        pnlHead.ResumeLayout(false);
        pnlHead.PerformLayout();
        pnlRoot.ResumeLayout(false);
        pnlRoot.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    /// <summary>One field: the label above, the framed password box, and the error line under it.</summary>
    private void AddField(Label label, string caption, InputFrame frame, TextBox textBox, FieldError error, int tabIndex, bool isFirst)
    {
        label.AutoSize = true;
        label.Font = AppTheme.LabelFont;
        label.ForeColor = AppTheme.Label;
        label.Margin = new Padding(0, isFirst ? 0 : 14, 0, AppTheme.LabelGap);
        label.TabIndex = tabIndex;
        label.Text = caption;

        textBox.UseSystemPasswordChar = true;
        textBox.TabIndex = 0;
        frame.Margin = Padding.Empty;
        frame.Width = FieldWidth;
        frame.TabIndex = tabIndex + 1;
        frame.Inner = textBox;

        error.MessageWidth = FieldWidth;

        pnlBody.Controls.Add(label);
        pnlBody.Controls.Add(frame);
        pnlBody.Controls.Add(error);
    }
}
