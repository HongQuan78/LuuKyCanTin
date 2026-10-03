using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

// Needs no SQL Server: nothing listens on port 1, so the connection fails fast.
public sealed class SchemaVersionCheckerUnreachableServerTests
{
    [Fact]
    public async Task KiemTra_UnreachableServer_IsConnectionFailedNotMismatch()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=tcp:127.0.0.1,1;Database=LuuKyCanTin;Integrated Security=true;Connect Timeout=2;Encrypt=false")
            .Options;
        await using var db = new AppDbContext(options);

        var result = await new SchemaVersionChecker(db, NullLogger<SchemaVersionChecker>.Instance).KiemTraAsync();

        result.Status.ShouldBe(SchemaVersionStatus.ConnectionFailed);
        result.Expected.ShouldBe(db.Database.GetMigrations().Last());
    }
}
