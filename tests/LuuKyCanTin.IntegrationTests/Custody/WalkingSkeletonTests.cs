using LuuKyCanTin.Application;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Administration;
using LuuKyCanTin.Application.Custody;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Infrastructure;
using LuuKyCanTin.IntegrationTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using UglyToad.PdfPig;

namespace LuuKyCanTin.IntegrationTests.Custody;

/// <summary>
/// The Sprint 0 gate: sign in, add one detainee, post one 500,000 đ receipt through the real ledger engine
/// and print it — on LocalDB/SQL Server, with the real migrations, DI, interceptor and report renderer.
/// </summary>
public class WalkingSkeletonTests(WalkingSkeletonFixture fixture) : IClassFixture<WalkingSkeletonFixture>
{
    // The initial password seeded by AddWalkingSkeletonTables; see docs/install.md. Epic 2 forces a change.
    private const string AdminPassword = "LuuKy@2026";

    private static readonly DateOnly VoucherDate = new(2026, 10, 1);

    private async Task ResetAsync()
    {
        await fixture.Database.ExecuteAsync("""
            DELETE FROM AuditLog;
            DELETE FROM CustodyVoucher;
            DELETE FROM Inmate;
            UPDATE VoucherCounter SET CurrentNumber = 0;
            UPDATE FacilityInfo
            SET ParentAgencyName = N'CÔNG AN TỈNH ABC',
                FacilityName = N'TRẠI TẠM GIAM ABC',
                Address = N'Xã ABC, huyện ABC, tỉnh ABC'
            WHERE Id = 1;
            """);
    }

