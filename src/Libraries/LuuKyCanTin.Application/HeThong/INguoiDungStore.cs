using LuuKyCanTin.Domain.HeThong;

namespace LuuKyCanTin.Application.HeThong;

public interface INguoiDungStore
{
    Task<NguoiDung?> TimTheoDangNhapAsync(string tenDangNhap, CancellationToken ct = default);
}
