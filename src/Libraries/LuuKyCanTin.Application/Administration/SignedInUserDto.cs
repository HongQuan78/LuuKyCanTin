namespace LuuKyCanTin.Application.Administration;

/// <param name="DisplayName">The name the shell greets; the user name until accounts are linked to staff.</param>
/// <param name="RoleNames">The role names joined with ", ", or empty when the account has no role.</param>
/// <param name="FacilityName">The unit name, or empty while unit information is not filled in.</param>
public sealed record SignedInUserDto(string UserName, string DisplayName, string RoleNames, string FacilityName);
