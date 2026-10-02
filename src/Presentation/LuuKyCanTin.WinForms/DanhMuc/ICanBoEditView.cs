namespace LuuKyCanTin.WinForms.DanhMuc;

/// <summary>The add/edit dialog for one staff member.</summary>
public interface ICanBoEditView : IDisposable
{
    event EventHandler LuuClicked;

    string TieuDe { set; }

    string MaCanBo { get; set; }

    string HoTen { get; set; }

    string ChucVu { get; set; }

    bool LaQuanGiao { get; set; }

    bool DangCongTac { get; set; }

    /// <summary>Shows the dialog modally; true if it closed after a successful save.</summary>
    bool HienThi();

    /// <summary>Shows why the save failed and keeps the dialog open so the user can correct it.</summary>
    void HienLoi(string thongBao);

    void DongDaLuu();
}
