using LuuKyCanTin.WinForms.Common;

namespace LuuKyCanTin.WinForms.Shell;

partial class LockScreenForm
{
    private const int CardWidth = 380;
    private const int CardPadding = 28;
    private const int FieldWidth = CardWidth - 2 * CardPadding;
    private const int HeadHeight = 116;

    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.Timer tmrWatch = null!;
    private Label lblAppTitle = null!;
    private FlowLayoutPanel pnlCard = null!;
    private Panel pnlHead = null!;
    private Label lblInitials = null!;
    private Label lblName = null!;
    private FlowLayoutPanel pnlHint = null!;
    private Label lblHintGlyph = null!;
    private Label lblHint = null!;
    private Banner bnrError = null!;
    private Label lblPassword = null!;
    private InputFrame frmPassword = null!;
    private TextBox txtPassword = null!;
    private Button btnUnlock = null!;
    private LinkLabel lnkSignOut = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        tmrWatch = new System.Windows.Forms.Timer(components);
        lblAppTitle = new Label();
        pnlCard = new FlowLayoutPanel();
        pnlHead = new Panel();
        lblInitials = new Label();
        lblName = new Label();
        pnlHint = new FlowLayoutPanel();
        lblHintGlyph = new Label();
        lblHint = new Label();
        bnrError = new Banner();
        lblPassword = new Label();
        frmPassword = new InputFrame();
        txtPassword = new TextBox();
        btnUnlock = new Button();
        lnkSignOut = new LinkLabel();
        pnlCard.SuspendLayout();
        pnlHead.SuspendLayout();
        pnlHint.SuspendLayout();
        SuspendLayout();

        // The title bar of the covered shell is hidden too, so the overlay carries the app title itself.
        lblAppTitle.AutoSize = true;
        lblAppTitle.Font = AppTheme.BodySemiboldFont;
        lblAppTitle.ForeColor = AppTheme.OnAccent;
        lblAppTitle.Location = new Point(16, 10);
        lblAppTitle.Name = "lblAppTitle";
        lblAppTitle.Text = "Lưu ký – Căn tin";
        lblAppTitle.UseMnemonic = false;

        // The white card is a top-down flow: hiding the error banner collapses its space and keeps the prototype gaps.
        pnlCard.AutoSize = true;
        pnlCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        pnlCard.BackColor = AppTheme.Card;
        pnlCard.FlowDirection = FlowDirection.TopDown;
        pnlCard.Margin = Padding.Empty;
        pnlCard.Name = "pnlCard";
        pnlCard.Padding = new Padding(CardPadding);
        pnlCard.WrapContents = false;
        pnlCard.Controls.Add(pnlHead);
        pnlCard.Controls.Add(bnrError);
        pnlCard.Controls.Add(lblPassword);
        pnlCard.Controls.Add(frmPassword);
        pnlCard.Controls.Add(btnUnlock);
        pnlCard.Controls.Add(lnkSignOut);

        // Avatar, name and the locked hint, centred inside a fixed-width header block.
        pnlHead.Margin = Padding.Empty;
        pnlHead.Name = "pnlHead";
        pnlHead.Size = new Size(FieldWidth, HeadHeight);
        pnlHead.Controls.Add(lblInitials);
        pnlHead.Controls.Add(lblName);
        pnlHead.Controls.Add(pnlHint);

        lblInitials.BackColor = AppTheme.Accent;
        lblInitials.Font = AppTheme.PageTitleFont;
        lblInitials.ForeColor = AppTheme.OnAccent;
        lblInitials.Location = new Point((FieldWidth - 56) / 2, 0);
        lblInitials.Name = "lblInitials";
        lblInitials.Size = new Size(56, 56);
        lblInitials.TextAlign = ContentAlignment.MiddleCenter;
        lblInitials.UseMnemonic = false;

        lblName.AutoEllipsis = true;
        lblName.AutoSize = false;
        lblName.Font = AppTheme.PageTitleFont;
        lblName.ForeColor = AppTheme.Text;
        lblName.Location = new Point(0, 62);
        lblName.Name = "lblName";
        lblName.Size = new Size(FieldWidth, 26);
        lblName.TextAlign = ContentAlignment.MiddleCenter;
        lblName.UseMnemonic = false;

