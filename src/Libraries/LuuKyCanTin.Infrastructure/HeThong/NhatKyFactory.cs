using System.Text.Encodings.Web;
using System.Text.Json;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Infrastructure.HeThong;

/// <summary>Builds audit-log rows, so the save interceptor and explicit events stamp them identically.</summary>
public sealed class NhatKyFactory(IClock clock, ICurrentUser currentUser)
{
    // Relaxed escaping keeps Vietnamese readable on the audit screen instead of \uXXXX sequences.
    private static readonly JsonSerializerOptions JsonOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public NhatKyThaoTac Tao(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieuCu, object? duLieuMoi) => new()
    {
        ThoiDiem = clock.Now,
        NguoiDungId = currentUser.NguoiDungId,
        MayTram = Environment.MachineName,
        HanhDong = hanhDong,
        TenBang = tenBang,
        BanGhiId = banGhiId,
        DuLieuCu = ToJson(duLieuCu),
        DuLieuMoi = ToJson(duLieuMoi),
    };

    private static string? ToJson(object? value) => value is null ? null : JsonSerializer.Serialize(value, JsonOptions);
}
