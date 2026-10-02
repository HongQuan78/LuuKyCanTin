namespace LuuKyCanTin.Domain.HeThong;

/// <summary>The signed-in user lacks the permission a service requires before it writes anything.</summary>
public sealed class KhongCoQuyenException(string maQuyen)
    : Exception("Bạn không có quyền thực hiện thao tác này")
{
    /// <summary>The permission code the operation required.</summary>
    public string MaQuyen { get; } = maQuyen;
}
