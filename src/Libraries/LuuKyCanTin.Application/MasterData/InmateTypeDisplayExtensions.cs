using LuuKyCanTin.Domain.MasterData;

namespace LuuKyCanTin.Application.MasterData;

public static class InmateTypeDisplayExtensions
{
    public static string ToDisplayText(this InmateType value) => value switch
    {
        InmateType.PreTrialDetainee => "Tạm giữ/tạm giam",
        InmateType.Prisoner => "Phạm nhân",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown inmate type."),
    };
}
