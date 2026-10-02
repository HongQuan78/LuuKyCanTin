using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.WinForms.DanhMuc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace LuuKyCanTin.WinForms.UnitTests.DanhMuc;

public class CanBoEditPresenterTests
{
    private static readonly CanBoDto DangCo = new(5, "CB05", "Lê Thị Bình", "Kế toán", false, true, [1, 2, 3]);

    private readonly ICanBoEditView _view = Substitute.For<ICanBoEditView>();
    private readonly ICanBoService _service = Substitute.For<ICanBoService>();
    private readonly IServiceScopeFactory _scopes;

    public CanBoEditPresenterTests()
    {
        _scopes = new ServiceCollection().AddScoped(_ => _service).BuildServiceProvider().GetRequiredService<IServiceScopeFactory>();
    }

    private void Fill(string ma = "cb01", string hoTen = "Nguyễn Văn An", string chucVu = "Quản giáo", bool quanGiao = true, bool dangCongTac = true)
    {
        _view.MaCanBo.Returns(ma);
        _view.HoTen.Returns(hoTen);
        _view.ChucVu.Returns(chucVu);
        _view.LaQuanGiao.Returns(quanGiao);
        _view.DangCongTac.Returns(dangCongTac);
    }

    [Fact]
    public void Adding_StartsWithAnEmptyActiveStaffMember()
    {
        _ = new CanBoEditPresenter(_view, _scopes, canBo: null);

        _view.Received().TieuDe = "Thêm cán bộ";
        _view.Received().DangCongTac = true;
        _view.DidNotReceive().MaCanBo = Arg.Any<string>();
    }

    [Fact]
    public void Editing_ShowsTheCurrentValues()
    {
        _ = new CanBoEditPresenter(_view, _scopes, DangCo);

        _view.Received().TieuDe = "Sửa cán bộ";
        _view.Received().MaCanBo = "CB05";
        _view.Received().HoTen = "Lê Thị Bình";
        _view.Received().ChucVu = "Kế toán";
        _view.Received().LaQuanGiao = false;
        _view.Received().DangCongTac = true;
    }

    [Fact]
    public void Save_WhenAdding_AddsAndCloses()
    {
        Fill();
        _ = new CanBoEditPresenter(_view, _scopes, canBo: null);

        _view.LuuClicked += Raise.Event();

        _service.Received(1).ThemAsync(new LuuCanBoRequest("cb01", "Nguyễn Văn An", "Quản giáo", true, true), Arg.Any<CancellationToken>());
        _view.Received(1).DongDaLuu();
    }

    [Fact]
    public void Save_WhenEditing_SendsTheLoadedRowVersion()
    {
        _ = new CanBoEditPresenter(_view, _scopes, DangCo);
        // What the user typed over the loaded values.
        Fill(ma: "CB05", dangCongTac: false);

        _view.LuuClicked += Raise.Event();

        _service.Received(1).SuaAsync(5, Arg.Is<LuuCanBoRequest>(r => r.MaCanBo == "CB05" && !r.DangCongTac && r.RowVer == DangCo.RowVer), Arg.Any<CancellationToken>());
        _view.Received(1).DongDaLuu();
    }

    [Fact]
    public void SaveError_IsShown_AndTheDialogStaysOpen()
    {
        Fill();
        _service.ThemAsync(Arg.Any<LuuCanBoRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new LoiNghiepVuException("Mã cán bộ đã tồn tại"));
        _ = new CanBoEditPresenter(_view, _scopes, canBo: null);

        _view.LuuClicked += Raise.Event();

        _view.Received(1).HienLoi("Mã cán bộ đã tồn tại");
        _view.DidNotReceive().DongDaLuu();
    }

    [Fact]
    public void ConcurrencyConflict_IsShownToo()
    {
        _service.SuaAsync(Arg.Any<int>(), Arg.Any<LuuCanBoRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new XungDotDuLieuException(new InvalidOperationException()));
        _ = new CanBoEditPresenter(_view, _scopes, DangCo);
        Fill();

        _view.LuuClicked += Raise.Event();

        _view.Received(1).HienLoi("Dữ liệu đã bị người khác thay đổi, vui lòng tải lại");
        _view.DidNotReceive().DongDaLuu();
    }

    [Fact]
    public void EachSave_UsesAFreshScope()
    {
        var scopes = Substitute.For<IServiceScopeFactory>();
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.GetService(typeof(ICanBoService)).Returns(_service);
        scopes.CreateScope().Returns(scope);
        _service.ThemAsync(Arg.Any<LuuCanBoRequest>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new LoiNghiepVuException("x"));
        Fill();
        _ = new CanBoEditPresenter(_view, scopes, canBo: null);

        _view.LuuClicked += Raise.Event();
        _view.LuuClicked += Raise.Event();

        scopes.Received(2).CreateScope();
        scope.Received(2).Dispose();
    }
}
