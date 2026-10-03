using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

/// <summary>
/// Upgrades a database that holds data in every table under the old Vietnamese names, and proves the rename migration
/// keeps every row, including the append-only audit log exactly as it was written.
/// </summary>
public sealed class RenameIdentifiersToEnglishMigrationTests
{
    private const string PreviousMigration = "20261002140541_AddVaiTroQuyen";
    private const string RenameMigration = "20261003120750_RenameIdentifiersToEnglish";

    private const string LegacyAuditPayload =
        """{"SoChungTu":"BNT-2026-00007","TrangThai":"DaGhiSo","SoTien":500000}""";

    private static readonly string[] LegacyTables =
    [
        "CanBo", "DoiTuong", "ChungTuLuuKy", "DemSoChungTu", "NguoiDung", "NguoiDungVaiTro", "NhatKyThaoTac", "Quyen",
        "ThongTinDonVi", "VaiTro", "VaiTroQuyen",
    ];

    private static readonly string[] Tables =
    [
        "Officer", "Inmate", "CustodyVoucher", "VoucherCounter", "[User]", "UserRole", "AuditLog", "Permission",
        "FacilityInfo", "Role", "RolePermission",
    ];

    [SqlServerFact]
    public async Task Migrate_DatabaseWithDataAtThePreviousMigration_KeepsEveryRow()
    {
        await using var database = new TestDatabase();
        await MigrateToAsync(database, PreviousMigration);
        await SeedLegacyRowsAsync(database);
        var countsBefore = await CountRowsAsync(database, LegacyTables);

        await MigrateToAsync(database, RenameMigration);

        (await CountRowsAsync(database, Tables)).ShouldBe(countsBefore);
        await using var db = database.CreateDbContext();

        var officer = await db.Officer.SingleAsync(o => o.OfficerCode == "CB-R1");
        officer.FullName.ShouldBe("Nguyễn Văn Cán Bộ");
        officer.Position.ShouldBe("Quản giáo");
        officer.IsSupervisingOfficer.ShouldBeTrue();
        officer.IsActive.ShouldBeFalse();

        var inmate = await db.Inmate.SingleAsync(i => i.InmateCode == "DT-R1");
        inmate.FullName.ShouldBe("Trần Văn Đối Tượng");
        inmate.BirthYear.ShouldBe((short)1990);
        inmate.InmateType.ShouldBe(InmateType.Prisoner);
        inmate.AdmissionDate.ShouldBe(new DateOnly(2026, 9, 1));
        inmate.Cell.ShouldBe("B2");
        inmate.Status.ShouldBe(InmateStatus.InCustody);
        inmate.CustodyBalance.ShouldBe(500_000m);

        var voucher = await db.CustodyVoucher.SingleAsync(v => v.VoucherNumber == "BNT-2026-00007");
        voucher.VoucherDate.ShouldBe(new DateOnly(2026, 10, 1));
        voucher.VoucherType.ShouldBe(VoucherType.Receipt);
        voucher.TransactionType.ShouldBe(TransactionType.SentByRelative);
        voucher.PaymentMethod.ShouldBe(PaymentMethod.BankTransfer);
        voucher.Status.ShouldBe(VoucherStatus.Posted);
        voucher.InmateId.ShouldBe(inmate.Id);
        voucher.InmateFullName.ShouldBe("Trần Văn Đối Tượng");
        voucher.InmateType.ShouldBe(InmateType.Prisoner);
        voucher.SenderFullName.ShouldBe("Trần Thị Mẹ");
        voucher.Relationship.ShouldBe("Mẹ");
        voucher.SenderAccountNumber.ShouldBe("0123456789");
        voucher.Description.ShouldBe("Gửi tiền");
        voucher.Amount.ShouldBe(500_000m);
        voucher.AmountInWords.ShouldBe("Năm trăm nghìn đồng");
        voucher.BalanceBefore.ShouldBe(0m);
        voucher.BalanceAfter.ShouldBe(500_000m);
        voucher.CreatedById.ShouldBe(1);

        var counter = await db.Set<VoucherCounter>().SingleAsync(c => c.VoucherTypeCode == "BNT" && c.Year == 2026);
        counter.Prefix.ShouldBe("BNT");
        counter.CurrentNumber.ShouldBe(7);

        var admin = await db.User.SingleAsync(u => u.UserName == "admin");
        admin.FailedAttemptCount.ShouldBe((byte)2);
        admin.MustChangePassword.ShouldBeTrue();
        (await db.UserRole.CountAsync(r => r.UserId == admin.Id)).ShouldBe(1);

        var facility = await db.FacilityInfo.SingleAsync();
        facility.FacilityName.ShouldBe("TRẠI TẠM GIAM R1");
        facility.Address.ShouldBe("Xã R1");

        (await db.Permission.SingleAsync(p => p.Id == 19)).Code.ShouldBe("LK-C.Xem");
        (await db.Role.SingleAsync(r => r.Id == 1)).Code.ShouldBe("QUAN_TRI");

        // The log is append-only: the old table name, JSON property names and action code stay as written.
        var auditLog = await db.AuditLog.SingleAsync(a => a.RecordId == voucher.Id);
        auditLog.Action.ShouldBe(AuditAction.Create);
        auditLog.TableName.ShouldBe("ChungTuLuuKy");
        auditLog.NewValues.ShouldBe(LegacyAuditPayload);
        auditLog.Workstation.ShouldBe("MAY-01");
        (await database.GetScalarAsync("SELECT [Action] FROM AuditLog")).ShouldBe("Them");
    }

