using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;
using LuuKyCanTin.IntegrationTests.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Seed;

public class DemoDataSeederTests
{
    [SqlServerFact]
    public async Task AnEmptyUnitRow_GetsTheDemoValues()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();

        await using (var db = database.CreateDbContext())
        {
            (await new DemoDataSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.ScalarAsync("SELECT TenDonVi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("TRẠI TẠM GIAM … (dữ liệu mẫu)");
        (await database.ScalarAsync("SELECT DiaChi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("Xã …, huyện …, tỉnh …");
    }

    [SqlServerFact]
    public async Task ARowWithOnlyTheParentUnitFilled_IsLeftUntouched()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await database.ExecuteAsync("UPDATE ThongTinDonVi SET TenCoQuanChuQuan = N'CÔNG AN X' WHERE Id = 1");

        await using (var db = database.CreateDbContext())
        {
            (await new DemoDataSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.ScalarAsync("SELECT TenCoQuanChuQuan FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("CÔNG AN X");
        (await database.ScalarAsync("SELECT TenDonVi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("");
        (await database.ScalarAsync("SELECT DiaChi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("");
    }
}
