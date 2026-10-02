namespace LuuKyCanTin.Application.Common;

/// <summary>A rule the user broke. The message is Vietnamese and shown to the user as is.</summary>
public class LoiNghiepVuException : Exception
{
    public LoiNghiepVuException(string message)
        : base(message)
    {
    }

    public LoiNghiepVuException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
