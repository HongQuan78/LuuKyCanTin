using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.Infrastructure.Persistence.Seed.Demo;
using LuuKyCanTin.IntegrationTests.Common;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Seed;

public class DemoDataSeederTests
{
    private static DemoDataSeeder TaoSeeder(AppDbContext db) =>
        new(db, new Pbkdf2MatKhauHasher());

    [SqlServerFact]
    public async Task AnEmptyUnitRow_GetsTheDemoValues()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();

        await using (var db = database.CreateDbContext())
        {
            (await TaoSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null))
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
            (await TaoSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.ScalarAsync("SELECT TenCoQuanChuQuan FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("CÔNG AN X");
        (await database.ScalarAsync("SELECT TenDonVi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("");
        (await database.ScalarAsync("SELECT DiaChi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("");
    }

    [SqlServerFact]
    public async Task DemoRun_TaoDuTaiKhoanVaiTro_ChayLaiKhongNhanDoi()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();

        await using (var db = database.CreateDbContext())
            await TaoSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null);

        // The seeded admin + one account per standard role.
        (await database.ScalarAsync("SELECT COUNT(*) FROM NguoiDung")).ShouldBe(6);
        (await database.ScalarAsync("SELECT COUNT(*) FROM NguoiDungVaiTro")).ShouldBe(6);
        (await database.ScalarAsync("SELECT COUNT(*) FROM CanBo")).ShouldBe(5);
        (await database.ScalarAsync(
            "SELECT v.Ma FROM NguoiDung u JOIN NguoiDungVaiTro uv ON uv.NguoiDungId = u.Id "
            + "JOIN VaiTro v ON v.Id = uv.VaiTroId WHERE u.TenDangNhap = 'luuky'")).ShouldBe("LUU_KY");

        await using (var db = database.CreateDbContext())
            await TaoSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null);

        (await database.ScalarAsync("SELECT COUNT(*) FROM NguoiDung")).ShouldBe(6);
        (await database.ScalarAsync("SELECT COUNT(*) FROM CanBo")).ShouldBe(5);
    }

    [SqlServerFact]
    public async Task DemoRun_TaiKhoanDangNhapDuocBangMatKhauDemo()
    {
        await using var database = new TestDatabase();
        await database.MigrateAsync();
        await using (var db = database.CreateDbContext())
            await TaoSeeder(db).SeedAsync(isDevelopment: true, confirmedDatabaseName: null);

        var hash = (string)(await database.ScalarAsync("SELECT MatKhauHash FROM NguoiDung WHERE TenDangNhap = 'ketoan'"))!;
        new Pbkdf2MatKhauHasher().Verify(DemoDataSeeder.MatKhauDemo, hash).ShouldBeTrue();
    }
}
