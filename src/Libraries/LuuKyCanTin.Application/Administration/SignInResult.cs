namespace LuuKyCanTin.Application.Administration;

public sealed record SignInResult(SignInStatus Status, string? Message)
{
    /// <summary>True when the credentials were accepted, whether or not a change is forced first.</summary>
    public bool Succeeded => Status is SignInStatus.Succeeded or SignInStatus.PasswordChangeRequired;

    public bool MustChangePassword => Status == SignInStatus.PasswordChangeRequired;

    public static SignInResult Ok() => new(SignInStatus.Succeeded, null);

    public static SignInResult PasswordChangeRequired() => new(SignInStatus.PasswordChangeRequired, null);

    public static SignInResult Fail(SignInStatus status, string message) => new(status, message);
}
