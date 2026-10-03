namespace LuuKyCanTin.Domain.Administration;

/// <summary>A document's creator tried to approve it, which FR3 / NEN-09 forbid.</summary>
/// <remarks>
/// The business-error base lives in Application and Domain must not reference it, so this derives from
/// <see cref="Exception"/> directly; the shell recognizes the type as a business warning, the same way it does
/// for <see cref="PermissionDeniedException"/>.
/// </remarks>
public sealed class SeparationOfDutiesViolationException : Exception
{
    public const string ViolationMessage = "Người lập không được tự duyệt.";

    public SeparationOfDutiesViolationException()
        : base(ViolationMessage)
    {
    }
}
