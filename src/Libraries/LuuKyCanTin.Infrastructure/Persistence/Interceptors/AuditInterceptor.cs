using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Domain.HeThong;
using LuuKyCanTin.Infrastructure.HeThong;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace LuuKyCanTin.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Fills the <see cref="AuditableEntity"/> columns and writes one <see cref="NhatKyThaoTac"/> row per changed
/// <see cref="IAuditable"/> record, in the same transaction as the change. It keeps state between the before- and
/// after-save callbacks, so every DbContext needs its own instance (it is registered scoped).
/// </summary>
/// <remarks>
/// ExecuteUpdate, ExecuteDelete and raw SQL bypass interceptors, so voucher writes must go through SaveChanges; don't
/// turn them into ExecuteUpdate. The conditional balance UPDATE on SoDuLuuKy is raw SQL on purpose: a balance is not
/// a voucher, and the voucher inserted in the same transaction is what the log records.
/// </remarks>
internal sealed class AuditInterceptor(IClock clock, ICurrentUser currentUser, NhatKyFactory nhatKyFactory) : SaveChangesInterceptor
{
    // Written to NguoiTaoId when nobody is signed in (seeding, admin commands); the log row itself keeps a null user.
    private const int KhongCoNguoiDung = 0;

    // Bookkeeping columns: the log row already records who and when, and a row version means nothing to a reader.
    private static readonly HashSet<string> DanhSachCotKhongGhi =
    [
        nameof(AuditableEntity.NgayTao),
        nameof(AuditableEntity.NguoiTaoId),
        nameof(AuditableEntity.NgaySua),
        nameof(AuditableEntity.NguoiSuaId),
        nameof(AuditableEntity.RowVer),
    ];

    private sealed record ThayDoi(EntityEntry Entry, HanhDong HanhDong, Dictionary<string, object?>? Cu, Dictionary<string, object?>? Moi);

    private List<ThayDoi> _danhSachThayDoi = [];
    private IDbContextTransaction? _transactionRieng;
    private bool _dangGhiNhatKy;

    private bool CoNhatKyCanGhi => _danhSachThayDoi.Count > 0;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (_dangGhiNhatKy || eventData.Context is not { } context)
            return result;

        ChuanBi(context);
        // A caller's transaction is joined; otherwise the change and its log rows need one of their own.
        if (CoNhatKyCanGhi && context.Database.CurrentTransaction is null)
            _transactionRieng = context.Database.BeginTransaction();
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (_dangGhiNhatKy || eventData.Context is not { } context)
            return result;

