using LuuKyCanTin.Domain.HeThong;
using Shouldly;

namespace LuuKyCanTin.Domain.UnitTests.HeThong;

public class NguoiDungTests
{
    private static readonly DateTime BayGio = new(2026, 10, 2, 9, 0, 0);
    private static readonly TimeSpan MuoiLamPhut = TimeSpan.FromMinutes(15);

    private static NguoiDung TaiKhoan() => new() { Id = 1, TenDangNhap = "admin" };

    [Fact]
    public void GhiNhanDangNhapSai_FourthAttempt_DoesNotLock()
    {
        var nguoiDung = TaiKhoan();

        for (var lan = 0; lan < NguoiDung.SoLanSaiToiDa - 1; lan++)
            nguoiDung.GhiNhanDangNhapSai(BayGio, MuoiLamPhut).ShouldBeFalse();

        nguoiDung.SoLanSai.ShouldBe((byte)4);
        nguoiDung.KhoaDen.ShouldBeNull();
    }

    [Fact]
    public void GhiNhanDangNhapSai_FifthAttempt_LocksForTheConfiguredPeriod()
    {
        var nguoiDung = TaiKhoan();
        for (var lan = 0; lan < NguoiDung.SoLanSaiToiDa - 1; lan++)
            nguoiDung.GhiNhanDangNhapSai(BayGio, MuoiLamPhut);

        var vuaKhoa = nguoiDung.GhiNhanDangNhapSai(BayGio, MuoiLamPhut);

        vuaKhoa.ShouldBeTrue();
        nguoiDung.SoLanSai.ShouldBe(NguoiDung.SoLanSaiToiDa);
        nguoiDung.KhoaDen.ShouldBe(BayGio + MuoiLamPhut);
        nguoiDung.DangBiKhoa(BayGio).ShouldBeTrue();
    }

    [Fact]
    public void GhiNhanDangNhapSai_WithoutAPeriod_LocksUntilAnAdministratorUnlocks()
    {
        var nguoiDung = TaiKhoan();
        for (var lan = 0; lan < NguoiDung.SoLanSaiToiDa - 1; lan++)
            nguoiDung.GhiNhanDangNhapSai(BayGio, thoiGianKhoa: null);

        nguoiDung.GhiNhanDangNhapSai(BayGio, thoiGianKhoa: null).ShouldBeTrue();

        nguoiDung.KhoaDen.ShouldBe(new DateTime(9999, 12, 31));
        nguoiDung.DangBiKhoa(BayGio.AddYears(100)).ShouldBeTrue();
    }

    [Fact]
    public void DangBiKhoa_WhenTheLockExpired_IsOpenAgain()
    {
        var nguoiDung = TaiKhoan();
        for (var lan = 0; lan < NguoiDung.SoLanSaiToiDa; lan++)
            nguoiDung.GhiNhanDangNhapSai(BayGio, MuoiLamPhut);

        nguoiDung.DangBiKhoa(BayGio.AddMinutes(16)).ShouldBeFalse();
    }

    [Fact]
    public void DangBiKhoa_WithoutALock_IsOpen()
    {
        TaiKhoan().DangBiKhoa(BayGio).ShouldBeFalse();
    }

    [Fact]
    public void GhiNhanDangNhapDung_ResetsTheFailuresAndTheLock()
    {
        var nguoiDung = TaiKhoan();
        for (var lan = 0; lan < NguoiDung.SoLanSaiToiDa; lan++)
            nguoiDung.GhiNhanDangNhapSai(BayGio, MuoiLamPhut);

        nguoiDung.GhiNhanDangNhapDung();

        nguoiDung.SoLanSai.ShouldBe((byte)0);
        nguoiDung.KhoaDen.ShouldBeNull();
    }

    [Fact]
    public void DoiMatKhau_StoresTheNewHashAndClearsTheForcedChange()
    {
        var nguoiDung = TaiKhoan();
        nguoiDung.PhaiDoiMatKhau = true;

        nguoiDung.DoiMatKhau("PBKDF2-SHA256$600000$moi$moi");

        nguoiDung.MatKhauHash.ShouldBe("PBKDF2-SHA256$600000$moi$moi");
        nguoiDung.PhaiDoiMatKhau.ShouldBeFalse();
    }
}
