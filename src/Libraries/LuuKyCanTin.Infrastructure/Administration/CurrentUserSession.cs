using LuuKyCanTin.Application.Abstractions;

namespace LuuKyCanTin.Infrastructure.Administration;

/// <summary>The workstation's sign-in state: one per process, set on sign-in and cleared on sign-out.</summary>
public sealed class CurrentUserSession : ICurrentUserSession
{
    private sealed record Session(int UserId, string UserName);

    // One reference swap, so a background save never sees the id of one user with the name of another.
    private volatile Session? _session;

    public int? UserId => _session?.UserId;

    public string? UserName => _session?.UserName;

    public bool IsSignedIn => _session is not null;

    public void SignIn(int userId, string userName) => _session = new Session(userId, userName);

    public void SignOut() => _session = null;
}
