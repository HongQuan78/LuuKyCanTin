namespace LuuKyCanTin.Domain.Administration;

/// <summary>The signed-in user lacks the permission a service requires before it writes anything.</summary>
public sealed class PermissionDeniedException(string permissionCode)
    : Exception("Bạn không có quyền thực hiện thao tác này")
{
    /// <summary>The permission code the operation required.</summary>
    public string PermissionCode { get; } = permissionCode;
}
