using System.Text.Json;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.IntegrationTests.Common;
using LuuKyCanTin.IntegrationTests.Persistence.TestModel;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace LuuKyCanTin.IntegrationTests.Persistence.Audit;

public class AuditInterceptorTests : IClassFixture<AuditDatabaseFixture>
{
    private const int NguoiDungId = 7;

    private readonly AuditDatabaseFixture _fixture;

    public AuditInterceptorTests(AuditDatabaseFixture fixture)
    {
        _fixture = fixture;
            _fixture.User.DangNhap(NguoiDungId, "thuquy", canBoId: null, hoTen: "thuquy");
    }

    private static MauChungTu NewVoucher(string noiDung = "Nộp tiền lưu ký") => new()
    {
        SoTien = 100_000,
        NgayChungTu = new DateOnly(2026, 10, 1),
        NoiDung = noiDung,
        TrangThai = MauTrangThai.DaGhiSo,
        MaBiMat = "bí-mật-ban-đầu",
    };

    private async Task<MauChungTu> InsertAsync(MauChungTu voucher)
    {
        await using var db = _fixture.CreateAuditedContext();
        db.MauChungTu.Add(voucher);
        await db.SaveChangesAsync();
        return voucher;
    }

    private async Task UpdateAsync(int id, Action<MauChungTu> change)
    {
        // Load, change, save: the "before" values come from the tracked query.
        await using var db = _fixture.CreateAuditedContext();
        change(await db.MauChungTu.SingleAsync(v => v.Id == id));
        await db.SaveChangesAsync();
    }

    private async Task<List<NhatKyThaoTac>> LogOfAsync(int id)
    {
        await using var db = _fixture.CreatePlainContext();
        return await db.NhatKyThaoTac.Where(n => n.TenBang == "MauChungTu" && n.BanGhiId == id).OrderBy(n => n.Id).ToListAsync();
    }

    private async Task<int> LogCountAsync()
    {
        await using var db = _fixture.CreatePlainContext();
        return await db.NhatKyThaoTac.CountAsync();
    }

