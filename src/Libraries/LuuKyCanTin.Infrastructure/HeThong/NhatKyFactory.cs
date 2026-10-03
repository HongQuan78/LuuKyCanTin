using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Infrastructure.HeThong;

/// <summary>Builds audit-log rows, so the save interceptor and explicit events stamp them identically.</summary>
public sealed class NhatKyFactory(IClock clock, ICurrentUser currentUser)
{
    // Relaxed escaping keeps Vietnamese readable on the audit screen instead of \uXXXX sequences, and enums are
    // written by name ("DaHuy", not 3), like HanhDong itself.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() },
    };

    public NhatKyThaoTac Tao(HanhDong hanhDong, string? tenBang, long? banGhiId, object? duLieuCu, object? duLieuMoi) => new()
    {
        ThoiDiem = clock.Now,
        NguoiDungId = currentUser.NguoiDungId,
        MayTram = Environment.MachineName,
        HanhDong = hanhDong,
        TenBang = tenBang,
        BanGhiId = banGhiId,
        DuLieuCu = SangJson(duLieuCu),
        DuLieuMoi = SangJson(duLieuMoi),
    };

    private static string? SangJson(object? giaTri) => giaTri is null ? null : JsonSerializer.Serialize(giaTri, JsonOptions);
}
