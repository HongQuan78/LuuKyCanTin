using System.Text.Json;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Audit;

public sealed class AuditInterceptorTests : IClassFixture<AuditDatabaseFixture>
{
    private const int NguoiDungId = 7;

    private readonly AuditDatabaseFixture _fixture;

    public AuditInterceptorTests(AuditDatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.User.DangNhap(NguoiDungId, "thuquy");
    }

    private static MauChungTu TaoChungTu(string noiDung = "Nộp tiền lưu ký") => new()
    {
        SoTien = 100_000,
        NgayChungTu = new DateOnly(2026, 10, 1),
        NoiDung = noiDung,
        TrangThai = MauTrangThai.DaGhiSo,
        MaBiMat = "bí-mật-ban-đầu",
    };

    private async Task<MauChungTu> ThemAsync(MauChungTu chungTu)
    {
        await using var db = _fixture.TaoDbContextCoNhatKy();
        db.MauChungTu.Add(chungTu);
        await db.SaveChangesAsync();
        return chungTu;
    }

    private async Task SuaAsync(int id, Action<MauChungTu> thayDoi)
    {
        // Load, change, save: the "before" values come from the tracked query.
        await using var db = _fixture.TaoDbContextCoNhatKy();
        thayDoi(await db.MauChungTu.SingleAsync(v => v.Id == id));
        await db.SaveChangesAsync();
    }

    private async Task<List<NhatKyThaoTac>> LayNhatKyAsync(int id)
    {
        await using var db = _fixture.TaoDbContext();
        return await db.NhatKyThaoTac.Where(n => n.TenBang == "MauChungTu" && n.BanGhiId == id).OrderBy(n => n.Id).ToListAsync();
    }

    private async Task<int> LaySoDongNhatKyAsync()
    {
        await using var db = _fixture.TaoDbContext();
        return await db.NhatKyThaoTac.CountAsync();
    }

