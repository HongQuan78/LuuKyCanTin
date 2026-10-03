using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Configurations.Administration;

public sealed class AuditLogConfigurationTests
{
    private static readonly DbContextOptions<AppDbContext> Options = new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=unused")
        .Options;

    private static IEntityType GetEntityType()
    {
        using var context = new AppDbContext(Options);
        return context.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(AuditLog))!;
    }

    [Fact]
    public void Configure_AnyModel_MapsToTableAuditLog()
    {
        GetEntityType().GetTableName().ShouldBe("AuditLog");
    }

    [Theory]
    [InlineData(nameof(AuditLog.Id), "bigint", false)]
    [InlineData(nameof(AuditLog.OccurredAt), "datetime2(0)", false)]
    [InlineData(nameof(AuditLog.UserId), "int", true)]
    [InlineData(nameof(AuditLog.Workstation), "nvarchar(100)", true)]
    [InlineData(nameof(AuditLog.Action), "varchar(20)", false)]
    [InlineData(nameof(AuditLog.TableName), "varchar(50)", true)]
    [InlineData(nameof(AuditLog.RecordId), "bigint", true)]
    [InlineData(nameof(AuditLog.OldValues), "nvarchar(max)", true)]
    [InlineData(nameof(AuditLog.NewValues), "nvarchar(max)", true)]
    public void Configure_Column_MatchesTheDatabaseDesign(string name, string columnType, bool nullable)
    {
        var property = GetEntityType().FindProperty(name)!;

        property.GetColumnType().ShouldBe(columnType);
        property.IsNullable.ShouldBe(nullable);
    }

    [Fact]
    public void Configure_Id_IsIdentity()
    {
        GetEntityType().FindProperty(nameof(AuditLog.Id))!.GetValueGenerationStrategy()
            .ShouldBe(SqlServerValueGenerationStrategy.IdentityColumn);
    }

    [Fact]
    public void Configure_Action_IsCheckedAgainstTheStoredCodes()
    {
        GetEntityType().GetCheckConstraints().ShouldHaveSingleItem().Sql
            .ShouldBe("[Action] IN ('Them', 'Sua', 'Huy', 'In', 'Duyet', 'DangNhap')");
    }

    [Fact]
    public void Configure_Indexes_SupportSearchByRecordAndByTime()
    {
        GetEntityType().GetIndexes().Select(i => string.Join(",", i.Properties.Select(p => p.Name)))
            .ShouldBe(["TableName,RecordId", "OccurredAt"], ignoreOrder: true);
    }
}
