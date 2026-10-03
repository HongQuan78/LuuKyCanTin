using System.ComponentModel;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// The DESIGN.md input: a 32px frame that hosts one borderless TextBox or ComboBox and an optional leading glyph, and
/// paints the <c>input</c>, <c>input-focus</c>, <c>input-error</c> and <c>input-readonly</c> states.
/// </summary>
internal sealed class InputFrame : Panel
{
    private const int InnerPadding = 10;
    private const int GlyphWidth = 16;
    private const int GlyphGap = 8;

    private readonly Label _glyph = new()
    {
        AutoSize = false,
        Size = new Size(GlyphWidth, AppTheme.InputHeight),
        TextAlign = ContentAlignment.MiddleCenter,
        Font = AppTheme.IconFont(10.5F),
        ForeColor = AppTheme.Muted,
        UseMnemonic = false,
        Visible = false,
    };

    private readonly Label _inlineLabel = new()
    {
        AutoSize = true,
        Font = AppTheme.BodyFont,
        ForeColor = AppTheme.Muted,
        UseMnemonic = false,
        Visible = false,
    };

    private Control? _inner;
    private bool _hasError;
    private bool _isReadOnly;

    public InputFrame()
    {
        SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        Height = AppTheme.InputHeight;
        BackColor = AppTheme.Card;
        Controls.Add(_glyph);
        Controls.Add(_inlineLabel);
        _glyph.Click += (_, _) => _inner?.Focus();
        _inlineLabel.Click += (_, _) => _inner?.Focus();
    }

    /// <summary>The hosted TextBox, ComboBox or NumericUpDown. Setting it adds it to the frame and strips its own border.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Control Inner
    {
        get => _inner ?? throw new InvalidOperationException("The input frame has no inner control yet.");
        set
        {
            _inner = value;
            if (value is TextBox textBox)
                textBox.BorderStyle = BorderStyle.None;
            else if (value is ComboBox comboBox)
                comboBox.FlatStyle = FlatStyle.Flat;
            else if (value is UpDownBase upDown)
                upDown.BorderStyle = BorderStyle.None;

            value.Font = AppTheme.BodyFont;
            value.GotFocus += (_, _) => Invalidate();
            value.LostFocus += (_, _) => Invalidate();
            Controls.Add(value);
            ApplyColours();
            LayoutInner();
        }
    }

    /// <summary>An icon-font glyph drawn before the text, or empty for none.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string Glyph
    {
        get => _glyph.Text;
        set
        {
            _glyph.Text = value;
            _glyph.Visible = value.Length > 0;
            LayoutInner();
        }
    }

    /// <summary>A muted caption inside the frame before the value, as list filters show it ("Trạng thái:"); empty for none.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public string InlineLabel
    {
        get => _inlineLabel.Text;
        set
        {
            _inlineLabel.Text = value;
            _inlineLabel.Visible = value.Length > 0;
            LayoutInner();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool HasError
    {
        get => _hasError;
        set
        {
            _hasError = value;
            Invalidate();
        }
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ReadOnly
    {
        get => _isReadOnly;
        set
        {
            _isReadOnly = value;
            if (_inner is TextBoxBase textBox)
                textBox.ReadOnly = value;
            else if (_inner is not null)
                _inner.Enabled = !value;
            ApplyColours();
            Invalidate();
        }
    }

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        LayoutInner();
    }

    protected override void OnClick(EventArgs e)
    {
        base.OnClick(e);
        _inner?.Focus();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var isFocused = _inner?.ContainsFocus == true && !_isReadOnly;
        var (colour, width) = _hasError ? (AppTheme.Danger, 2) : isFocused ? (AppTheme.Accent, 2) : (AppTheme.InputBorder, 1);
        ControlPaint.DrawBorder(e.Graphics, ClientRectangle,
            colour, width, ButtonBorderStyle.Solid, colour, width, ButtonBorderStyle.Solid,
            colour, width, ButtonBorderStyle.Solid, colour, width, ButtonBorderStyle.Solid);
    }

    private void ApplyColours()
    {
        BackColor = _isReadOnly ? AppTheme.Subtle : AppTheme.Card;
        _glyph.BackColor = BackColor;
        _inlineLabel.BackColor = BackColor;
        if (_inner is null)
            return;

        _inner.BackColor = BackColor;
        _inner.ForeColor = _isReadOnly ? AppTheme.Text2 : AppTheme.Text;
    }

    private void LayoutInner()
    {
        var left = InnerPadding;
        if (_glyph.Visible)
        {
            // Inside the 2px focus border, so the glyph never paints over it.
            _glyph.Bounds = new Rectangle(left, 2, GlyphWidth, Math.Max(0, Height - 4));
            left += GlyphWidth + GlyphGap;
        }

        if (_inlineLabel.Visible)
        {
            _inlineLabel.Location = new Point(left, (Height - _inlineLabel.Height) / 2);
            left += _inlineLabel.Width + 2;
        }

        if (_inner is null)
            return;

        if (_inner is DateTimePicker)
        {
            LayoutDatePicker(left);
            return;
        }

        _inner.Width = Math.Max(0, Width - left - InnerPadding);
        _inner.Location = new Point(left, (Height - _inner.Height) / 2);
    }

    // A date picker cannot drop its own border, so it is laid out 2px larger on every side and clipped to its inside;
    // only the frame's border shows.
    private void LayoutDatePicker(int left)
    {
        const int border = 2;
        var width = Math.Max(2 * border, Width - left - InnerPadding + 2 * border);
        _inner!.Bounds = new Rectangle(left - border, (Height - _inner.Height) / 2, width, _inner.Height);

        var previous = _inner.Region;
        _inner.Region = new Region(new Rectangle(border, border, width - 2 * border, _inner.Height - 2 * border));
        previous?.Dispose();
    }
}
