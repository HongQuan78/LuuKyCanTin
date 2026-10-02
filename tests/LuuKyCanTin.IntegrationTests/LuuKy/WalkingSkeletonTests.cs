using LuuKyCanTin.Application;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.BaoCao;
using LuuKyCanTin.Application.DanhMuc;
using LuuKyCanTin.Application.HeThong;
using LuuKyCanTin.Application.LuuKy;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.DanhMuc;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Domain.LuuKy;
using LuuKyCanTin.Infrastructure;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Infrastructure;
using LuuKyCanTin.IntegrationTests.TestUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using UglyToad.PdfPig;

namespace LuuKyCanTin.IntegrationTests.LuuKy;

/// <summary>
/// The Sprint 0 gate: sign in, add one detainee, post one 500,000 đ receipt through the real ledger engine
/// and print it — on LocalDB/SQL Server, with the real migrations, DI, interceptor and report renderer.
/// </summary>
public class WalkingSkeletonTests(WalkingSkeletonFixture fixture) : IClassFixture<WalkingSkeletonFixture>
{
    // The initial password seeded by AddWalkingSkeletonTables; see docs/install.md. Epic 2 forces a change.
    private const string MatKhauAdmin = "LuuKy@2026";

    private static readonly DateOnly NgayChungTu = new(2026, 10, 1);

    private async Task ResetAsync()
    {
        await fixture.Database.ExecuteAsync("""
            DELETE FROM NhatKyThaoTac;
            DELETE FROM ChungTuLuuKy;
            DELETE FROM DoiTuong;
            UPDATE DemSoChungTu SET SoHienTai = 0;
            UPDATE ThongTinDonVi
            SET TenCoQuanChuQuan = N'CÔNG AN TỈNH ABC',
                TenDonVi = N'TRẠI TẠM GIAM ABC',
                DiaChi = N'Xã ABC, huyện ABC, tỉnh ABC'
            WHERE Id = 1;
            """);
    }

