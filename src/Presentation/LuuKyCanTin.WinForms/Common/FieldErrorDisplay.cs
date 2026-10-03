namespace LuuKyCanTin.WinForms.Common;

/// <summary>
/// Shows validation messages on a form's fields: the red frame, the message under the field, and focus on the first
/// invalid one. A field enum lists its fields in visual order, so the lowest invalid value is the first on screen.
/// </summary>
internal sealed class FieldErrorDisplay<TField>
    where TField : struct, Enum
{
    private readonly Dictionary<TField, (InputFrame Frame, FieldError Error)> _fields = [];

    /// <summary>Registers a field. Changing its value clears its own error: the message described the old value.</summary>
    public void Add(TField field, InputFrame frame, FieldError error)
    {
        _fields.Add(field, (frame, error));
        frame.Inner.TextChanged += (_, _) => Clear(field);
    }

    public void Show(IReadOnlyList<FieldMessage<TField>> errors)
    {
        ClearAll();
        if (errors.Count == 0)
            return;

        foreach (var group in errors.GroupBy(e => e.Field))
        {
            var (frame, error) = _fields[group.Key];
            frame.HasError = true;
            error.Message = string.Join('\n', group.Select(e => e.Message));
        }

        var inner = _fields[errors.Min(e => e.Field)].Frame.Inner;
        inner.Focus();
        if (inner is TextBoxBase textBox)
            textBox.SelectAll();
    }

    public void ClearAll()
    {
        foreach (var field in _fields.Keys)
            Clear(field);
    }

    /// <summary>Clears one field's error, for example when the field becomes read-only and can no longer be fixed.</summary>
    public void Clear(TField field)
    {
        var (frame, error) = _fields[field];
        frame.HasError = false;
        error.Message = "";
    }
}
