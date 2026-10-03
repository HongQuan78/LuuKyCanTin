using LuuKyCanTin.Domain.Administration;

namespace LuuKyCanTin.Infrastructure.Persistence.Configurations.Administration;

/// <summary>
/// The text stored for each <see cref="AuditAction"/>. The log is append-only and its existing rows already hold these
/// codes, so they are data that never follows a rename of the enum members.
/// </summary>
internal static class AuditActionCodes
{
    public static string ToCode(AuditAction action) => action switch
    {
        AuditAction.Create => "Them",
        AuditAction.Update => "Sua",
        AuditAction.Cancel => "Huy",
        AuditAction.Print => "In",
        AuditAction.Approve => "Duyet",
        AuditAction.SignIn => "DangNhap",
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown audit action."),
    };

    public static AuditAction FromCode(string code) => code switch
    {
        "Them" => AuditAction.Create,
        "Sua" => AuditAction.Update,
        "Huy" => AuditAction.Cancel,
        "In" => AuditAction.Print,
        "Duyet" => AuditAction.Approve,
        "DangNhap" => AuditAction.SignIn,
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "Unknown audit action code."),
    };
}
