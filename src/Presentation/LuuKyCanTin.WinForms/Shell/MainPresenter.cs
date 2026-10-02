using LuuKyCanTin.WinForms.Common;
using Microsoft.Extensions.Options;

namespace LuuKyCanTin.WinForms.Shell;

public sealed class MainPresenter
{
    private readonly IMainView _view;
    private readonly AppOptions _options;

    public MainPresenter(IMainView view, IOptions<AppOptions> options, IDieuHuong dieuHuong)
    {
        _view = view;
        _options = options.Value;
        _view.Loaded += OnLoaded;
        _view.DanhMucCanBoClicked += (_, _) => dieuHuong.MoDanhMucCanBo();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        _view.TieuDe = _options.TieuDe;
    }
}
