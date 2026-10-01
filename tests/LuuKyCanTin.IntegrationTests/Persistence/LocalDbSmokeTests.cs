using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public class LocalDbSmokeTests
{
    [LocalDbFact]
    public async Task LocalDb_AcceptsConnectionAndQuery()
    {
        await using var connection = new SqlConnection(LocalDbFactAttribute.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT 1", connection);

        var result = await command.ExecuteScalarAsync();

        result.ShouldBe(1);
    }
}
