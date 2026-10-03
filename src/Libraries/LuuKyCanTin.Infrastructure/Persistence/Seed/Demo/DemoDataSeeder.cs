using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.MasterData;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;

internal sealed class DemoDataSeeder(AppDbContext db, IPasswordHasher passwordHasher) : IDemoDataSeeder
{
    /// <summary>Password of every demo account. Development only; the admin still changes its own at first sign-in.</summary>
    public const string DemoPassword = "Demo@2026";

    private sealed record DemoAccount(
        string UserName, string OfficerCode, string FullName, string Position, string RoleCodes, bool IsSupervisingOfficer);

    // One account per standard role, so a tester can sign in and see each role's permissions.
    private static readonly IReadOnlyList<DemoAccount> DemoAccounts =
    [
        new("luuky", "CB-LK", "Nguyễn Thị Lưu Ký", "Cán bộ theo dõi tiền lưu ký", RoleCodes.CustodyOfficer, false),
        new("cantin", "CB-CT", "Trần Văn Căn Tin", "Cán bộ căn tin", RoleCodes.CanteenOfficer, false),
        new("quangiao", "CB-QG", "Lê Văn Quản Giáo", "Cán bộ quản giáo", RoleCodes.SupervisingOfficer, true),
        new("lanhdao", "CB-LD", "Phạm Văn Lãnh Đạo", "Chỉ huy phụ trách", RoleCodes.Leader, false),
        new("ketoan", "CB-KT", "Hoàng Thị Kế Toán", "Kế toán đơn vị", RoleCodes.Accountant, false),
    ];

    public async Task<DemoSeedDecision> SeedAsync(
        bool isDevelopment, string? confirmedDatabaseName, CancellationToken ct = default)
    {
        var decision = DemoSeedPolicy.Decide(isDevelopment, confirmedDatabaseName, db.Database.GetDbConnection().Database);
        if (decision != DemoSeedDecision.Allowed)
            return decision;

        await FillUnitInfoAsync(ct);
        await FillDemoAccountsAsync(ct);
        return decision;
    }

    // The installed row is empty on purpose; on a dev machine the printed header should not be blank.
    // Any field the admin already filled means the row is in use and must not be overwritten.
    private async Task FillUnitInfoAsync(CancellationToken cancellationToken)
    {
        var facility = await db.FacilityInfo.FirstOrDefaultAsync(u => u.Id == 1, cancellationToken);
        if (facility is null
            || !string.IsNullOrWhiteSpace(facility.ParentAgencyName)
            || !string.IsNullOrWhiteSpace(facility.FacilityName)
            || !string.IsNullOrWhiteSpace(facility.Address))
            return;

        facility.ParentAgencyName = "CÔNG AN TỈNH …";
        facility.FacilityName = "TRẠI TẠM GIAM … (dữ liệu mẫu)";
        facility.Address = "Xã …, huyện …, tỉnh …";
        await db.SaveChangesAsync(cancellationToken);
    }

    // Idempotent: an account or staff code that exists is left alone, so re-running --seed-demo changes nothing.
    private async Task FillDemoAccountsAsync(CancellationToken cancellationToken)
    {
        foreach (var account in DemoAccounts)
        {
            if (await db.User.AnyAsync(u => u.UserName == account.UserName, cancellationToken))
                continue;

            var officer = await db.Officer.SingleOrDefaultAsync(c => c.OfficerCode == account.OfficerCode, cancellationToken);
            if (officer is null)
            {
                officer = new Officer(account.OfficerCode, account.FullName, account.Position, account.IsSupervisingOfficer);
                db.Officer.Add(officer);
                await db.SaveChangesAsync(cancellationToken);
            }

            var user = new User
            {
                UserName = account.UserName,
                OfficerId = officer.Id,
                PasswordHash = passwordHasher.Hash(DemoPassword),
                IsActive = true,
                MustChangePassword = false,
            };
            db.User.Add(user);
            await db.SaveChangesAsync(cancellationToken);

            var roleId = await db.Role
                .Where(v => v.Code == account.RoleCodes)
                .Select(v => v.Id)
                .SingleAsync(cancellationToken);
            db.UserRole.Add(new UserRole { UserId = user.Id, RoleId = roleId });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
