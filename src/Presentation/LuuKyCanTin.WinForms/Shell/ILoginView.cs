namespace LuuKyCanTin.WinForms.Shell;

public interface ILoginView
{
    event EventHandler? DangNhapBam;

    string TenDangNhap { get; }

    string MatKhau { get; }

    void HienLoi(string thongBao);

    /// <summary>Closes the form; true sets <see cref="DialogResult.OK"/> so the shell may open.</summary>
    void DongVoiKetQua(bool thanhCong);
}
