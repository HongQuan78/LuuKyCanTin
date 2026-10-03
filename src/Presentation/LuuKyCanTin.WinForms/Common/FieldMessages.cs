using FluentValidation.Results;

namespace LuuKyCanTin.WinForms.Common;

/// <summary>Turns a service's validation failures into messages for a form's fields.</summary>
internal static class FieldMessages
{
    /// <summary>
    /// Splits the failures into those whose request property is a field of the form, in their order, and the messages
    /// of the rest, which the form shows as a banner.
    /// </summary>
    public static (IReadOnlyList<FieldMessage<TField>> FieldErrors, IReadOnlyList<string> OtherMessages) Split<TField>(
        IEnumerable<ValidationFailure> failures, Func<string, TField?> toField)
        where TField : struct, Enum
    {
        var fieldErrors = new List<FieldMessage<TField>>();
        var otherMessages = new List<string>();
        foreach (var failure in failures)
        {
            if (toField(failure.PropertyName) is { } field)
                fieldErrors.Add(new FieldMessage<TField>(field, failure.ErrorMessage));
            else
                otherMessages.Add(failure.ErrorMessage);
        }

        return (fieldErrors, otherMessages);
    }
}
