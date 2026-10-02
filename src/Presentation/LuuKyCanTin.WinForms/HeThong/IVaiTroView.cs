using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>The "Vai trò" screen: role list on the left, module × action checkboxes on the right.</summary>
public interface IVaiTroView
{
    event EventHandler Loaded;

    /// <summary>The selected role changed; load its permission set.</summary>
    event EventHandler VaiTroThayDoi;

    event EventHandler LuuClicked;

    /// <summary>Discard the edits and reload from the database.</summary>
    event EventHandler HuyThayDoiClicked;

    int? VaiTroDangChon { get; }

    /// <summary>Every ticked permission code, in the grid and in the special list.</summary>
    IReadOnlyList<string> QuyenDaChon { get; }

    void HienDanhSachVaiTro(IReadOnlyList<VaiTroDto> danhSach);

    void HienQuyen(IReadOnlyList<string> maQuyen);

    void HienThongBao(string thongBao);

    void HienLoi(string thongBao);
}
