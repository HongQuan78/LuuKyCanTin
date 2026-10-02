namespace LuuKyCanTin.Application.Common;

/// <summary>The row was changed by someone else after this user loaded it: its row version no longer matches.</summary>
public sealed class XungDotDuLieuException(Exception innerException)
    : LoiNghiepVuException(ThongBao, innerException)
{
    public const string ThongBao = "Dữ liệu đã bị người khác thay đổi, vui lòng tải lại";
}