    private static Dictionary<string, JsonElement> Json(string? json) =>
        JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json.ShouldNotBeNull())!;

    [SqlServerFact]
    public async Task Insert_WritesThemRowWithNewValues()
    {
        var voucher = await InsertAsync(NewVoucher());

        var row = (await LogOfAsync(voucher.Id)).ShouldHaveSingleItem();

        row.HanhDong.ShouldBe(HanhDong.Them);
        row.ThoiDiem.ShouldBe(_fixture.Clock.Now);
        row.NguoiDungId.ShouldBe(NguoiDungId);
        row.MayTram.ShouldBe(Environment.MachineName);
        row.DuLieuCu.ShouldBeNull();
        var moi = Json(row.DuLieuMoi);
        moi["Id"].GetInt32().ShouldBe(voucher.Id);
        moi["SoTien"].GetDecimal().ShouldBe(100_000m);
        moi["NoiDung"].GetString().ShouldBe("Nộp tiền lưu ký");
        moi.Keys.ShouldNotContain("RowVer");
        moi.Keys.ShouldNotContain("NgayTao");
        moi.Keys.ShouldNotContain("NguoiTaoId");
    }

    [SqlServerFact]
    public async Task Insert_KeepsVietnameseReadableInTheJson()
    {
        var voucher = await InsertAsync(NewVoucher());

        (await LogOfAsync(voucher.Id)).ShouldHaveSingleItem().DuLieuMoi.ShouldNotBeNull().ShouldContain("\"NoiDung\":\"Nộp tiền lưu ký\"");
    }

    [SqlServerFact]
    public async Task Insert_StoresTheActionAsText()
    {
        var voucher = await InsertAsync(NewVoucher());

        (await _fixture.Database.ScalarAsync($"SELECT HanhDong FROM NhatKyThaoTac WHERE TenBang = 'MauChungTu' AND BanGhiId = {voucher.Id}"))
            .ShouldBe("Them");
    }

    [SqlServerFact]
    public async Task Insert_FillsCreationAuditColumns()
    {
        var voucher = await InsertAsync(NewVoucher());

        await using var db = _fixture.CreatePlainContext();
        var stored = await db.MauChungTu.SingleAsync(v => v.Id == voucher.Id);
        stored.NgayTao.ShouldBe(_fixture.Clock.Now);
        stored.NguoiTaoId.ShouldBe(NguoiDungId);
        stored.NgaySua.ShouldBeNull();
        stored.NguoiSuaId.ShouldBeNull();
    }

    [SqlServerFact]
    public async Task Update_WritesSuaRowWithOnlyTheChangedColumns()
    {
        var voucher = await InsertAsync(NewVoucher());

        await UpdateAsync(voucher.Id, v => v.SoTien = 150_000);

        var row = (await LogOfAsync(voucher.Id)).Last();
        row.HanhDong.ShouldBe(HanhDong.Sua);
        var cu = Json(row.DuLieuCu);
        var moi = Json(row.DuLieuMoi);
        cu.Keys.ShouldBe(["SoTien"]);
        moi.Keys.ShouldBe(["SoTien"]);
        cu["SoTien"].GetDecimal().ShouldBe(100_000m);
        moi["SoTien"].GetDecimal().ShouldBe(150_000m);
    }

    [SqlServerFact]
    public async Task Update_FillsModificationAuditColumns_AndKeepsCreationOnes()
    {
        var voucher = await InsertAsync(NewVoucher());
        var createdAt = _fixture.Clock.Now;
        _fixture.Clock.Advance(TimeSpan.FromMinutes(5));
        _fixture.User.DangNhap(NguoiDungId + 1, "ketoan", canBoId: null, hoTen: "ketoan");
        try
        {
            await UpdateAsync(voucher.Id, v =>
            {
                v.SoTien = 120_000;
                v.NgayTao = DateTime.MinValue;
            });
        }
        finally
        {
        _fixture.User.DangNhap(NguoiDungId, "thuquy", canBoId: null, hoTen: "thuquy");
        }

        await using var db = _fixture.CreatePlainContext();
        var stored = await db.MauChungTu.SingleAsync(v => v.Id == voucher.Id);
        stored.NgayTao.ShouldBe(createdAt);
        stored.NguoiTaoId.ShouldBe(NguoiDungId);
        stored.NgaySua.ShouldBe(_fixture.Clock.Now);
        stored.NguoiSuaId.ShouldBe(NguoiDungId + 1);
    }

    [SqlServerFact]
    public async Task Cancel_WritesHuyRow_NotSua()
    {
        var voucher = await InsertAsync(NewVoucher());

        await UpdateAsync(voucher.Id, v => v.TrangThai = MauTrangThai.DaHuy);

        var row = (await LogOfAsync(voucher.Id)).Last();
        row.HanhDong.ShouldBe(HanhDong.Huy);
        Json(row.DuLieuCu)["TrangThai"].GetInt32().ShouldBe((int)MauTrangThai.DaGhiSo);
        Json(row.DuLieuMoi)["TrangThai"].GetInt32().ShouldBe((int)MauTrangThai.DaHuy);
    }

    [SqlServerFact]
    public async Task UpdateWithoutRealChange_WritesNoRow()
    {
        var voucher = await InsertAsync(NewVoucher());

        await UpdateAsync(voucher.Id, v => v.SoTien = v.SoTien);

        (await LogOfAsync(voucher.Id)).ShouldHaveSingleItem().HanhDong.ShouldBe(HanhDong.Them);
    }

    [SqlServerFact]
    public async Task ExcludedProperty_NeverAppearsInTheJson_ButItsChangeIsStillLogged()
    {
        var voucher = await InsertAsync(NewVoucher());

        await UpdateAsync(voucher.Id, v => v.MaBiMat = "bí-mật-mới");

        var log = await LogOfAsync(voucher.Id);
        log.Count.ShouldBe(2);
        log[1].HanhDong.ShouldBe(HanhDong.Sua);
        log.ShouldAllBe(n => !(n.DuLieuCu ?? "").Contains("bí-mật") && !(n.DuLieuMoi ?? "").Contains("bí-mật"));
        log.ShouldAllBe(n => !(n.DuLieuCu ?? "").Contains("MaBiMat") && !(n.DuLieuMoi ?? "").Contains("MaBiMat"));
    }

    [SqlServerFact]
    public async Task FailingSave_RollsBackBothTheEntityAndItsAuditRows()
    {
        var marker = Guid.NewGuid().ToString("N");
        var logCount = await LogCountAsync();

        await using (var db = _fixture.CreateAuditedContext())
        {
            db.MauChungTu.Add(NewVoucher(marker));
            // Violates CK_MauChungTu_TrangThai, so the batch fails after the first insert.
            var invalid = NewVoucher(marker);
            invalid.TrangThai = (MauTrangThai)9;
            db.MauChungTu.Add(invalid);

            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }

        await using var check = _fixture.CreatePlainContext();
        (await check.MauChungTu.CountAsync(v => v.NoiDung == marker)).ShouldBe(0);
        (await LogCountAsync()).ShouldBe(logCount);
    }

    [SqlServerFact]
    public async Task FailingAuditWrite_RollsBackTheEntityToo()
    {
        const string reject = "TU-CHOI-NHAT-KY";
        // Test-only constraint: an audit row mentioning the marker fails, after the voucher INSERT has succeeded.
        await _fixture.Database.ExecuteAsync($"""
            IF OBJECT_ID('CK_Test_TuChoiNhatKy') IS NULL
                ALTER TABLE NhatKyThaoTac ADD CONSTRAINT CK_Test_TuChoiNhatKy CHECK (DuLieuMoi NOT LIKE '%{reject}%')
            """);
        var marker = $"{reject}-{Guid.NewGuid():N}";
        var logCount = await LogCountAsync();

        await using (var db = _fixture.CreateAuditedContext())
        {
            db.MauChungTu.Add(NewVoucher(marker));
            await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync());

            // The rejected log row must not stay queued for the context's next save.
            db.ChangeTracker.Entries<NhatKyThaoTac>().ShouldBeEmpty();
        }

        await using var check = _fixture.CreatePlainContext();
        (await check.MauChungTu.CountAsync(v => v.NoiDung == marker)).ShouldBe(0);
        (await LogCountAsync()).ShouldBe(logCount);
    }

    [SqlServerFact]
    public async Task CallerTransaction_IsJoined_SoItsRollbackUndoesTheAuditRowsToo()
    {
        var marker = Guid.NewGuid().ToString("N");
        var logCount = await LogCountAsync();

        await using (var db = _fixture.CreateAuditedContext())
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            db.MauChungTu.Add(NewVoucher(marker));
            await db.SaveChangesAsync();

            db.Database.CurrentTransaction.ShouldBeSameAs(transaction);
            await transaction.RollbackAsync();
        }

        await using var check = _fixture.CreatePlainContext();
        (await check.MauChungTu.CountAsync(v => v.NoiDung == marker)).ShouldBe(0);
        (await LogCountAsync()).ShouldBe(logCount);
    }

    [SqlServerFact]
    public async Task CallerTransaction_Committed_KeepsEntityAndAuditRow()
    {
        await using var db = _fixture.CreateAuditedContext();
        await using var transaction = await db.Database.BeginTransactionAsync();
        var voucher = NewVoucher();
        db.MauChungTu.Add(voucher);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        (await LogOfAsync(voucher.Id)).ShouldHaveSingleItem().HanhDong.ShouldBe(HanhDong.Them);
    }

    [SqlServerFact]
    public void SynchronousSave_IsAuditedToo()
    {
        var voucher = NewVoucher();
        using (var db = _fixture.CreateAuditedContext())
        {
            db.MauChungTu.Add(voucher);
            db.SaveChanges();
        }

        LogOfAsync(voucher.Id).GetAwaiter().GetResult().ShouldHaveSingleItem().HanhDong.ShouldBe(HanhDong.Them);
    }

    [SqlServerFact]
    public async Task DeletingAnAuditedEntity_Throws()
    {
        var voucher = await InsertAsync(NewVoucher());

        await using var db = _fixture.CreateAuditedContext();
        db.MauChungTu.Remove(await db.MauChungTu.SingleAsync(v => v.Id == voucher.Id));

        (await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync())).Message.ShouldContain("cancel");
    }

    [SqlServerFact]
    public async Task ModifyingAnAuditRow_Throws()
    {
        var voucher = await InsertAsync(NewVoucher());

        await using var db = _fixture.CreateAuditedContext();
        var row = await db.NhatKyThaoTac.FirstAsync(n => n.TenBang == "MauChungTu" && n.BanGhiId == voucher.Id);
        db.Entry(row).Property(n => n.DuLieuMoi).CurrentValue = "{}";

        (await Should.ThrowAsync<InvalidOperationException>(() => db.SaveChangesAsync())).Message.ShouldContain("append-only");
    }

    [SqlServerFact]
    public async Task DeletingAnAuditRow_Throws()
    {
        var voucher = await InsertAsync(NewVoucher());

        await using var db = _fixture.CreateAuditedContext();
        db.NhatKyThaoTac.Remove(await db.NhatKyThaoTac.FirstAsync(n => n.TenBang == "MauChungTu" && n.BanGhiId == voucher.Id));

        Should.Throw<InvalidOperationException>(() => db.SaveChanges()).Message.ShouldContain("append-only");
    }
}