    [SqlServerFact]
    public async Task MigrateDown_FromTheRename_RestoresTheLegacyNamesWithEveryRow()
    {
        await using var database = new TestDatabase();
        await MigrateToAsync(database, PreviousMigration);
        await SeedLegacyRowsAsync(database);
        var countsBefore = await CountRowsAsync(database, LegacyTables);
        await MigrateToAsync(database, RenameMigration);

        await MigrateToAsync(database, PreviousMigration);

        (await CountRowsAsync(database, LegacyTables)).ShouldBe(countsBefore);
        (await database.GetScalarAsync("SELECT SoDuLuuKy FROM DoiTuong WHERE MaSo = 'DT-R1'")).ShouldBe(500_000m);
        (await database.GetScalarAsync("SELECT HanhDong FROM NhatKyThaoTac")).ShouldBe("Them");
    }

    private static async Task MigrateToAsync(TestDatabase database, string migration)
    {
        await using var db = database.CreateDbContext();
        await db.GetService<IMigrator>().MigrateAsync(migration);
    }

    private static async Task<List<int>> CountRowsAsync(TestDatabase database, IEnumerable<string> tables)
    {
        var counts = new List<int>();
        foreach (var table in tables)
            counts.Add(Convert.ToInt32(await database.GetScalarAsync($"SELECT COUNT(*) FROM {table}")));
        return counts;
    }

    // Written against the schema of the previous migration, so it uses the old names on purpose.
    private static Task SeedLegacyRowsAsync(TestDatabase database) => database.ExecuteAsync("""
        INSERT INTO CanBo (MaCanBo, HoTen, ChucVu, LaQuanGiao, DangCongTac, NgayTao, NguoiTaoId)
        VALUES ('CB-R1', N'Nguyễn Văn Cán Bộ', N'Quản giáo', 1, 0, '2026-10-01', 1);

        INSERT INTO DoiTuong (MaSo, HoTen, NamSinh, LoaiDoiTuong, NgayVao, BuongGiam, TrangThai, SoDuLuuKy, NgayTao, NguoiTaoId)
        VALUES ('DT-R1', N'Trần Văn Đối Tượng', 1990, 2, '2026-09-01', N'B2', 1, 500000, '2026-10-01', 1);

        INSERT INTO ChungTuLuuKy (SoChungTu, NgayChungTu, LoaiPhieu, NghiepVu, HinhThuc, TrangThai, DoiTuongId,
            HoTenDoiTuong, LoaiDoiTuong, NguoiGuiHoTen, QuanHe, SoTaiKhoanNguoiGui, NoiDung, SoTien, SoTienBangChu,
            SoDuTruoc, SoDuSau, NgayTao, NguoiTaoId)
        SELECT 'BNT-2026-00007', '2026-10-01', 1, 12, 2, 2, Id, HoTen, LoaiDoiTuong, N'Trần Thị Mẹ', N'Mẹ',
            '0123456789', N'Gửi tiền', 500000, N'Năm trăm nghìn đồng', 0, 500000, '2026-10-01', 1
        FROM DoiTuong WHERE MaSo = 'DT-R1';

        INSERT INTO NhatKyThaoTac (ThoiDiem, NguoiDungId, MayTram, HanhDong, TenBang, BanGhiId, DuLieuMoi)
        SELECT '2026-10-01 08:00', 1, N'MAY-01', 'Them', 'ChungTuLuuKy', Id,
            N'{"SoChungTu":"BNT-2026-00007","TrangThai":"DaGhiSo","SoTien":500000}'
        FROM ChungTuLuuKy WHERE SoChungTu = 'BNT-2026-00007';

        UPDATE DemSoChungTu SET SoHienTai = 7 WHERE LoaiChungTu = 'BNT' AND Nam = 2026;
        UPDATE NguoiDung SET SoLanSai = 2 WHERE TenDangNhap = 'admin';
        UPDATE ThongTinDonVi SET TenDonVi = N'TRẠI TẠM GIAM R1', DiaChi = N'Xã R1' WHERE Id = 1;
        """);
}
