namespace LuuKyCanTin.Application.Abstractions;

/// <summary>
/// The signed-in user, plus the two operations that change the session. Read-only clients depend on
/// <see cref="ICurrentUser"/>; only sign-in and sign-out need this wider contract.
/// </summary>
public interface ICurrentUserSession : ICurrentUser
{
    void SignIn(int userId, string userName);

    void SignOut();
}
