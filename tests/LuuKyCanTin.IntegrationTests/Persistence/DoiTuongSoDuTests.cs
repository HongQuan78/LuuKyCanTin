using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

/// <summary>
/// Pins <c>PropertySaveBehavior.Ignore</c> on <c>DoiTuong.SoDuLuuKy</c>: an ordinary EF save must never write
/// the balance, so only the ledger engine's conditional UPDATE can change it.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class DoiTuongSoDuTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task SavingADetainee_LeavesTheBalanceUntouched()
    {
        // varchar(30): "DT-" + 24 hex chars.
        var maSo = $"DT-{Guid.NewGuid():N}"[..27];
        await fixture.Database.ThucThiAsync($"""
            INSERT INTO DoiTuong (MaSo, HoTen, LoaiDoiTuong, NgayVao, TrangThai, SoDuLuuKy, NgayTao, NguoiTaoId)
            VALUES ('{maSo}', N'Nguyễn Văn A', 1, '2026-10-01', 1, 12345, SYSDATETIME(), 0)
            """);

        try
        {
            await using (var db = fixture.Database.TaoDbContext())
            {
                var doiTuong = await db.DoiTuong.SingleAsync(d => d.MaSo == maSo);
                doiTuong.HoTen = "Nguyễn Văn B";
                await db.SaveChangesAsync();
            }

            Convert.ToDecimal(await fixture.Database.LayGiaTriAsync($"SELECT SoDuLuuKy FROM DoiTuong WHERE MaSo = '{maSo}'"))
                .ShouldBe(12345m);
            (await fixture.Database.LayGiaTriAsync($"SELECT HoTen FROM DoiTuong WHERE MaSo = '{maSo}'"))
                .ShouldBe("Nguyễn Văn B");
        }
        finally
        {
            await fixture.Database.ThucThiAsync($"DELETE FROM DoiTuong WHERE MaSo = '{maSo}'");
        }
    }
}
