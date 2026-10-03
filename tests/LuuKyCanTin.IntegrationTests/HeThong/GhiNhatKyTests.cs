using System.Text.Json;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.HeThong;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.HeThong;

public sealed class GhiNhatKyTests(AuditDatabaseFixture fixture) : IClassFixture<AuditDatabaseFixture>
{
    [SqlServerFact]
    public async Task GhiAsync_SignedInUser_WritesUserMachineAndClockTime()
    {
        fixture.User.DangNhap(42, "admin");
        fixture.Clock.Now = new DateTime(2026, 10, 2, 7, 15, 30);

        await using (var db = fixture.TaoDbContextCoNhatKy())
            await new GhiNhatKy(db, fixture.NhatKyFactory).GhiAsync(HanhDong.DangNhap, "NguoiDung", 42, new { TenDangNhap = "admin" });

        await using var dbKiemTra = fixture.TaoDbContext();
        var nhatKy = await dbKiemTra.NhatKyThaoTac.SingleAsync(n => n.HanhDong == HanhDong.DangNhap && n.BanGhiId == 42);
        nhatKy.NguoiDungId.ShouldBe(42);
        nhatKy.MayTram.ShouldBe(Environment.MachineName);
        nhatKy.ThoiDiem.ShouldBe(new DateTime(2026, 10, 2, 7, 15, 30));
        nhatKy.TenBang.ShouldBe("NguoiDung");
        nhatKy.DuLieuCu.ShouldBeNull();
        JsonDocument.Parse(nhatKy.DuLieuMoi!).RootElement.GetProperty("TenDangNhap").GetString().ShouldBe("admin");
    }

    [SqlServerFact]
    public async Task GhiAsync_WithoutPayloadOrUser_WritesNullColumns()
    {
        fixture.User.DangXuat();
        var tenBang = $"In_{Guid.NewGuid():N}"[..30];

        await using (var db = fixture.TaoDbContextCoNhatKy())
            await new GhiNhatKy(db, fixture.NhatKyFactory).GhiAsync(HanhDong.In, tenBang, null);

        await using var dbKiemTra = fixture.TaoDbContext();
        var nhatKy = await dbKiemTra.NhatKyThaoTac.SingleAsync(n => n.TenBang == tenBang);
        nhatKy.HanhDong.ShouldBe(HanhDong.In);
        nhatKy.NguoiDungId.ShouldBeNull();
        nhatKy.BanGhiId.ShouldBeNull();
        nhatKy.DuLieuMoi.ShouldBeNull();
    }
}