        pnlHint.AutoSize = true;
        pnlHint.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        pnlHint.Location = new Point(0, 92);
        pnlHint.Margin = Padding.Empty;
        pnlHint.Name = "pnlHint";
        pnlHint.WrapContents = false;
        pnlHint.Controls.Add(lblHintGlyph);
        pnlHint.Controls.Add(lblHint);

        lblHintGlyph.AutoSize = true;
        lblHintGlyph.Font = AppTheme.IconFont(10.5F);
        lblHintGlyph.ForeColor = AppTheme.Muted;
        lblHintGlyph.Margin = new Padding(0, 3, 6, 0);
        lblHintGlyph.Name = "lblHintGlyph";
        lblHintGlyph.Text = Glyphs.Lock;
        lblHintGlyph.UseMnemonic = false;

        lblHint.AutoSize = true;
        lblHint.ForeColor = AppTheme.Muted;
        lblHint.Margin = Padding.Empty;
        lblHint.Name = "lblHint";
        lblHint.Text = "Phiên làm việc đã bị khoá";
        lblHint.UseMnemonic = false;

        bnrError.Margin = new Padding(0, 14, 0, 0);
        bnrError.Name = "bnrError";
        bnrError.Width = FieldWidth;

        lblPassword.AutoSize = true;
        lblPassword.Font = AppTheme.LabelFont;
        lblPassword.ForeColor = AppTheme.Label;
        lblPassword.Margin = new Padding(0, 14, 0, 5);
        lblPassword.Name = "lblPassword";
        lblPassword.Text = "Mật khẩu";
        lblPassword.UseMnemonic = false;

        frmPassword.Margin = new Padding(0, 0, 0, 14);
        frmPassword.Name = "frmPassword";
        frmPassword.Size = new Size(FieldWidth, AppTheme.InputHeight);
        frmPassword.TabIndex = 0;
        txtPassword.Name = "txtPassword";
        txtPassword.PlaceholderText = "Nhập mật khẩu";
        txtPassword.TabIndex = 0;
        txtPassword.UseSystemPasswordChar = true;
        frmPassword.Inner = txtPassword;
        frmPassword.Glyph = Glyphs.Lock;

        btnUnlock.Margin = new Padding(0, 0, 0, 14);
        btnUnlock.Name = "btnUnlock";
        btnUnlock.Size = new Size(FieldWidth, AppTheme.ButtonHeightLarge);
        btnUnlock.TabIndex = 1;
        btnUnlock.Text = "&Mở khoá";
        btnUnlock.Click += OnUnlockClicked;
        AppTheme.StylePrimary(btnUnlock);

        lnkSignOut.ActiveLinkColor = AppTheme.AccentHover;
        lnkSignOut.AutoSize = false;
        lnkSignOut.LinkColor = AppTheme.Accent;
        lnkSignOut.Margin = Padding.Empty;
        lnkSignOut.Name = "lnkSignOut";
        lnkSignOut.Size = new Size(FieldWidth, 20);
        lnkSignOut.TabIndex = 2;
        lnkSignOut.Text = "Đăng xuất";
        lnkSignOut.TextAlign = ContentAlignment.MiddleCenter;
        lnkSignOut.UseMnemonic = false;
        lnkSignOut.VisitedLinkColor = AppTheme.Accent;
        lnkSignOut.LinkClicked += OnSignOutClicked;

        AcceptButton = btnUnlock;
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = AppTheme.Side;
        ClientSize = new Size(800, 520);
        Controls.Add(pnlCard);
        Controls.Add(lblAppTitle);
        Font = AppTheme.BodyFont;
        FormBorderStyle = FormBorderStyle.None;
        Name = "LockScreenForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.Manual;
        Text = "Lưu ký – Căn tin";
        // Re-disables a form that opens after the lock; there is no OpenForms-changed event to hook.
        tmrWatch.Interval = 500;

        pnlHint.ResumeLayout(false);
        pnlHint.PerformLayout();
        pnlHead.ResumeLayout(false);
        pnlCard.ResumeLayout(false);
        pnlCard.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
