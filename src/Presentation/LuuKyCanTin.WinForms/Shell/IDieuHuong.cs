namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Opens the app's screens, so the shell's presenter never creates a Form itself.</summary>
public interface IDieuHuong
{
    void MoDanhMucCanBo();

    /// <summary>Opens the role and permission screen.</summary>
    void MoVaiTro();

    /// <summary>Opens the voluntary change-password dialog for the signed-in user.</summary>
    void MoDoiMatKhau();
}
