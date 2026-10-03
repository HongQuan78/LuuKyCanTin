using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Domain.Administration;
using LuuKyCanTin.Domain.Common;
using LuuKyCanTin.Infrastructure.Administration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace LuuKyCanTin.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Fills the <see cref="AuditableEntity"/> columns and writes one <see cref="AuditLog"/> row per changed
/// <see cref="IAuditable"/> record, in the same transaction as the change. It keeps state between the before- and
/// after-save callbacks, so every DbContext needs its own instance (it is registered scoped).
/// </summary>
/// <remarks>
/// ExecuteUpdate, ExecuteDelete and raw SQL bypass interceptors, so voucher writes must go through SaveChanges; don't
/// turn them into ExecuteUpdate. The conditional balance UPDATE on CustodyBalance is raw SQL on purpose: a balance is not
/// a voucher, and the voucher inserted in the same transaction is what the log records.
/// </remarks>
internal sealed class AuditInterceptor(IClock clock, ICurrentUser currentUser, AuditLogFactory auditLogFactory) : SaveChangesInterceptor
{
    // Written to CreatedById when nobody is signed in (seeding, admin commands); the log row itself keeps a null user.
    private const int NoUser = 0;

    // Bookkeeping columns: the log row already records who and when, and a row version means nothing to a reader.
    private static readonly HashSet<string> BookkeepingColumns =
    [
        nameof(AuditableEntity.CreatedAt),
        nameof(AuditableEntity.CreatedById),
        nameof(AuditableEntity.ModifiedAt),
        nameof(AuditableEntity.ModifiedById),
        nameof(AuditableEntity.RowVer),
    ];

    private sealed record PendingChange(EntityEntry Entry, AuditAction Action, Dictionary<string, object?>? OldValues, Dictionary<string, object?>? NewValues);

    private List<PendingChange> _pendingChanges = [];
    private IDbContextTransaction? _ownTransaction;
    private bool _isWritingAuditLogs;

    private bool HasPendingChanges => _pendingChanges.Count > 0;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (_isWritingAuditLogs || eventData.Context is not { } context)
            return result;

        Prepare(context);
        // A caller's transaction is joined; otherwise the change and its log rows need one of their own.
        if (HasPendingChanges && context.Database.CurrentTransaction is null)
            _ownTransaction = context.Database.BeginTransaction();
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (_isWritingAuditLogs || eventData.Context is not { } context)
            return result;

