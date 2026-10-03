namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// The signed-in user, plus the two operations that change the session. Read-only clients depend on
/// <see cref="ICurrentUser"/>; only sign-in and sign-out need this wider contract.
/// </summary>
public interface ICurrentUserSession : ICurrentUser
{
    /// <param name="permissionCodes">The codes the user's roles grant, loaded once at sign-in for UI checks.</param>
    void SignIn(int userId, string userName, int? officerId, string? fullName, IReadOnlyCollection<string> permissionCodes);

    void SignOut();
}
