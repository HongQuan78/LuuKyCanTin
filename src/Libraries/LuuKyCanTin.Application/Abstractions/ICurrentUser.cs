namespace LuuKyCanTin.Application.Abstractions;

/// <summary>The user signed in on this workstation, if any.</summary>
public interface ICurrentUser
{
    int? UserId { get; }

    string? UserName { get; }

    bool IsSignedIn { get; }
}
