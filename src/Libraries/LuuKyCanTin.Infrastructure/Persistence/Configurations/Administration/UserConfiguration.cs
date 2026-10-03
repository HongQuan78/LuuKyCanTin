using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

internal sealed class UserConfiguration : AuditableEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        builder.Property(e => e.UserName).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.UserName).IsUnique();

        // The hash is ASCII base64; NotAudited keeps it out of the audit-log JSON.
        builder.Property(e => e.PasswordHash).HasMaxLength(200).IsUnicode(false).IsRequired();

        // The sentinel is the default itself, so a true value lets the database default apply and an explicit
        // false is still written (without it EF would treat false as "unset" only if it were the sentinel).
        builder.Property(e => e.IsActive).HasDefaultValue(true).HasSentinel(true);
        builder.Property(e => e.MustChangePassword).HasDefaultValue(false);
        builder.Property(e => e.FailedAttemptCount).HasDefaultValue((byte)0);
        builder.Property(e => e.LockedUntil).HasColumnType("datetime2(0)");
    }
}
