using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.Custody;
using LuuKyCanTin.Domain.MasterData;
using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.EnumChecks;

// Model-only: kept out of the SQL Server collection so they run, and pass, without a server.
public sealed class EnumModelTests
{
    private static readonly DbContextOptions<AppDbContext> ModelOnlyOptions = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    [Fact]
    public void GetEnumColumns_EveryEnumColumn_IsDeclaredByte()
    {
        using var db = new AppDbContext(ModelOnlyOptions);

        EnumModel.GetEnumColumns(db)
            .Where(c => Enum.GetUnderlyingType(c.EnumType) != typeof(byte))
            .Select(c => $"{c.EnumType.Name} is not declared ': byte'")
            .ShouldBeEmpty();
    }

    [Fact]
    public void GetEnumColumns_NumericEnumColumn_IsTinyint()
    {
        using var db = new AppDbContext(ModelOnlyOptions);

        EnumModel.GetEnumProperties(db)
            .Where(p => !EnumModel.IsStoredAsText(p) && p.GetColumnType() != "tinyint")
            .Select(p => $"{p.DeclaringType.DisplayName()}.{p.Name} is {p.GetColumnType()}; declare the enum ': byte'")
            .ShouldBeEmpty();
    }

    [Fact]
    public void GetEnumProperties_NullableAndNonNullableEnums_FindsBoth()
    {
        using var db = new TestAppDbContext(ModelOnlyOptions);

        EnumModel.GetEnumColumns(db).Select(c => c with { ToStoredText = null }).ShouldBe(
        [
            new EnumColumn(EnumModel.DefaultSchema, "SampleVoucher", "Status", typeof(SampleStatus)),
            new EnumColumn(EnumModel.DefaultSchema, "SampleVoucher", "PreviousStatus", typeof(SampleStatus)),
            new EnumColumn(EnumModel.DefaultSchema, "AuditLog", "Action", typeof(AuditAction), IsStoredAsText: true),
            new EnumColumn(EnumModel.DefaultSchema, "Inmate", "InmateType", typeof(InmateType)),
            new EnumColumn(EnumModel.DefaultSchema, "Inmate", "Status", typeof(InmateStatus)),
            new EnumColumn(EnumModel.DefaultSchema, "CustodyVoucher", "VoucherType", typeof(VoucherType)),
            new EnumColumn(EnumModel.DefaultSchema, "CustodyVoucher", "TransactionType", typeof(TransactionType)),
            new EnumColumn(EnumModel.DefaultSchema, "CustodyVoucher", "PaymentMethod", typeof(PaymentMethod)),
            new EnumColumn(EnumModel.DefaultSchema, "CustodyVoucher", "Status", typeof(VoucherStatus)),
            new EnumColumn(EnumModel.DefaultSchema, "CustodyVoucher", "InmateType", typeof(InmateType)),
        ], ignoreOrder: true);
    }

    [Fact]
    public void GetEnumColumns_AuditAction_IsStoredAsItsLegacyCode()
    {
        using var db = new AppDbContext(ModelOnlyOptions);

        var action = EnumModel.GetEnumColumns(db).Single(c => c.EnumType == typeof(AuditAction));

        Enum.GetValues<AuditAction>().Select(a => action.ToStoredText!(a))
            .ShouldBe(["Them", "Sua", "Huy", "In", "Duyet", "DangNhap"]);
    }
}
