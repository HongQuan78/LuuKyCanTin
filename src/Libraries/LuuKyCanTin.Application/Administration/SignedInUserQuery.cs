using LuuKyCanTin.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.Administration;

public sealed class SignedInUserQuery(IAppDbContext db, ICurrentUser currentUser, IFacilityInfoStore facilityInfo)
    : ISignedInUserQuery
{
    public async Task<SignedInUserDto?> GetAsync(CancellationToken ct = default)
    {
        if (currentUser.UserId is not { } userId)
            return null;

        var roleNames = await (from userRole in db.UserRole
                               join role in db.Role on userRole.RoleId equals role.Id
                               where userRole.UserId == userId
                               orderby role.Id
                               select role.Name).ToListAsync(ct);
        var facility = await facilityInfo.GetAsync(ct);
        var userName = currentUser.UserName ?? "";
        var displayName = string.IsNullOrWhiteSpace(currentUser.FullName) ? userName : currentUser.FullName;

        return new SignedInUserDto(userName, displayName, string.Join(", ", roleNames), facility?.FacilityName ?? "");
    }
}
