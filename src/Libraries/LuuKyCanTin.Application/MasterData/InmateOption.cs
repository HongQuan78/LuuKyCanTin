namespace LuuKyCanTin.Application.MasterData;

/// <summary>One row of the receipt form's detainee selector. The shared picker (UX-DR2) comes in Story 3.2.</summary>
public sealed record InmateOption(int Id, string InmateCode, string FullName)
{
    public string DisplayText => $"{InmateCode} — {FullName}";
}
