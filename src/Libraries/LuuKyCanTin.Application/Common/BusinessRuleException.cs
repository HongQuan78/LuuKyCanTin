namespace LuuKyCanTin.Application.Common;

/// <summary>A rule the user broke. The message is Vietnamese and shown to the user as is.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message)
        : base(message)
    {
    }

    public BusinessRuleException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
