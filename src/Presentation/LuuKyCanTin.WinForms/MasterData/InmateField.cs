namespace LuuKyCanTin.WinForms.MasterData;

/// <summary>The validated fields of the add-detainee dialog, in visual order, so an error lands under the field it concerns.</summary>
public enum InmateField : byte
{
    InmateCode = 1,
    BirthYear = 2,
    FullName = 3,
    InmateType = 4,
    AdmissionDate = 5,
    Cell = 6,
}
