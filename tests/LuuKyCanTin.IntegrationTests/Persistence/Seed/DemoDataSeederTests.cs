using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Infrastructure.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;
using LuuKyCanTin.IntegrationTests.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Seed;

public class DemoDataSeederTests
{
    private static DemoDataSeeder CreateSeeder(AppDbContext db) =>
        new(db, new Pbkdf2PasswordHasher());

    [SqlServerFact]
    public async Task AnEmptyUnitRow_GetsTheDemoValues()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();

        await using (var db = database.CreateDbContext())
        {
            (await CreateSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.GetScalarAsync("SELECT FacilityName FROM FacilityInfo WHERE Id = 1")).ShouldBe("TRẠI TẠM GIAM … (dữ liệu mẫu)");
        (await database.GetScalarAsync("SELECT Address FROM FacilityInfo WHERE Id = 1")).ShouldBe("Xã …, huyện …, tỉnh …");
    }

    [SqlServerFact]
    public async Task ARowWithOnlyTheParentUnitFilled_IsLeftUntouched()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await database.ExecuteAsync("UPDATE FacilityInfo SET ParentAgencyName = N'CÔNG AN X' WHERE Id = 1");

        await using (var db = database.CreateDbContext())
        {
            (await CreateSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.GetScalarAsync("SELECT ParentAgencyName FROM FacilityInfo WHERE Id = 1")).ShouldBe("CÔNG AN X");
        (await database.GetScalarAsync("SELECT FacilityName FROM FacilityInfo WHERE Id = 1")).ShouldBe("");
        (await database.GetScalarAsync("SELECT Address FROM FacilityInfo WHERE Id = 1")).ShouldBe("");
    }

    [SqlServerFact]
    public async Task DemoRun_CreatesEveryRoleAccount_RerunDoesNotDuplicate()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();

        await using (var db = database.CreateDbContext())
            await CreateSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null);

        // The seeded admin + one account per standard role.
        (await database.GetScalarAsync("SELECT COUNT(*) FROM [User]")).ShouldBe(6);
        (await database.GetScalarAsync("SELECT COUNT(*) FROM UserRole")).ShouldBe(6);
        (await database.GetScalarAsync("SELECT COUNT(*) FROM Officer")).ShouldBe(5);
        (await database.GetScalarAsync(
            "SELECT r.Code FROM [User] u JOIN UserRole ur ON ur.UserId = u.Id "
            + "JOIN Role r ON r.Id = ur.RoleId WHERE u.UserName = 'luuky'")).ShouldBe("LUU_KY");

        await using (var db = database.CreateDbContext())
            await CreateSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null);

        (await database.GetScalarAsync("SELECT COUNT(*) FROM [User]")).ShouldBe(6);
        (await database.GetScalarAsync("SELECT COUNT(*) FROM Officer")).ShouldBe(5);
    }

    [SqlServerFact]
    public async Task DemoRun_AccountsSignInWithTheDemoPassword()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using (var db = database.CreateDbContext())
            await CreateSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null);

        var hash = (string)(await database.GetScalarAsync("SELECT PasswordHash FROM [User] WHERE UserName = 'ketoan'"))!;
        new Pbkdf2PasswordHasher().Verify(DemoDataSeeder.DemoPassword, hash).ShouldBeTrue();
    }

    [SqlServerFact]
    public async Task Seed_OutsideDevelopmentConfirmedWithConnectedDatabaseName_IsAllowed()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();

        await using (var db = database.CreateDbContext())
        {
            (await CreateSeeder(db).SeedAsync(isDevelopment: false, confirmedDatabaseName: database.Name))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.GetScalarAsync("SELECT COUNT(*) FROM [User]")).ShouldBe(6);
    }

    [SqlServerFact]
    public async Task Seed_OutsideDevelopmentNotConfirmed_IsRefusedAndWritesNothing()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();

        (string? Confirmation, DemoSeedDecision Expected)[] cases =
        [
            (null, DemoSeedDecision.NotDevelopment),
            ("", DemoSeedDecision.DatabaseNameMismatch),
            ("LuuKyCanTin", DemoSeedDecision.DatabaseNameMismatch),
        ];
        foreach (var (confirmation, expected) in cases)
        {
            await using var db = database.CreateDbContext();
            (await CreateSeeder(db).SeedAsync(isDevelopment: false, confirmedDatabaseName: confirmation))
                .ShouldBe(expected);
        }

        // Only the admin account seeded by the migrations.
        (await database.GetScalarAsync("SELECT COUNT(*) FROM [User]")).ShouldBe(1);
        (await database.GetScalarAsync("SELECT FacilityName FROM FacilityInfo WHERE Id = 1")).ShouldBe("");
    }
}
