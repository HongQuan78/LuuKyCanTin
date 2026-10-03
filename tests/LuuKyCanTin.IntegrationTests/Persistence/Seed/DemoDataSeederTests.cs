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
        await database.ApDungMigrationAsync();

        await using (var db = database.TaoDbContext())
        {
            (await TaoSeeder(db).NapDuLieuMauAsync(laMoiTruongPhatTrien: true, tenCoSoDuLieuXacNhan: null))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.LayGiaTriAsync("SELECT TenDonVi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("TRẠI TẠM GIAM … (dữ liệu mẫu)");
        (await database.LayGiaTriAsync("SELECT DiaChi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("Xã …, huyện …, tỉnh …");
    }

    [SqlServerFact]
    public async Task ARowWithOnlyTheParentUnitFilled_IsLeftUntouched()
    {
        await using var database = new TestDatabase();
        await database.ApDungMigrationAsync();
        await database.ThucThiAsync("UPDATE ThongTinDonVi SET TenCoQuanChuQuan = N'CÔNG AN X' WHERE Id = 1");

        await using (var db = database.TaoDbContext())
        {
            (await TaoSeeder(db).NapDuLieuMauAsync(laMoiTruongPhatTrien: true, tenCoSoDuLieuXacNhan: null))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.LayGiaTriAsync("SELECT TenCoQuanChuQuan FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("CÔNG AN X");
        (await database.LayGiaTriAsync("SELECT TenDonVi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("");
        (await database.LayGiaTriAsync("SELECT DiaChi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("");
    }

    [SqlServerFact]
    public async Task DemoRun_TaoDuTaiKhoanVaiTro_ChayLaiKhongNhanDoi()
    {
        await using var database = new TestDatabase();
        await database.ApDungMigrationAsync();

        await using (var db = database.TaoDbContext())
            await TaoSeeder(db).NapDuLieuMauAsync(laMoiTruongPhatTrien: true, tenCoSoDuLieuXacNhan: null);

        // The seeded admin + one account per standard role.
        (await database.LayGiaTriAsync("SELECT COUNT(*) FROM NguoiDung")).ShouldBe(6);
        (await database.LayGiaTriAsync("SELECT COUNT(*) FROM NguoiDungVaiTro")).ShouldBe(6);
        (await database.LayGiaTriAsync("SELECT COUNT(*) FROM CanBo")).ShouldBe(5);
        (await database.LayGiaTriAsync(
            "SELECT v.Ma FROM NguoiDung u JOIN NguoiDungVaiTro uv ON uv.NguoiDungId = u.Id "
            + "JOIN VaiTro v ON v.Id = uv.VaiTroId WHERE u.TenDangNhap = 'luuky'")).ShouldBe("LUU_KY");

        await using (var db = database.TaoDbContext())
            await TaoSeeder(db).NapDuLieuMauAsync(laMoiTruongPhatTrien: true, tenCoSoDuLieuXacNhan: null);

        (await database.LayGiaTriAsync("SELECT COUNT(*) FROM NguoiDung")).ShouldBe(6);
        (await database.LayGiaTriAsync("SELECT COUNT(*) FROM CanBo")).ShouldBe(5);
    }

    [SqlServerFact]
    public async Task DemoRun_TaiKhoanDangNhapDuocBangMatKhauDemo()
    {
        await using var database = new TestDatabase();
        await database.ApDungMigrationAsync();
        await using (var db = database.TaoDbContext())
            await TaoSeeder(db).NapDuLieuMauAsync(laMoiTruongPhatTrien: true, tenCoSoDuLieuXacNhan: null);

        var hash = (string)(await database.LayGiaTriAsync("SELECT MatKhauHash FROM NguoiDung WHERE TenDangNhap = 'ketoan'"))!;
        new Pbkdf2MatKhauHasher().Verify(DemoDataSeeder.MatKhauDemo, hash).ShouldBeTrue();
    }

    [SqlServerFact]
    public async Task NapDuLieuMau_OutsideDevelopmentConfirmedWithConnectedDatabaseName_IsAllowed()
    {
        await using var database = new TestDatabase();
        await database.ApDungMigrationAsync();

        await using (var db = database.TaoDbContext())
        {
            (await TaoSeeder(db).NapDuLieuMauAsync(laMoiTruongPhatTrien: false, tenCoSoDuLieuXacNhan: database.Name))
                .ShouldBe(DemoSeedDecision.Allowed);
        }

        (await database.LayGiaTriAsync("SELECT COUNT(*) FROM NguoiDung")).ShouldBe(6);
    }

    [SqlServerFact]
    public async Task NapDuLieuMau_OutsideDevelopmentNotConfirmed_IsRefusedAndWritesNothing()
    {
        await using var database = new TestDatabase();
        await database.ApDungMigrationAsync();

        (string? XacNhan, DemoSeedDecision Expected)[] danhSachTruongHop =
        [
            (null, DemoSeedDecision.NotDevelopment),
            ("", DemoSeedDecision.DatabaseNameMismatch),
            ("LuuKyCanTin", DemoSeedDecision.DatabaseNameMismatch),
        ];
        foreach (var (xacNhan, expected) in danhSachTruongHop)
        {
            await using var db = database.TaoDbContext();
            (await TaoSeeder(db).NapDuLieuMauAsync(laMoiTruongPhatTrien: false, tenCoSoDuLieuXacNhan: xacNhan))
                .ShouldBe(expected);
        }

        // Only the admin account seeded by the migrations.
        (await database.LayGiaTriAsync("SELECT COUNT(*) FROM NguoiDung")).ShouldBe(1);
        (await database.LayGiaTriAsync("SELECT TenDonVi FROM ThongTinDonVi WHERE Id = 1")).ShouldBe("");
    }
}
