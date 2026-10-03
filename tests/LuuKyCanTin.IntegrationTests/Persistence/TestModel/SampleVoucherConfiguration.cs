using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.IntegrationTests.Persistence.TestModel;

public sealed class SampleVoucherConfiguration : AuditableEntityConfiguration<SampleVoucher>
{
    protected override void ConfigureEntity(EntityTypeBuilder<SampleVoucher> builder)
    {
        builder.HasEnumCheck(e => e.Status);
        builder.HasEnumCheck(e => e.PreviousStatus);
    }
}
