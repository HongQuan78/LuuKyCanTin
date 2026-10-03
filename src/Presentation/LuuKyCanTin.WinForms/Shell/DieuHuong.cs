using LuuKyCanTin.WinForms.DanhMuc;
using LuuKyCanTin.WinForms.HeThong;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Composes each screen with its presenter. Presenters get scopes, never a DbContext.</summary>
internal sealed class DieuHuong(IServiceScopeFactory scopes) : IDieuHuong
{
    private CanBoForm? _canBo;
    private VaiTroForm? _vaiTro;
    private TaiKhoanForm? _taiKhoan;

    // One staff window at a time: a second menu click brings the open one forward.
    public void MoDanhMucCanBo()
    {
        if (_canBo is { IsDisposed: false })
        {
            _canBo.Activate();
            return;
        }

        _canBo = new CanBoForm();
        _ = new CanBoPresenter(_canBo, scopes, () => new CanBoEditForm());
        _canBo.Show();
    }

    public void MoDoiMatKhau()
    {
        using var form = new DoiMatKhauForm();
        _ = new DoiMatKhauPresenter(form, scopes, batBuoc: false);
        form.ShowDialog();
    }

    // One role window at a time, like the staff register.
    public void MoVaiTro()
    {
        if (_vaiTro is { IsDisposed: false })
        {
            _vaiTro.Activate();
            return;
        }

        _vaiTro = new VaiTroForm();
        _ = new VaiTroPresenter(_vaiTro, scopes);
        _vaiTro.Show();
    }

    // One account window at a time. The temporary password dialog belongs to the account screen, not the shell.
    public void MoTaiKhoan()
    {
        if (_taiKhoan is { IsDisposed: false })
        {
            _taiKhoan.Activate();
            return;
        }

        _taiKhoan = new TaiKhoanForm();
        _ = new TaiKhoanPresenter(
            _taiKhoan,
            scopes,
            () => new TaoTaiKhoanForm(),
            () => new TaiKhoanVaiTroForm(),
            HienMatKhauTam);
        _taiKhoan.Show();
    }

    private static void HienMatKhauTam(string matKhauTam)
    {
        using var hopThoai = new MatKhauTamForm(matKhauTam);
        hopThoai.ShowDialog();
    }
}
