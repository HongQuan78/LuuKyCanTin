namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The stable codes of the 6 standard roles. Display names live with the seed and in the database, so they can
/// be corrected without touching code. The code values are data stored in the database, so they never change.
/// </summary>
public static class RoleCodes
{
    public const string Administrator = "QUAN_TRI";
    public const string CustodyOfficer = "LUU_KY";
    public const string CanteenOfficer = "CAN_TIN";
    public const string SupervisingOfficer = "QUAN_GIAO";
    public const string Leader = "LANH_DAO";
    public const string Accountant = "KE_TOAN";
}
