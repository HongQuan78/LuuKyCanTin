using System.Text.Json;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.HeThong;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.HeThong;

public class GhiNhatKyTests(AuditDatabaseFixture fixture) : IClassFixture<AuditDatabaseFixture>
{
    [SqlServerFact]
    public async Task GhiAsync_WritesBusinessEventWithUserMachineAndClockTime()
    {
        fixture.User.DangNhap(42, "admin");
        fixture.Clock.Now = new DateTime(2026, 10, 2, 7, 15, 30);

        await using (var db = fixture.CreateAuditedContext())
            await new GhiNhatKy(db, fixture.NhatKyFactory).GhiAsync(HanhDong.DangNhap, "NguoiDung", 42, new { TenDangNhap = "admin" });

        await using var check = fixture.CreatePlainContext();
        var row = await check.NhatKyThaoTac.SingleAsync(n => n.HanhDong == HanhDong.DangNhap && n.BanGhiId == 42);
        row.NguoiDungId.ShouldBe(42);
        row.MayTram.ShouldBe(Environment.MachineName);
        row.ThoiDiem.ShouldBe(new DateTime(2026, 10, 2, 7, 15, 30));
        row.TenBang.ShouldBe("NguoiDung");
        row.DuLieuCu.ShouldBeNull();
        JsonDocument.Parse(row.DuLieuMoi!).RootElement.GetProperty("TenDangNhap").GetString().ShouldBe("admin");
    }

    [SqlServerFact]
    public async Task GhiAsync_WithoutPayloadOrUser_WritesNullColumns()
    {
        fixture.User.DangXuat();
        var marker = $"In_{Guid.NewGuid():N}"[..30];

        await using (var db = fixture.CreateAuditedContext())
            await new GhiNhatKy(db, fixture.NhatKyFactory).GhiAsync(HanhDong.In, marker, null);

        await using var check = fixture.CreatePlainContext();
        var row = await check.NhatKyThaoTac.SingleAsync(n => n.TenBang == marker);
        row.HanhDong.ShouldBe(HanhDong.In);
        row.NguoiDungId.ShouldBeNull();
        row.BanGhiId.ShouldBeNull();
        row.DuLieuMoi.ShouldBeNull();
    }
}
