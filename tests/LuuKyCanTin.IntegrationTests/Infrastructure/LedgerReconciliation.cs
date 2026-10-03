using LuuKyCanTin.IntegrationTests.Common;
using Microsoft.Data.SqlClient;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Infrastructure;

/// <summary>
/// NEN-19: a balance is always the sum of the posted documents. Called after every test that touches the
/// ledger; a failure names the detainees whose balance does not reconcile.
/// </summary>
public static class LedgerReconciliation
{
    private const string Sql = """
        SELECT d.InmateCode, d.CustodyBalance
        FROM Inmate d
        WHERE d.CustodyBalance <> ISNULL((
            SELECT SUM(CASE WHEN c.VoucherType = 1 THEN c.Amount ELSE -c.Amount END)
            FROM CustodyVoucher c
            WHERE c.InmateId = d.Id AND c.Status = 2
        ), 0)
        """;

    public static async Task AssertBalancedAsync(TestDatabase database)
    {
        var unbalanced = await FindUnbalancedAsync(database);
        unbalanced.ShouldBeEmpty("Số dư lưu ký không khớp với tổng chứng từ đã ghi sổ: " + string.Join("; ", unbalanced));
    }

    public static async Task<IReadOnlyList<string>> FindUnbalancedAsync(TestDatabase database)
    {
        await using var connection = new SqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var result = new List<string>();
        while (await reader.ReadAsync())
            result.Add($"{reader.GetString(0)}: số dư {reader.GetDecimal(1)}");
        return result;
    }
}
