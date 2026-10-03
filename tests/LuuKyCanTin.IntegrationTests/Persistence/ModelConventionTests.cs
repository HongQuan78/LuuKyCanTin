using LuuKyCanTin.Infrastructure.Persistence;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

// Model-only checks: building the model needs the SqlServer provider but never opens a connection.
public sealed class ModelConventionTests
{
    private static readonly DbContextOptions<AppDbContext> Options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    private static IModel GetDesignTimeModel(DbContext context) => context.GetService<IDesignTimeModel>().Model;

    private static IProperty GetProperty(string name)
    {
        using var context = new TestAppDbContext(Options);
        return GetDesignTimeModel(context).FindEntityType(typeof(SampleVoucher))!.FindProperty(name)!;
    }

    [Fact]
    public void OnModelCreating_AnyModel_UsesVietnameseCaseAndAccentInsensitiveCollation()
    {
        using var context = new AppDbContext(Options);

        GetDesignTimeModel(context).GetCollation().ShouldBe("Vietnamese_CI_AI");
    }

    [Theory]
    [InlineData(nameof(SampleVoucher.Amount), "decimal(18,0)", false)]
    [InlineData(nameof(SampleVoucher.VoucherDate), "date", false)]
    [InlineData(nameof(SampleVoucher.PrintedAt), "datetime2(0)", true)]
    [InlineData(nameof(SampleVoucher.Description), "nvarchar(max)", false)]
    [InlineData(nameof(SampleVoucher.Status), "tinyint", false)]
    [InlineData(nameof(SampleVoucher.PreviousStatus), "tinyint", true)]
    [InlineData(nameof(SampleVoucher.CreatedAt), "datetime2(0)", false)]
    [InlineData(nameof(SampleVoucher.CreatedById), "int", false)]
    [InlineData(nameof(SampleVoucher.ModifiedAt), "datetime2(0)", true)]
    [InlineData(nameof(SampleVoucher.ModifiedById), "int", true)]
    [InlineData(nameof(SampleVoucher.RowVer), "rowversion", false)]
    public void ConfigureConventions_SharedPropertyType_HasExpectedColumnType(string name, string columnType, bool nullable)
    {
        var property = GetProperty(name);

        property.GetColumnType().ShouldBe(columnType);
        property.IsNullable.ShouldBe(nullable);
    }

    [Fact]
    public void OnModelCreating_IntKey_IsIdentity()
    {
        GetProperty(nameof(SampleVoucher.Id)).GetValueGenerationStrategy().ShouldBe(SqlServerValueGenerationStrategy.IdentityColumn);
    }

    [Fact]
    public void Configure_RowVer_IsConcurrencyToken()
    {
        var rowVer = GetProperty(nameof(SampleVoucher.RowVer));

        rowVer.IsConcurrencyToken.ShouldBeTrue();
        rowVer.ValueGenerated.ShouldBe(ValueGenerated.OnAddOrUpdate);
    }

    [Fact]
    public void HasEnumCheck_EnumColumn_EmitsOneInListConstraint()
    {
        using var context = new TestAppDbContext(Options);
        var checks = GetDesignTimeModel(context).FindEntityType(typeof(SampleVoucher))!.GetCheckConstraints()
            .ToDictionary(c => c.ModelName, c => c.Sql);

        checks.ShouldBe(new Dictionary<string, string>
        {
            ["CK_SampleVoucher_Status"] = "[Status] IN (1, 2, 3)",
            ["CK_SampleVoucher_PreviousStatus"] = "[PreviousStatus] IN (1, 2, 3)",
        }, ignoreOrder: true);
    }
}
