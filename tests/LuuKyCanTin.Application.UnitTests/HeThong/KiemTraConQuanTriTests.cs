using LuuKyCanTin.Application.HeThong;
using Shouldly;

namespace LuuKyCanTin.Application.UnitTests.HeThong;

/// <summary>
/// The pure decision behind the last-administrator guard: a snapshot of accounts × roles in, "is anyone left?" out.
/// </summary>
public class KiemTraConQuanTriTests
{
    private const int VaiTroQuanTri = 1;
    private const int VaiTroKeToan = 2;

    private static readonly Dictionary<int, IReadOnlyCollection<string>> QuyenTheoVaiTro = new()
    {
        [VaiTroQuanTri] = [MaQuyen.HT.Xem, MaQuyen.HT.Sua],
        [VaiTroKeToan] = [MaQuyen.LKBC.Xem, MaQuyen.HHBC.Xem],
    };

    private static KiemTraConQuanTri.HienTrangTaiKhoan TaiKhoan(int id, bool dangHoatDong, params int[] vaiTroIds) =>
        new(id, dangHoatDong, vaiTroIds);

    [Fact]
    public void NoAccounts_LeavesNobody()
    {
        KiemTraConQuanTri.ConQuanTri([], QuyenTheoVaiTro).ShouldBeFalse();
    }

    [Fact]
    public void ActiveAccountWithTheAdminRole_IsEnough()
    {
        KiemTraConQuanTri.ConQuanTri([TaiKhoan(1, dangHoatDong: true, VaiTroQuanTri)], QuyenTheoVaiTro)
            .ShouldBeTrue();
    }

    [Fact]
    public void InactiveAdmin_DoesNotCount()
    {
        KiemTraConQuanTri.ConQuanTri([TaiKhoan(1, dangHoatDong: false, VaiTroQuanTri)], QuyenTheoVaiTro)
            .ShouldBeFalse();
    }

    [Fact]
    public void ActiveAccountWithoutTheAdminRole_DoesNotCount()
    {
        KiemTraConQuanTri.ConQuanTri([TaiKhoan(1, dangHoatDong: true, VaiTroKeToan)], QuyenTheoVaiTro)
            .ShouldBeFalse();
    }

    [Fact]
    public void OneAdminAmongSeveralAccounts_IsEnough()
    {
        KiemTraConQuanTri.ConQuanTri(
            [
                TaiKhoan(1, dangHoatDong: true, VaiTroKeToan),
                TaiKhoan(2, dangHoatDong: false, VaiTroQuanTri),
                TaiKhoan(3, dangHoatDong: true, VaiTroQuanTri),
            ],
            QuyenTheoVaiTro).ShouldBeTrue();
    }

    [Fact]
    public void UnknownRoleId_DoesNotCount()
    {
        KiemTraConQuanTri.ConQuanTri([TaiKhoan(1, dangHoatDong: true, 99)], QuyenTheoVaiTro).ShouldBeFalse();
    }
}
