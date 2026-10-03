namespace LuuKyCanTin.Domain.MasterData;

/// <summary>The legal category of a detainee, printed on every document that snapshots it.</summary>
public enum InmateType : byte
{
    PreTrialDetainee = 1,
    Prisoner = 2,
}
