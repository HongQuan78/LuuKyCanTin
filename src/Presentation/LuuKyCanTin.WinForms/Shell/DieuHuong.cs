using LuuKyCanTin.WinForms.DanhMuc;
using Microsoft.Extensions.DependencyInjection;

namespace LuuKyCanTin.WinForms.Shell;

/// <summary>Composes each screen with its presenter. Presenters get scopes, never a DbContext.</summary>
internal sealed class DieuHuong(IServiceScopeFactory scopes) : IDieuHuong
{
    private CanBoForm? _canBo;

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
}
