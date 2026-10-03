using LuuKyCanTin.Application.HeThong;

namespace LuuKyCanTin.WinForms.HeThong;

/// <summary>The role dialog for one account: a checked list of the 6 standard roles.</summary>
public interface ITaiKhoanVaiTroView : IDisposable
{
    event EventHandler Loaded;

    event EventHandler LuuClicked;

    string TieuDe { set; }

    IReadOnlyList<int> VaiTroDaChon { get; }

    void HienDanhSachVaiTro(IReadOnlyList<VaiTroDto> danhSach);

    void HienVaiTroDaChon(IReadOnlyList<int> vaiTroIds);

    /// <summary>Shows the dialog modally; true if it closed after a successful save.</summary>
    bool HienThi();

    void HienLoi(string thongBao);

    void DongDaLuu();
}
