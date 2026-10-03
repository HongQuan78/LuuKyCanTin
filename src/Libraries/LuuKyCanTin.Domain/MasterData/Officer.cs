using LuuKyCanTin.Domain.Common;

namespace LuuKyCanTin.Domain.MasterData;

/// <summary>
/// A staff member. <see cref="Position"/> is the business role as free text; system permissions come from the user
/// account's roles, never from this record.
/// </summary>
/// <remarks>
/// Staff are never deleted: someone who has left gets <see cref="IsActive"/> = false, which hides them from
/// selection lists while historical documents keep pointing at them.
/// </remarks>
public sealed class Officer : AuditableEntity, IAuditable
{
    public Officer(string officerCode, string fullName, string? position, bool isSupervisingOfficer)
    {
        Update(officerCode, fullName, position, isSupervisingOfficer);
    }

    private Officer()
    {
    }

    public int Id { get; private set; }

    /// <summary>Upper-case, so a code typed in either case finds the same person.</summary>
    public string OfficerCode { get; private set; } = "";

    public string FullName { get; private set; } = "";

    public string? Position { get; private set; }

    public bool IsSupervisingOfficer { get; private set; }

    public bool IsActive { get; set; } = true;

    public void Update(string officerCode, string fullName, string? position, bool isSupervisingOfficer)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(officerCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullName);

        OfficerCode = officerCode.Trim().ToUpperInvariant();
        FullName = fullName.Trim();
        Position = string.IsNullOrWhiteSpace(position) ? null : position.Trim();
        IsSupervisingOfficer = isSupervisingOfficer;
    }
}
