using LuuKyCanTin.Application.DanhMuc;

namespace LuuKyCanTin.WinForms.DanhMuc;

/// <summary>The staff list with its search box.</summary>
public interface ICanBoView
{
    event EventHandler Loaded;

    /// <summary>Raised once typing pauses, or when the "show staff who left" box changes.</summary>
    event EventHandler TimKiemThayDoi;

    event EventHandler ThemClicked;

    event EventHandler SuaClicked;

    string TuKhoa { get; }

    bool HienCaNguoiDaNghi { get; }

    CanBoDto? CanBoDangChon { get; }

    void HienDanhSach(IReadOnlyList<CanBoDto> danhSach);
}