    private static Dictionary<string, JsonElement> DocJson(string? json) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json.ShouldNotBeNull())!;

    [SqlServerFact]
    public async Task SaveChanges_NewVoucher_WritesThemRowWithNewValues()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        var nhatKy = (await LayNhatKyAsync(chungTu.Id)).ShouldHaveSingleItem();

        nhatKy.HanhDong.ShouldBe(HanhDong.Them);
        nhatKy.ThoiDiem.ShouldBe(_fixture.Clock.Now);
        nhatKy.NguoiDungId.ShouldBe(NguoiDungId);
        nhatKy.MayTram.ShouldBe(Environment.MachineName);
        nhatKy.DuLieuCu.ShouldBeNull();
        var moi = DocJson(nhatKy.DuLieuMoi);
        moi["Id"].GetInt32().ShouldBe(chungTu.Id);
        moi["SoTien"].GetDecimal().ShouldBe(100_000m);
        moi["NoiDung"].GetString().ShouldBe("Nộp tiền lưu ký");
        moi["TrangThai"].GetString().ShouldBe(nameof(MauTrangThai.DaGhiSo));
        moi.Keys.ShouldNotContain("RowVer");
        moi.Keys.ShouldNotContain("NgayTao");
        moi.Keys.ShouldNotContain("NguoiTaoId");
    }

    [SqlServerFact]
    public async Task SaveChanges_VietnameseText_StaysReadableInTheJson()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        (await LayNhatKyAsync(chungTu.Id)).ShouldHaveSingleItem().DuLieuMoi.ShouldNotBeNull().ShouldContain("\"NoiDung\":\"Nộp tiền lưu ký\"");
    }

    [SqlServerFact]
    public async Task SaveChanges_NewVoucher_StoresTheActionAsText()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        (await _fixture.Database.LayGiaTriAsync($"SELECT HanhDong FROM NhatKyThaoTac WHERE TenBang = 'MauChungTu' AND BanGhiId = {chungTu.Id}"))
            .ShouldBe("Them");
    }

    [SqlServerFact]
    public async Task SaveChanges_NewVoucher_FillsCreationAuditColumns()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        await using var db = _fixture.TaoDbContext();
        var chungTuDaLuu = await db.MauChungTu.SingleAsync(v => v.Id == chungTu.Id);
        chungTuDaLuu.NgayTao.ShouldBe(_fixture.Clock.Now);
        chungTuDaLuu.NguoiTaoId.ShouldBe(NguoiDungId);
        chungTuDaLuu.NgaySua.ShouldBeNull();
        chungTuDaLuu.NguoiSuaId.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task SaveChanges_ChangedVoucher_WritesSuaRowWithOnlyTheChangedColumns()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        await SuaAsync(chungTu.Id, v => v.SoTien = 150_000);

        var nhatKy = (await LayNhatKyAsync(chungTu.Id)).Last();
        nhatKy.HanhDong.ShouldBe(HanhDong.Sua);
        var cu = DocJson(nhatKy.DuLieuCu);
        var moi = DocJson(nhatKy.DuLieuMoi);
        cu.Keys.ShouldBe(["SoTien"]);
        moi.Keys.ShouldBe(["SoTien"]);
        cu["SoTien"].GetDecimal().ShouldBe(100_000m);
        moi["SoTien"].GetDecimal().ShouldBe(150_000m);
    }

    [SqlServerFact]
    public async Task SaveChanges_ChangedVoucher_FillsModificationColumnsAndKeepsCreationOnes()
    {
        var chungTu = await ThemAsync(TaoChungTu());
        var ngayTao = _fixture.Clock.Now;
        _fixture.Clock.Advance(TimeSpan.FromMinutes(5));
        _fixture.User.DangNhap(NguoiDungId + 1, "ketoan");
        try
        {
            await SuaAsync(chungTu.Id, v =>
            {
                v.SoTien = 120_000;
                v.NgayTao = DateTime.MinValue;
            });
        }
        finally
        {
            _fixture.User.DangNhap(NguoiDungId, "thuquy");
        }

        await using var db = _fixture.TaoDbContext();
        var chungTuDaLuu = await db.MauChungTu.SingleAsync(v => v.Id == chungTu.Id);
        chungTuDaLuu.NgayTao.ShouldBe(ngayTao);
        chungTuDaLuu.NguoiTaoId.ShouldBe(NguoiDungId);
        chungTuDaLuu.NgaySua.ShouldBe(_fixture.Clock.Now);
        chungTuDaLuu.NguoiSuaId.ShouldBe(NguoiDungId + 1);
    }

    [SqlServerFact]
    public async Task SaveChanges_CancelledVoucher_WritesHuyRowNotSua()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        await SuaAsync(chungTu.Id, v => v.TrangThai = MauTrangThai.DaHuy);

        var nhatKy = (await LayNhatKyAsync(chungTu.Id)).Last();
        nhatKy.HanhDong.ShouldBe(HanhDong.Huy);
        DocJson(nhatKy.DuLieuCu)["TrangThai"].GetString().ShouldBe(nameof(MauTrangThai.DaGhiSo));
        DocJson(nhatKy.DuLieuMoi)["TrangThai"].GetString().ShouldBe(nameof(MauTrangThai.DaHuy));
    }

    [SqlServerFact]
    public async Task SaveChanges_AlreadyCancelledVoucherEdited_WritesSuaRow()
    {
        var chungTu = await ThemAsync(TaoChungTu());
        await SuaAsync(chungTu.Id, v => v.TrangThai = MauTrangThai.DaHuy);

        await SuaAsync(chungTu.Id, v => v.NoiDung = "Huỷ do ghi nhầm đối tượng");

        var nhatKy = (await LayNhatKyAsync(chungTu.Id)).Last();
        nhatKy.HanhDong.ShouldBe(HanhDong.Sua);
        DocJson(nhatKy.DuLieuMoi).Keys.ShouldBe(["NoiDung"]);
    }

    [SqlServerFact]
    public async Task SaveChanges_NoRealChange_WritesNoRow()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        await SuaAsync(chungTu.Id, v => v.SoTien = v.SoTien);

        (await LayNhatKyAsync(chungTu.Id)).ShouldHaveSingleItem().HanhDong.ShouldBe(HanhDong.Them);
    }

    [SqlServerFact]
    public async Task SaveChanges_DetachedUpdate_ThrowsAndWritesNothing()
    {
        var chungTu = await ThemAsync(TaoChungTu());
        var chungTuTachRoi = TaoChungTu();
        chungTuTachRoi.Id = chungTu.Id;
        chungTuTachRoi.SoTien = 999_000;
        chungTuTachRoi.RowVer = chungTu.RowVer;

        await using (var db = _fixture.TaoDbContextCoNhatKy())
        {
            db.MauChungTu.Update(chungTuTachRoi);

            (await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync())).Message.ShouldContain("Load it");
        }

        await using var dbKiemTra = _fixture.TaoDbContext();
        (await dbKiemTra.MauChungTu.SingleAsync(v => v.Id == chungTu.Id)).SoTien.ShouldBe(100_000m);
        (await LayNhatKyAsync(chungTu.Id)).ShouldHaveSingleItem().HanhDong.ShouldBe(HanhDong.Them);
    }

    [SqlServerFact]
    public async Task SaveChanges_ExcludedPropertyChanged_LogsTheChangeWithoutItsValue()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        await SuaAsync(chungTu.Id, v => v.MaBiMat = "bí-mật-mới");

        var danhSachNhatKy = await LayNhatKyAsync(chungTu.Id);
        danhSachNhatKy.Count.ShouldBe(2);
        danhSachNhatKy[1].HanhDong.ShouldBe(HanhDong.Sua);
        danhSachNhatKy.ShouldAllBe(n => !(n.DuLieuCu ?? "").Contains("bí-mật") && !(n.DuLieuMoi ?? "").Contains("bí-mật"));
        danhSachNhatKy.ShouldAllBe(n => !(n.DuLieuCu ?? "").Contains("MaBiMat") && !(n.DuLieuMoi ?? "").Contains("MaBiMat"));
    }

    [SqlServerFact]
    public async Task SaveChanges_InsertFails_RollsBackBothTheEntityAndItsAuditRows()
    {
        var dauHieu = Guid.NewGuid().ToString("N");
        var soDongNhatKy = await LaySoDongNhatKyAsync();

        await using (var db = _fixture.TaoDbContextCoNhatKy())
        {
            db.MauChungTu.Add(TaoChungTu(dauHieu));
            // Violates CK_MauChungTu_TrangThai, so the batch fails after the first insert.
            var chungTuSai = TaoChungTu(dauHieu);
            chungTuSai.TrangThai = (MauTrangThai)9;
            db.MauChungTu.Add(chungTuSai);

            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }

        await using var dbKiemTra = _fixture.TaoDbContext();
        (await dbKiemTra.MauChungTu.CountAsync(v => v.NoiDung == dauHieu)).ShouldBe(0);
        (await LaySoDongNhatKyAsync()).ShouldBe(soDongNhatKy);
    }

    [SqlServerFact]
    public async Task SaveChanges_RetryInSameContextAfterFailure_StoresOnlyTheRetry()
    {
        var dauHieu = Guid.NewGuid().ToString("N");

        await using (var db = _fixture.TaoDbContextCoNhatKy())
        {
            db.MauChungTu.Add(TaoChungTu(dauHieu));
            var chungTuSai = TaoChungTu(dauHieu);
            chungTuSai.TrangThai = (MauTrangThai)9;
            db.MauChungTu.Add(chungTuSai);
            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());

            // The failed save's own transaction is gone, so the retry can't commit leftovers of the failed batch.
            db.Database.CurrentTransaction.ShouldBeNull();

            chungTuSai.TrangThai = MauTrangThai.DaGhiSo;
            await db.SaveChangesAsync();
        }

        await using var dbKiemTra = _fixture.TaoDbContext();
        var danhSachId = await dbKiemTra.MauChungTu.Where(v => v.NoiDung == dauHieu).Select(v => v.Id).ToListAsync();
        danhSachId.Count.ShouldBe(2);
        foreach (var id in danhSachId)
            (await LayNhatKyAsync(id)).ShouldHaveSingleItem().HanhDong.ShouldBe(HanhDong.Them);
    }

    [SqlServerFact]
    public async Task SaveChanges_AuditWriteFails_RollsBackTheEntityToo()
    {
        const string tuChoi = "TU-CHOI-NHAT-KY";
        // Test-only constraint: an audit row mentioning the marker fails, after the voucher INSERT has succeeded.
        await _fixture.Database.ThucThiAsync($"""
            IF OBJECT_ID('CK_Test_TuChoiNhatKy') IS NULL
                ALTER TABLE NhatKyThaoTac ADD CONSTRAINT CK_Test_TuChoiNhatKy CHECK (DuLieuMoi NOT LIKE '%{tuChoi}%')
            """);
        var dauHieu = $"{tuChoi}-{Guid.NewGuid():N}";
        var soDongNhatKy = await LaySoDongNhatKyAsync();

        await using (var db = _fixture.TaoDbContextCoNhatKy())
        {
            db.MauChungTu.Add(TaoChungTu(dauHieu));
            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());

            // The rejected log row must not stay queued for the context's next save.
            db.ChangeTracker.Entries<NhatKyThaoTac>().ShouldBeEmpty();
        }

        await using var dbKiemTra = _fixture.TaoDbContext();
        (await dbKiemTra.MauChungTu.CountAsync(v => v.NoiDung == dauHieu)).ShouldBe(0);
        (await LaySoDongNhatKyAsync()).ShouldBe(soDongNhatKy);
    }

    [SqlServerFact]
    public async Task SaveChanges_CallerTransactionRolledBack_UndoesTheAuditRowsToo()
    {
        var dauHieu = Guid.NewGuid().ToString("N");
        var soDongNhatKy = await LaySoDongNhatKyAsync();

        await using (var db = _fixture.TaoDbContextCoNhatKy())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            db.MauChungTu.Add(TaoChungTu(dauHieu));
            await db.SaveChangesAsync();

            db.Database.CurrentTransaction.ShouldBeSameAs(transaction);
            await transaction.RollbackAsync();
        }

        await using var dbKiemTra = _fixture.TaoDbContext();
        (await dbKiemTra.MauChungTu.CountAsync(v => v.NoiDung == dauHieu)).ShouldBe(0);
        (await LaySoDongNhatKyAsync()).ShouldBe(soDongNhatKy);
    }

    [SqlServerFact]
    public async Task SaveChanges_CallerTransactionCommitted_KeepsEntityAndAuditRow()
    {
        await using var db = _fixture.TaoDbContextCoNhatKy();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var chungTu = TaoChungTu();
        db.MauChungTu.Add(chungTu);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        (await LayNhatKyAsync(chungTu.Id)).ShouldHaveSingleItem().HanhDong.ShouldBe(HanhDong.Them);
    }

    [SqlServerFact]
    public void SaveChanges_Synchronous_IsAuditedToo()
    {
        var chungTu = TaoChungTu();
        using (var db = _fixture.TaoDbContextCoNhatKy())
        {
            db.MauChungTu.Add(chungTu);
            db.SaveChanges();
        }

        LayNhatKyAsync(chungTu.Id).GetAwaiter().GetResult().ShouldHaveSingleItem().HanhDong.ShouldBe(HanhDong.Them);
    }

    [SqlServerFact]
    public async Task SaveChanges_DeletedVoucher_Throws()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        await using var db = _fixture.TaoDbContextCoNhatKy();
        db.MauChungTu.Remove(await db.MauChungTu.SingleAsync(v => v.Id == chungTu.Id));

        (await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync())).Message.ShouldContain("cancel");
    }

    [SqlServerFact]
    public async Task SaveChanges_ModifiedAuditRow_Throws()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        await using var db = _fixture.TaoDbContextCoNhatKy();
        var nhatKy = await db.NhatKyThaoTac.FirstAsync(n => n.TenBang == "MauChungTu" && n.BanGhiId == chungTu.Id);
        db.Entry(nhatKy).Property(n => n.DuLieuMoi).CurrentValue = "{}";

        (await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync())).Message.ShouldContain("append-only");
    }

    [SqlServerFact]
    public async Task SaveChanges_DeletedAuditRow_Throws()
    {
        var chungTu = await ThemAsync(TaoChungTu());

        await using var db = _fixture.TaoDbContextCoNhatKy();
        db.NhatKyThaoTac.Remove(await db.NhatKyThaoTac.FirstAsync(n => n.TenBang == "MauChungTu" && n.BanGhiId == chungTu.Id));

        Should.Throw<InvalidOperationException>(() => db.SaveChanges()).Message.ShouldContain("append-only");
    }
}
