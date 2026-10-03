using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence;

public class SqlServerSmokeTests
{
    [SqlServerFact]
    public async Task OpenAsync_ConfiguredServer_AcceptsSelectQuery()
    {
        await using var connection = new SqlConnection(SqlServerFactAttribute.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand("SELECT 1", connection);

        var result = await command.ExecuteScalarAsync();

        result.ShouldBe(1);
    }
}
