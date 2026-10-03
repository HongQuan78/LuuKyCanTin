using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

/// <summary>
/// Pins <c>PropertySaveBehavior.Ignore</c> on <c>Inmate.CustodyBalance</c>: an ordinary EF save must never write
/// the balance, so only the ledger engine's conditional UPDATE can change it.
/// </summary>
[Collection(SqlServerCollection.Name)]
public class InmateBalanceTests(SqlServerFixture fixture)
{
    [SqlServerFact]
    public async Task SavingADetainee_LeavesTheBalanceUntouched()
    {
        // varchar(30): "DT-" + 24 hex chars.
        var inmateCode = $"DT-{Guid.NewGuid():N}"[..27];
        await fixture.Database.ExecuteAsync($"""
            INSERT INTO Inmate (InmateCode, FullName, InmateType, AdmissionDate, Status, CustodyBalance, CreatedAt, CreatedById)
            VALUES ('{inmateCode}', N'Nguyễn Văn A', 1, '2026-10-01', 1, 12345, SYSDATETIME(), 0)
            """);

        try
        {
            await using (var db = fixture.Database.CreateDbContext())
            {
                var inmate = await db.Inmate.SingleAsync(d => d.InmateCode == inmateCode);
                inmate.FullName = "Nguyễn Văn B";
                await db.SaveChangesAsync();
            }

            Convert.ToDecimal(await fixture.Database.GetScalarAsync($"SELECT CustodyBalance FROM Inmate WHERE InmateCode = '{inmateCode}'"))
                .ShouldBe(12345m);
            (await fixture.Database.GetScalarAsync($"SELECT FullName FROM Inmate WHERE InmateCode = '{inmateCode}'"))
                .ShouldBe("Nguyễn Văn B");
        }
        finally
        {
            await fixture.Database.ExecuteAsync($"DELETE FROM Inmate WHERE InmateCode = '{inmateCode}'");
        }
    }
}
