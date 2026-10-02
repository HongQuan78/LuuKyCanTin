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
        SELECT d.MaSo, d.SoDuLuuKy
        FROM DoiTuong d
        WHERE d.SoDuLuuKy <> ISNULL((
            SELECT SUM(CASE WHEN c.LoaiPhieu = 1 THEN c.SoTien ELSE -c.SoTien END)
            FROM ChungTuLuuKy c
            WHERE c.DoiTuongId = d.Id AND c.TrangThai = 2
        ), 0)
        """;

    public static async Task AssertBalancedAsync(TestDatabase database)
    {
        var lech = await FindUnbalancedAsync(database);
        lech.ShouldBeEmpty("Số dư lưu ký không khớp với tổng chứng từ đã ghi sổ: " + string.Join("; ", lech));
    }

    public static async Task<IReadOnlyList<string>> FindUnbalancedAsync(TestDatabase database)
    {
        await using var connection = new SqlConnection(database.ConnectionString);
        await connection.OpenAsync();
        await using var command = new SqlCommand(Sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var ketQua = new List<string>();
        while (await reader.ReadAsync())
            ketQua.Add($"{reader.GetString(0)}: số dư {reader.GetDecimal(1)}");
        return ketQua;
    }
}
