using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.IntegrationTests.Persistence.TestModel;

public sealed class MauChungTuConfiguration : AuditableEntityConfiguration<MauChungTu>
{
    protected override void CauHinhRieng(EntityTypeBuilder<MauChungTu> builder)
    {
        builder.HasEnumCheck(e => e.TrangThai);
        builder.HasEnumCheck(e => e.TrangThaiTruoc);
    }
}
