namespace LuuKyCanTin.WinForms.Shell;

public interface ILoginView
{
    event EventHandler? DangNhapBam;

    string TenDangNhap { get; }

    string MatKhau { get; }

    void HienLoi(string thongBao);

    /// <summary>Clears the password box, for a locked account: retyping the same password can't help.</summary>
    void XoaMatKhau();

    /// <summary>Closes the form; true sets <see cref="DialogResult.OK"/> so the shell may open.</summary>
    void DongVoiKetQua(bool thanhCong);
}
