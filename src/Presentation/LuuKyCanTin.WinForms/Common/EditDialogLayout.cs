namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// Builds the edit dialog of key-02 B: a head (title and muted subtitle), a body of 2 columns with labels above framed
/// fields (16px column gap, 14px rows, padding 20) and a subtle footer band with the buttons right-aligned, primary last.
/// The dialog sizes itself to its content, so an error under a field never clips.
/// </summary>
internal sealed class EditDialogLayout
{
    private const int RowGap = 14;

    private readonly Form _dialog;
    private readonly int _columnWidth;

    /// <param name="width">The dialog's client width: 560, or 480 for a small dialog.</param>
    public EditDialogLayout(Form dialog, int width)
    {
        _dialog = dialog;
        _columnWidth = (width - 2 * AppTheme.DialogPadding - AppTheme.Gap) / 2;
        FullWidth = 2 * _columnWidth + AppTheme.Gap;

        Root = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 3,
            Margin = Padding.Empty,
            Name = "pnlRoot",
        };
        Root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, width));
        for (var i = 0; i < 3; i++)
            Root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Head = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = Padding.Empty,
            Padding = new Padding(AppTheme.DialogPadding, 18, AppTheme.DialogPadding, 4),
            Name = "pnlHead",
        };

        Body = new TableLayoutPanel
        {
            AutoSize = true,
            ColumnCount = 2,
            Margin = Padding.Empty,
            // The last row keeps its 14px bottom margin, so the body pads 20 below in total.
            Padding = new Padding(AppTheme.DialogPadding, AppTheme.Gap, AppTheme.DialogPadding, AppTheme.DialogPadding - RowGap),
            Name = "pnlBody",
        };
        Body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, _columnWidth + AppTheme.Gap));
        Body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, _columnWidth));

        Footer = new FlowLayoutPanel
        {
            AutoSize = true,
            BackColor = AppTheme.Subtle,
            Dock = DockStyle.Fill,
            // RightToLeft flow: the first button added sits at the right, so add the primary first.
            FlowDirection = FlowDirection.RightToLeft,
            Margin = Padding.Empty,
            Padding = new Padding(AppTheme.DialogPadding, 12, AppTheme.DialogPadding - AppTheme.GapSmall, 12),
            Name = "pnlFooter",
        };
        Footer.Paint += (_, e) =>
        {
            using var line = new SolidBrush(AppTheme.Border);
            e.Graphics.FillRectangle(line, 0, 0, Footer.Width, 1);
        };

        Head.TabIndex = 0;
        Body.TabIndex = 1;
        Footer.TabIndex = 2;
        Root.Controls.Add(Head, 0, 0);
        Root.Controls.Add(Body, 0, 1);
        Root.Controls.Add(Footer, 0, 2);
    }

    public TableLayoutPanel Root { get; }

    public FlowLayoutPanel Head { get; }

    public TableLayoutPanel Body { get; }

    public FlowLayoutPanel Footer { get; }

    /// <summary>The width of a field that spans both columns.</summary>
    public int FullWidth { get; }

    public void SetHeading(Label heading, Label subtitle)
    {
        heading.AutoSize = true;
        heading.Font = AppTheme.DialogTitleFont;
        heading.ForeColor = AppTheme.Text;
        heading.Margin = Padding.Empty;
        heading.MaximumSize = new Size(FullWidth, 0);
        heading.UseMnemonic = false;

        subtitle.AutoSize = true;
        subtitle.ForeColor = AppTheme.Muted;
        subtitle.Margin = new Padding(0, 4, 0, 0);
        subtitle.MaximumSize = new Size(FullWidth, 0);
        subtitle.UseMnemonic = false;

        Head.Controls.Add(heading);
        Head.Controls.Add(subtitle);
    }

    /// <summary>The form-level error banner, across both columns above the first field row.</summary>
    public void AddBanner(Banner banner, int row)
    {
        banner.Margin = new Padding(0, 0, 0, RowGap);
        banner.Width = FullWidth;
        Body.Controls.Add(banner, 0, row);
        Body.SetColumnSpan(banner, 2);
    }

    /// <summary>One field: the label above, the framed input, and the error line under it, in one grid cell.</summary>
    public void AddField(Label label, string caption, InputFrame frame, Control inner, FieldError error, int column, int row, bool isWide = false)
    {
        var width = isWide ? FullWidth : _columnWidth;

        label.AutoSize = true;
        label.Font = AppTheme.LabelFont;
        label.ForeColor = AppTheme.Label;
        label.Margin = new Padding(0, 0, 0, AppTheme.LabelGap);
        label.TabIndex = 0;
        label.Text = caption;

        frame.Margin = Padding.Empty;
        frame.Width = width;
        frame.TabIndex = 1;
        frame.Inner = inner;

        error.MessageWidth = width;

        var cell = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 0, column == 0 && !isWide ? AppTheme.Gap : 0, RowGap),
            Name = "cell" + inner.Name,
            TabIndex = row * 2 + column,
        };
        cell.Controls.Add(label);
        cell.Controls.Add(frame);
        cell.Controls.Add(error);
        Body.Controls.Add(cell, column, row);
        if (isWide)
            Body.SetColumnSpan(cell, 2);
    }

    /// <summary>
    /// A multi-line list in one cell: the label above and the control itself, without an input frame around it.
    /// </summary>
    public void AddListField(Label label, string caption, Control list, int column, int row, bool isWide = false)
    {
        var width = isWide ? FullWidth : _columnWidth;

        label.AutoSize = true;
        label.Font = AppTheme.LabelFont;
        label.ForeColor = AppTheme.Label;
        label.Margin = new Padding(0, 0, 0, AppTheme.LabelGap);
        label.TabIndex = 0;
        label.Text = caption;

        list.Margin = Padding.Empty;
        list.Width = width;
        list.TabIndex = 1;

        var cell = new FlowLayoutPanel
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 0, column == 0 && !isWide ? AppTheme.Gap : 0, RowGap),
            Name = "cell" + list.Name,
            TabIndex = row * 2 + column,
        };
        cell.Controls.Add(label);
        cell.Controls.Add(list);
        Body.Controls.Add(cell, column, row);
        if (isWide)
            Body.SetColumnSpan(cell, 2);
    }

    /// <summary>A control in a cell without a label above it, such as a check box, aligned with the inputs.</summary>
    public void AddPlain(Control control, int column, int row)
    {
        control.Margin = new Padding(0, 0, column == 0 ? AppTheme.Gap : 0, RowGap);
        control.TabIndex = row * 2 + column;
        Body.Controls.Add(control, column, row);
    }

    /// <summary>The footer buttons from right to left: the primary first, then the secondary ones.</summary>
    public void AddButtons(Button primary, params Button[] secondary)
    {
        var tabIndex = secondary.Length;
        StyleButton(primary, tabIndex--);
        AppTheme.StylePrimary(primary);
        Footer.Controls.Add(primary);
        foreach (var button in secondary)
        {
            StyleButton(button, tabIndex--);
            AppTheme.StyleSecondary(button);
            Footer.Controls.Add(button);
        }
    }

    /// <summary>Applies the dialog chrome and adds the layout; call it last, between Suspend/ResumeLayout.</summary>
    public void ApplyTo(string text)
    {
        _dialog.AutoScaleMode = AutoScaleMode.Font;
        _dialog.AutoSize = true;
        _dialog.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _dialog.BackColor = AppTheme.Card;
        _dialog.Font = AppTheme.BodyFont;
        _dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
        _dialog.MaximizeBox = false;
        _dialog.MinimizeBox = false;
        _dialog.ShowIcon = false;
        _dialog.ShowInTaskbar = false;
        _dialog.StartPosition = FormStartPosition.CenterParent;
        _dialog.Text = text;
        _dialog.Controls.Add(Root);
    }

    private static void StyleButton(Button button, int tabIndex)
    {
        button.AutoSize = true;
        button.Margin = new Padding(0, 0, AppTheme.GapSmall, 0);
        button.MinimumSize = new Size(88, AppTheme.ButtonHeight);
        button.TabIndex = tabIndex;
    }
}
