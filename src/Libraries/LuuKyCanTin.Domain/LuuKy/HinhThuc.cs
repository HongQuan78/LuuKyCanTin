namespace LuuKyCanTin.Domain.LuuKy;

/// <summary>How the money is handed over. A transfer requires the sender's account number.</summary>
public enum HinhThuc : byte
{
    TienMat = 1,
    ChuyenKhoan = 2,
}
