namespace LuuKyCanTin.WinForms.Common;

/// <summary>A message for one field of a form; <typeparamref name="TField"/> is that form's field enum.</summary>
public sealed record FieldMessage<TField>(TField Field, string Message)
    where TField : struct, Enum;