        ChuanBi(context);
        if (CoNhatKyCanGhi && context.Database.CurrentTransaction is null)
            _transactionRieng = await context.Database.BeginTransactionAsync(cancellationToken);
        return result;
    }

    // The log rows are built after the save because added records only get their IDENTITY ids from the INSERT.
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_dangGhiNhatKy || eventData.Context is not { } context)
            return result;

        List<NhatKyThaoTac> danhSachNhatKy = [];
        try
        {
            danhSachNhatKy = ThemNhatKy(context);
            if (danhSachNhatKy.Count > 0)
            {
                _dangGhiNhatKy = true;
                context.SaveChanges();
            }
            _transactionRieng?.Commit();
        }
        catch
        {
            BoNhatKy(context, danhSachNhatKy);
            _transactionRieng?.Rollback();
            throw;
        }
        finally
        {
            KetThuc();
        }
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_dangGhiNhatKy || eventData.Context is not { } context)
            return result;

        List<NhatKyThaoTac> danhSachNhatKy = [];
        try
        {
            danhSachNhatKy = ThemNhatKy(context);
            if (danhSachNhatKy.Count > 0)
            {
                _dangGhiNhatKy = true;
                await context.SaveChangesAsync(cancellationToken);
            }
            if (_transactionRieng is not null)
                await _transactionRieng.CommitAsync(cancellationToken);
        }
        catch
        {
            BoNhatKy(context, danhSachNhatKy);
            if (_transactionRieng is not null)
                await _transactionRieng.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            KetThuc();
        }
        return result;
    }

    // A failure while writing the log rows is handled in SavedChanges, which then reports the failure here again.
    public override void SaveChangesFailed(DbContextErrorEventData eventData) => HoanTacTransaction();

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        => await HoanTacTransactionAsync();

    public override void SaveChangesCanceled(DbContextEventData eventData) => HoanTacTransaction();

    public override async Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
        => await HoanTacTransactionAsync();

    /// <summary>Stamps the audit columns and records what each audited entry changes.</summary>
    private void ChuanBi(DbContext context)
    {
        // SavingChanges runs before EF's own DetectChanges, so the entry states are not final yet.
        if (context.ChangeTracker.AutoDetectChangesEnabled)
            context.ChangeTracker.DetectChanges();

        var thoiDiem = clock.Now;
        var nguoiDungId = currentUser.NguoiDungId;
        _danhSachThayDoi = [];

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is NhatKyThaoTac)
            {
                if (entry.State is EntityState.Modified or EntityState.Deleted)
                    throw new InvalidOperationException("NhatKyThaoTac is append-only: audit rows can never be changed or deleted.");
                continue;
            }

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (entry.Entity is AuditableEntity)
                DienCotKiemToan(entry, thoiDiem, nguoiDungId);

            if (entry.Entity is IAuditable && GhiNhan(entry) is { } thayDoi)
                _danhSachThayDoi.Add(thayDoi);
        }
    }

    private static void DienCotKiemToan(EntityEntry entry, DateTime thoiDiem, int? nguoiDungId)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Property(nameof(AuditableEntity.NgayTao)).CurrentValue = thoiDiem;
            entry.Property(nameof(AuditableEntity.NguoiTaoId)).CurrentValue = nguoiDungId ?? KhongCoNguoiDung;
        }
        else if (entry.State == EntityState.Modified)
        {
            entry.Property(nameof(AuditableEntity.NgaySua)).CurrentValue = thoiDiem;
            entry.Property(nameof(AuditableEntity.NguoiSuaId)).CurrentValue = nguoiDungId;
            GiuNguyen(entry.Property(nameof(AuditableEntity.NgayTao)));
            GiuNguyen(entry.Property(nameof(AuditableEntity.NguoiTaoId)));
        }
    }

    // Who created a record is never rewritten by a later edit.
    private static void GiuNguyen(PropertyEntry property)
    {
        property.CurrentValue = property.OriginalValue;
        property.IsModified = false;
    }

    private static ThayDoi? GhiNhan(EntityEntry entry)
    {
        var daHuySau = entry.Entity is ICoTrangThaiHuy { DaHuy: true };
        var daHuyTruoc = entry.State == EntityState.Modified && entry.OriginalValues.ToObject() is ICoTrangThaiHuy { DaHuy: true };
        var hanhDong = HanhDongNhatKy.XacDinh(entry.State, daHuyTruoc, daHuySau);

        if (entry.State == EntityState.Added)
            return new ThayDoi(entry, hanhDong, Cu: null, Moi: null);

        var danhSachCotDanhDau = entry.Properties
            .Where(p => p.IsModified && !DanhSachCotKhongGhi.Contains(p.Metadata.Name))
            .ToList();
        var danhSachCotThayDoi = danhSachCotDanhDau
            .Where(p => !p.Metadata.GetValueComparer().Equals(p.OriginalValue, p.CurrentValue))
            .ToList();

        // OriginalValues are only right for an entity loaded by a query. Update/Attach of a detached one flags its
        // columns with "before" equal to "after", which would save the change with no log row at all.
        if (danhSachCotThayDoi.Count == 0 && danhSachCotDanhDau.Count > 0)
            throw new InvalidOperationException(
                $"{entry.Metadata.ClrType.Name} was saved without its original values (Update/Attach of a detached entity), "
                + "so the audit log cannot tell what changed. Load it, change it, then save.");
        if (danhSachCotThayDoi.Count == 0)
            return null;

        // A change to a secret is still logged, just without its values.
        var danhSachCotDuocGhi = danhSachCotThayDoi.Where(DuocPhepGhiGiaTri).ToList();
        return new ThayDoi(
            entry,
            hanhDong,
            danhSachCotDuocGhi.ToDictionary(p => p.Metadata.GetColumnName(), p => p.OriginalValue),
            danhSachCotDuocGhi.ToDictionary(p => p.Metadata.GetColumnName(), p => p.CurrentValue));
    }

    private List<NhatKyThaoTac> ThemNhatKy(DbContext context)
    {
        var danhSachNhatKy = _danhSachThayDoi
            .Select(t => nhatKyFactory.Tao(
                t.HanhDong,
                t.Entry.Metadata.GetTableName(),
                LayKhoa(t.Entry),
                t.Cu,
                t.Moi ?? t.Entry.Properties
                    .Where(p => !DanhSachCotKhongGhi.Contains(p.Metadata.Name) && DuocPhepGhiGiaTri(p))
                    .ToDictionary(p => p.Metadata.GetColumnName(), p => p.CurrentValue)))
            .ToList();

        context.Set<NhatKyThaoTac>().AddRange(danhSachNhatKy);
        return danhSachNhatKy;
    }

    private static bool DuocPhepGhiGiaTri(PropertyEntry property) =>
        property.Metadata.PropertyInfo?.IsDefined(typeof(KhongGhiNhatKyAttribute), inherit: true) != true;

    private static long? LayKhoa(EntityEntry entry)
    {
        if (entry.Metadata.FindPrimaryKey() is not { Properties: [var khoa] })
            return null;

        return entry.Property(khoa.Name).CurrentValue switch
        {
            int i => i,
            long l => l,
            short s => s,
            byte b => b,
            _ => null,
        };
    }

    // Rolled-back log rows must not linger as Added and be inserted by the context's next save.
    private static void BoNhatKy(DbContext context, List<NhatKyThaoTac> danhSachNhatKy)
    {
        foreach (var nhatKy in danhSachNhatKy)
            context.Entry(nhatKy).State = EntityState.Detached;
    }

    private void HoanTacTransaction()
    {
        if (_dangGhiNhatKy)
            return;
        _transactionRieng?.Rollback();
        KetThuc();
    }

    // No ct: the rollback must run even when the save itself was cancelled.
    private async Task HoanTacTransactionAsync()
    {
        if (_dangGhiNhatKy)
            return;
        if (_transactionRieng is not null)
            await _transactionRieng.RollbackAsync(CancellationToken.None);
        KetThuc();
    }

    private void KetThuc()
    {
        _transactionRieng?.Dispose();
        _transactionRieng = null;
        _danhSachThayDoi = [];
        _dangGhiNhatKy = false;
    }
}