    private async Task<int> DangNhapAdminAsync()
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var ketQua = await scope.ServiceProvider.GetRequiredService<DangNhapService>().DangNhapAsync("admin", MatKhauAdmin);
        ketQua.ThanhCong.ShouldBeTrue(ketQua.ThongBao);
        return scope.ServiceProvider.GetRequiredService<ICurrentUser>().NguoiDungId.ShouldNotBeNull();
    }

    private async Task<int> ThemDoiTuongAsync(string maSo = "DT-0001")
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        var ketQua = await scope.ServiceProvider.GetRequiredService<ThemDoiTuongService>().ThemAsync(
            new ThemDoiTuongRequest(maSo, "Nguyễn Văn A", 1990, LoaiDoiTuong.TamGiuTamGiam, new DateOnly(2026, 9, 1), "A3"));

        ketQua.ThanhCong.ShouldBeTrue(ketQua.ThongBao);
        return ketQua.Id;
    }

    private static GhiSoBienNhanThuRequest BienNhan(int doiTuongId) => new()
    {
        DoiTuongId = doiTuongId,
        NgayChungTu = NgayChungTu,
        NghiepVu = NghiepVu.NguoiThanGui,
        HinhThuc = HinhThuc.TienMat,
        NguoiGuiHoTen = "Trần Thị B",
        QuanHe = "Mẹ",
        NoiDung = "Tiền gửi lưu ký",
        SoTien = 500_000,
    };

    [SqlServerFact]
    public async Task Walk_SignInAddDetaineePostReceiptAndPrint()
    {
        await ResetAsync();
        var adminId = await DangNhapAdminAsync();
        var doiTuongId = await ThemDoiTuongAsync();

        KetQuaGhiSo ketQua;
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            ketQua = await scope.ServiceProvider.GetRequiredService<GhiSoLuuKyService>()
                .GhiSoBienNhanThuAsync(BienNhan(doiTuongId));
        }

        ketQua.ThanhCong.ShouldBeTrue(ketQua.ThongBao);
        ketQua.SoChungTu.ShouldBe("BNT-2026-00001");
        ketQua.SoDuSau.ShouldBe(500_000m);

        await using (var db = fixture.Database.CreateDbContext())
        {
            var chungTu = await db.ChungTuLuuKy.SingleAsync(c => c.Id == ketQua.Id);
            chungTu.SoChungTu.ShouldBe("BNT-2026-00001");
            chungTu.TrangThai.ShouldBe(TrangThaiChungTu.DaGhiSo);
            chungTu.SoDuTruoc.ShouldBe(0m);
            chungTu.SoDuSau.ShouldBe(500_000m);
            chungTu.SoTienBangChu.ShouldBe("Năm trăm nghìn đồng");
            chungTu.HoTenDoiTuong.ShouldBe("Nguyễn Văn A");
            chungTu.LoaiDoiTuong.ShouldBe(LoaiDoiTuong.TamGiuTamGiam);
            chungTu.NgayChungTu.ShouldBe(NgayChungTu);

            (await db.DoiTuong.SingleAsync(d => d.Id == doiTuongId)).SoDuLuuKy.ShouldBe(500_000m);
            (await db.DemSoChungTu.SingleAsync(d => d.LoaiChungTu == "BNT" && d.Nam == 2026)).SoHienTai.ShouldBe(1);

            var nhatKy = await db.NhatKyThaoTac.OrderBy(n => n.Id).ToListAsync();
            nhatKy.Count.ShouldBe(2);
            var themRow = nhatKy.Single(n => n.HanhDong == HanhDong.Them);
            themRow.TenBang.ShouldBe("ChungTuLuuKy");
            themRow.BanGhiId.ShouldBe(ketQua.Id);
            nhatKy.Single(n => n.HanhDong == HanhDong.DangNhap).NguoiDungId.ShouldBe(adminId);
        }

        // Print: the model reads the stored snapshot plus ThongTinDonVi, and the renderer emits a real PDF.
        await using (var scope = fixture.Services.CreateAsyncScope())
        {
            var model = (await scope.ServiceProvider.GetRequiredService<LayBienNhanThuDeInQuery>()
                .LayAsync(ketQua.Id)).ShouldNotBeNull();

            model.TenDonVi.ShouldBe("TRẠI TẠM GIAM ABC");
            model.HoTenDoiTuong.ShouldBe("Nguyễn Văn A");
            model.SoTien.ShouldBe(500_000m);
            model.SoTienBangChu.ShouldBe("Năm trăm nghìn đồng");

            var pdf = scope.ServiceProvider.GetRequiredService<IReportRenderer>().Render(model);
            pdf.Take(4).ShouldBe([0x25, 0x50, 0x44, 0x46]);

            using var document = PdfDocument.Open(pdf);
            var text = string.Concat(document.GetPages().Select(p => p.Text));
            text.ShouldContain("TRẠI TẠM GIAM ABC");
            text.ShouldContain("BNT-2026-00001");
            text.ShouldContain("Nguyễn Văn A");
            text.ShouldContain("500.000");
            text.ShouldContain("Năm trăm nghìn đồng");
        }

        await LedgerReconciliation.AssertBalancedAsync(fixture.Database);
    }

    [SqlServerFact]
    public async Task Rollback_AfterNumbering_ReturnsTheNumberAndLeavesNoTrace()
    {
        await ResetAsync();
        await DangNhapAdminAsync();
        var doiTuongId = await ThemDoiTuongAsync("DT-0002");

        var writer = Substitute.For<ISoDuLuuKyWriter>();
        writer.CongAsync(Arg.Any<int>(), Arg.Any<decimal>(), Arg.Any<CancellationToken>())
            .Returns(_ => Task.FromResult<(decimal SoDuTruoc, decimal SoDuSau)?>(null));

        await using var scope = fixture.Services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var ghiSo = new GhiSoLuuKyService(
            sp.GetRequiredService<IDoiTuongStore>(),
            sp.GetRequiredService<IChungTuLuuKyStore>(),
            sp.GetRequiredService<IAppDbContext>(),
            sp.GetRequiredService<INumberingService>(),
            writer,
            sp.GetRequiredService<IClock>());

        var ketQua = await ghiSo.GhiSoBienNhanThuAsync(BienNhan(doiTuongId));

        ketQua.ThanhCong.ShouldBeFalse();
        Convert.ToInt32(await fixture.Database.ScalarAsync("SELECT COUNT(*) FROM ChungTuLuuKy")).ShouldBe(0);
        Convert.ToInt32(await fixture.Database.ScalarAsync("SELECT COUNT(*) FROM NhatKyThaoTac WHERE HanhDong = 'Them'")).ShouldBe(0);
        Convert.ToDecimal(await fixture.Database.ScalarAsync($"SELECT SoDuLuuKy FROM DoiTuong WHERE Id = {doiTuongId}")).ShouldBe(0m);
        Convert.ToInt32(await fixture.Database.ScalarAsync("SELECT SoHienTai FROM DemSoChungTu WHERE LoaiChungTu = 'BNT' AND Nam = 2026"))
            .ShouldBe(0, "a rolled-back posting must not consume a document number");

        await LedgerReconciliation.AssertBalancedAsync(fixture.Database);
    }
}

/// <summary>A dedicated database per test class, migrated from scratch, wired like the app's DI.</summary>
public sealed class WalkingSkeletonFixture : IAsyncLifetime
{
    public TestDatabase Database { get; } = new();

    public FakeClock Clock { get; } = new(new DateTime(2026, 10, 1, 8, 30, 0));

    public ServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        if (!SqlServerFactAttribute.ShouldRun)
            return;

        await Database.MigrateAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:LuuKyCanTin"] = Database.ConnectionString,
            })
            .Build();

        var services = new ServiceCollection()
            .AddApplication()
            .AddInfrastructure(configuration);

        // The walking skeleton is dated once, so numbering and documents are deterministic.
        services.AddSingleton<IClock>(Clock);

        Services = services.BuildServiceProvider();
    }

    public async Task DisposeAsync()
    {
        if (!SqlServerFactAttribute.ShouldRun)
            return;

        await Services.DisposeAsync();
        await Database.DisposeAsync();
    }
}