    private async Task<int> SignInAdminAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<SignInService>().SignInAsync("admin", AdminPassword);
        result.Succeeded.ShouldBeTrue(result.Message);
        return scope.ServiceProvider.GetRequiredService<ICurrentUser>().UserId.ShouldNotBeNull();
    }

    private async Task<int> AddInmateAsync(string inmateCode = "DT-0001")
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<AddInmateService>().AddAsync(
            new AddInmateRequest(inmateCode, "Nguyễn Văn A", 1990, InmateType.PreTrialDetainee, new DateOnly(2026, 9, 1), "A3"));

        result.Succeeded.ShouldBeTrue(result.Message);
        return result.Id;
    }

    private static PostDepositReceiptRequest Receipt(int inmateId) => new()
    {
        InmateId = inmateId,
        VoucherDate = VoucherDate,
        TransactionType = TransactionType.SentByRelative,
        PaymentMethod = PaymentMethod.Cash,
        SenderFullName = "Trần Thị B",
        Relationship = "Mẹ",
        Description = "Tiền gửi lưu ký",
        Amount = 500_000,
    };

    [SqlServerFact]
    public async Task Walk_SignInAddDetaineePostReceiptAndPrint()
    {
        await ResetAsync();
        var adminId = await SignInAdminAsync();
        var inmateId = await AddInmateAsync();

        PostingResult result;
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            result = await scope.ServiceProvider.GetRequiredService<CustodyLedgerService>()
                .PostDepositReceiptAsync(Receipt(inmateId));
        }

        result.Succeeded.ShouldBeTrue(result.Message);
        result.VoucherNumber.ShouldBe("BNT-2026-00001");
        result.BalanceAfter.ShouldBe(500_000m);

        await using (var db = fixture.Database.CreateDbContext())
        {
            var voucher = await db.CustodyVoucher.SingleAsync(c => c.Id == result.Id);
            voucher.VoucherNumber.ShouldBe("BNT-2026-00001");
            voucher.Status.ShouldBe(VoucherStatus.Posted);
            voucher.BalanceBefore.ShouldBe(0m);
            voucher.BalanceAfter.ShouldBe(500_000m);
            voucher.AmountInWords.ShouldBe("Năm trăm nghìn đồng");
            voucher.InmateFullName.ShouldBe("Nguyễn Văn A");
            voucher.InmateType.ShouldBe(InmateType.PreTrialDetainee);
            voucher.VoucherDate.ShouldBe(VoucherDate);

            (await db.Inmate.SingleAsync(d => d.Id == inmateId)).CustodyBalance.ShouldBe(500_000m);
            (await db.VoucherCounter.SingleAsync(d => d.VoucherTypeCode == "BNT" && d.Year == 2026)).CurrentNumber.ShouldBe(1);

            var auditLog = await db.AuditLog.OrderBy(n => n.Id).ToListAsync();
            auditLog.Count.ShouldBe(2);
            var createRow = auditLog.Single(n => n.Action == AuditAction.Create);
            createRow.TableName.ShouldBe("CustodyVoucher");
            createRow.RecordId.ShouldBe(result.Id);
            auditLog.Single(n => n.Action == AuditAction.SignIn).UserId.ShouldBe(adminId);
        }

        // Print: the model reads the stored snapshot plus FacilityInfo, and the renderer emits a real PDF.
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var model = (await scope.ServiceProvider.GetRequiredService<DepositReceiptPrintQuery>()
                .GetAsync(result.Id)).ShouldNotBeNull();

            model.FacilityName.ShouldBe("TRẠI TẠM GIAM ABC");
            model.InmateFullName.ShouldBe("Nguyễn Văn A");
            model.Amount.ShouldBe(500_000m);
            model.AmountInWords.ShouldBe("Năm trăm nghìn đồng");

            var pdf = scope.ServiceProvider.GetRequiredService<IReportRenderer>().Render(model);
            pdf.Take(4).ShouldBe([0x25, 0x50, 0x44, 0x46]);

            using var document = PdfDocument.Open(pdf);
            var text = string.Concat(document.GetPages().Select(p => p.Text));
            text.ShouldContain("TRẠI TẠM GIAM ABC");
            text.ShouldContain("BNT-2026-00001");
            text.ShouldContain("Nguyễn Văn A");
            text.ShouldContain("500.000");
            text.ShouldContain("Năm trăm nghìn đồng");
        }

        await LedgerReconciliation.AssertBalancedAsync(fixture.Database);
    }

    [SqlServerFact]
    public async Task Rollback_AfterNumbering_ReturnsTheNumberAndLeavesNoTrace()
    {
        await ResetAsync();
        await SignInAdminAsync();
        var inmateId = await AddInmateAsync("DT-0002");

        var writer = Substitute.For<ICustodyBalanceWriter>();
        writer.IncreaseAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<(decimal BalanceBefore, decimal BalanceAfter)?>(null));

        await using var scope = fixture.Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var ledger = new CustodyLedgerService(
            sp.GetRequiredService<IInmateStore>(),
            sp.GetRequiredService<ICustodyVoucherStore>(),
            sp.GetRequiredService<IAppDbContext>(),
            sp.GetRequiredService<INumberingService>(),
            writer,
            sp.GetRequiredService<IClock>());

        var result = await ledger.PostDepositReceiptAsync(Receipt(inmateId));

        result.Succeeded.ShouldBeFalse();
        Convert.ToInt32(await fixture.Database.GetScalarAsync("SELECT COUNT(*) FROM CustodyVoucher")).ShouldBe(0);
        Convert.ToInt32(await fixture.Database.GetScalarAsync("SELECT COUNT(*) FROM AuditLog WHERE [Action] = 'Them'")).ShouldBe(0);
        Convert.ToDecimal(await fixture.Database.GetScalarAsync($"SELECT CustodyBalance FROM Inmate WHERE Id = {inmateId}")).ShouldBe(0m);
        Convert.ToInt32(await fixture.Database.GetScalarAsync("SELECT CurrentNumber FROM VoucherCounter WHERE VoucherTypeCode = 'BNT' AND Year = 2026"))
            .ShouldBe(0, "a rolled-back posting must not consume a document number");

        await LedgerReconciliation.AssertBalancedAsync(fixture.Database);
    }
}

/// <summary>A dedicated database per test class, migrated from scratch, wired like the app's DI.</summary>
public sealed class WalkingSkeletonFixture : IAsyncLifetime
{
    public TestDatabase Database { get; } = new();

    public FakeClock Clock { get; } = new(new DateTime(2026, 10, 1, 8, 30, 0));

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        if (!SqlServerFactAttribute.CanRun)
            return;

        await Database.MigrateAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:LuuKyCanTin"] = Database.ConnectionString,
            })
            .Build();

        var services = new ServiceCollection()
            .AddApplication()
            .AddInfrastructure(configuration);

        // The walking skeleton is dated once, so numbering and documents are deterministic.
        services.AddSingleton<IClock>(Clock);

        Services = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        if (!SqlServerFactAttribute.CanRun)
            return;

        await Services.DisposeAsync();
        await Database.DisposeAsync();
    }
}