        Prepare(context);
        if (HasPendingChanges && context.Database.CurrentTransaction is null)
            _ownTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return result;
    }

    // The log rows are built after the save because added records only get their IDENTITY ids from the INSERT.
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_isWritingAuditLogs || eventData.Context is not { } context)
            return result;

        List<AuditLog> auditLogs = [];
        try
        {
            auditLogs = AddAuditLogs(context);
            if (auditLogs.Count > 0)
            {
                _isWritingAuditLogs = true;
                context.SaveChanges();
            }
            _ownTransaction?.Commit();
        }
        catch
        {
            DetachAuditLogs(context, auditLogs);
            _ownTransaction?.Rollback();
            throw;
        }
        finally
        {
            Reset();
        }
        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
    {
        if (_isWritingAuditLogs || eventData.Context is not { } context)
            return result;

        List<AuditLog> auditLogs = [];
        try
        {
            auditLogs = AddAuditLogs(context);
            if (auditLogs.Count > 0)
            {
                _isWritingAuditLogs = true;
                await context.SaveChangesAsync(cancellationToken);
            }
            if (_ownTransaction is not null)
                await _ownTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            DetachAuditLogs(context, auditLogs);
            if (_ownTransaction is not null)
                await _ownTransaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            Reset();
        }
        return result;
    }

    // A failure while writing the log rows is handled in SavedChanges, which then reports the failure here again.
    public override void SaveChangesFailed(DbContextErrorEventData eventData) => RollBackOwnTransaction();

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        => await RollBackOwnTransactionAsync();

    public override void SaveChangesCanceled(DbContextEventData eventData) => RollBackOwnTransaction();

    public override async Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
        => await RollBackOwnTransactionAsync();

    /// <summary>Stamps the audit columns and records what each audited entry changes.</summary>
    private void Prepare(DbContext context)
    {
        // SavingChanges runs before EF's own DetectChanges, so the entry states are not final yet.
        if (context.ChangeTracker.AutoDetectChangesEnabled)
            context.ChangeTracker.DetectChanges();

        var now = clock.Now;
        var userId = currentUser.UserId;
        _pendingChanges = [];

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog)
            {
                if (entry.State is EntityState.Modified or EntityState.Deleted)
                    throw new InvalidOperationException("AuditLog is append-only: audit rows can never be changed or deleted.");
                continue;
            }

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                continue;

            if (entry.Entity is AuditableEntity)
                StampAuditColumns(entry, now, userId);

            if (entry.Entity is IAuditable && CaptureChange(entry) is { } change)
                _pendingChanges.Add(change);
        }
    }

    private static void StampAuditColumns(EntityEntry entry, DateTime now, int? userId)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;
            entry.Property(nameof(AuditableEntity.CreatedById)).CurrentValue = userId ?? NoUser;
        }
        else if (entry.State == EntityState.Modified)
        {
            entry.Property(nameof(AuditableEntity.ModifiedAt)).CurrentValue = now;
            entry.Property(nameof(AuditableEntity.ModifiedById)).CurrentValue = userId;
            KeepOriginalValue(entry.Property(nameof(AuditableEntity.CreatedAt)));
            KeepOriginalValue(entry.Property(nameof(AuditableEntity.CreatedById)));
        }
    }

    // Who created a record is never rewritten by a later edit.
    private static void KeepOriginalValue(PropertyEntry property)
    {
        property.CurrentValue = property.OriginalValue;
        property.IsModified = false;
    }

    private static PendingChange? CaptureChange(EntityEntry entry)
    {
        var isCancelledAfter = entry.Entity is ICancellable { IsCancelled: true };
        var isCancelledBefore = entry.State == EntityState.Modified && entry.OriginalValues.ToObject() is ICancellable { IsCancelled: true };
        var action = AuditActionResolver.Resolve(entry.State, isCancelledBefore, isCancelledAfter);

        if (entry.State == EntityState.Added)
            return new PendingChange(entry, action, OldValues: null, NewValues: null);

        var flaggedColumns = entry.Properties
            .Where(p => p.IsModified && !BookkeepingColumns.Contains(p.Metadata.Name))
            .ToList();
        var changedColumns = flaggedColumns
            .Where(p => !p.Metadata.GetValueComparer().Equals(p.OriginalValue, p.CurrentValue))
            .ToList();

        // OriginalValues are only right for an entity loaded by a query. Update/Attach of a detached one flags its
        // columns with "before" equal to "after", which would save the change with no log row at all.
        if (changedColumns.Count == 0 && flaggedColumns.Count > 0)
            throw new InvalidOperationException(
                $"{entry.Metadata.ClrType.Name} was saved without its original values (Update/Attach of a detached entity), "
                + "so the audit log cannot tell what changed. Load it, change it, then save.");
        if (changedColumns.Count == 0)
            return null;

        // A change to a secret is still logged, just without its values.
        var loggableColumns = changedColumns.Where(IsValueLoggable).ToList();
        return new PendingChange(
            entry,
            action,
            loggableColumns.ToDictionary(p => p.Metadata.GetColumnName(), p => p.OriginalValue),
            loggableColumns.ToDictionary(p => p.Metadata.GetColumnName(), p => p.CurrentValue));
    }

    private List<AuditLog> AddAuditLogs(DbContext context)
    {
        var auditLogs = _pendingChanges
            .Select(t => auditLogFactory.Create(
                t.Action,
                t.Entry.Metadata.GetTableName(),
                GetKey(t.Entry),
                t.OldValues,
                t.NewValues ?? t.Entry.Properties
                    .Where(p => !BookkeepingColumns.Contains(p.Metadata.Name) && IsValueLoggable(p))
                    .ToDictionary(p => p.Metadata.GetColumnName(), p => p.CurrentValue)))
            .ToList();

        context.Set<AuditLog>().AddRange(auditLogs);
        return auditLogs;
    }

    private static bool IsValueLoggable(PropertyEntry property) =>
        property.Metadata.PropertyInfo?.IsDefined(typeof(NotAuditedAttribute), inherit: true) != true;

    private static long? GetKey(EntityEntry entry)
    {
        if (entry.Metadata.FindPrimaryKey() is not { Properties: [var key] })
            return null;

        return entry.Property(key.Name).CurrentValue switch
        {
            int i => i,
            long l => l,
            short s => s,
            byte b => b,
            _ => null,
        };
    }

    // Rolled-back log rows must not linger as Added and be inserted by the context's next save.
    private static void DetachAuditLogs(DbContext context, List<AuditLog> auditLogs)
    {
        foreach (var auditLog in auditLogs)
            context.Entry(auditLog).State = EntityState.Detached;
    }

    private void RollBackOwnTransaction()
    {
        if (_isWritingAuditLogs)
            return;
        _ownTransaction?.Rollback();
        Reset();
    }

    // No ct: the rollback must run even when the save itself was cancelled.
    private async Task RollBackOwnTransactionAsync()
    {
        if (_isWritingAuditLogs)
            return;
        if (_ownTransaction is not null)
            await _ownTransaction.RollbackAsync(CancellationToken.None);
        Reset();
    }

    private void Reset()
    {
        _ownTransaction?.Dispose();
        _ownTransaction = null;
        _pendingChanges = [];
        _isWritingAuditLogs = false;
    }
}
