using FluentValidation;
using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.MasterData;
using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.Administration;

public sealed class AccountService(
    IAppDbContext db,
    IUserStore userStore,
    IPermissionChecker permissionChecker,
    IAuditLogWriter auditLog,
    IPasswordHasher passwordHasher,
    IClock clock,
    ICurrentUser currentUser,
    IValidator<CreateAccountRequest> validator,
    LastAdministratorGuard lastAdministratorGuard) : IAccountService
{
    public const string DuplicateUserNameMessage = "Tên đăng nhập đã tồn tại.";
    public const string OfficerAlreadyHasActiveAccountMessage = "Cán bộ này đã có tài khoản đang hoạt động.";
    public const string AccountConflictMessage =
        "Không tạo được tài khoản: tên đăng nhập đã tồn tại hoặc cán bộ đã có tài khoản đang hoạt động.";
    public const string InvalidOfficerMessage = "Cán bộ không tồn tại hoặc đã ngừng công tác.";
    public const string InvalidRoleMessage = "Vai trò không hợp lệ.";
    public const string AccountNotFoundMessage = "Không tìm thấy tài khoản.";
    public const string CannotDeactivateSelfMessage = "Không thể ngừng hoạt động tài khoản đang đăng nhập.";

    public async Task<IReadOnlyList<AccountDto>> GetAllAsync(CancellationToken ct = default)
    {
        var now = clock.Now;
        var rows = await userStore.GetAllWithRolesAsync(ct);

        return rows.Select(row => new AccountDto(
            row.User.Id,
            row.User.UserName,
            row.OfficerFullName,
            string.Join(", ", row.Roles.Select(role => role.Name)),
            row.Roles.Select(role => role.Id).ToList(),
            row.User.IsActive,
            row.User.IsLocked(now),
            row.User.MustChangePassword)).ToList();
    }

    public async Task<IReadOnlyList<OfficerDto>> GetOfficersForAccountCreationAsync(CancellationToken ct = default)
    {
        // One active account per person: an officer who already has one is not offered again.
        var accountRows = await userStore.GetAllWithRolesAsync(ct);
        var officerIdsWithAnActiveAccount = accountRows
            .Where(row => row.User.OfficerId is not null && row.User.IsActive)
            .Select(row => row.User.OfficerId!.Value)
            .ToHashSet();

        var officers = await db.Officer.AsNoTracking()
            .Where(officer => officer.IsActive)
            .OrderBy(officer => officer.FullName)
            .ThenBy(officer => officer.OfficerCode)
            .Select(officer => new OfficerDto(
                officer.Id, officer.OfficerCode, officer.FullName, officer.Position,
                officer.IsSupervisingOfficer, officer.IsActive, officer.RowVer))
            .ToListAsync(ct);

        return officers.Where(officer => !officerIdsWithAnActiveAccount.Contains(officer.Id)).ToList();
    }

    public async Task<CreateAccountResult> CreateAsync(CreateAccountRequest request, CancellationToken ct = default)
    {
        // Authorization first, before any read or transaction, so a refused call writes nothing at all.
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);
        await EnsureValidAsync(request, ct);

        var userName = request.UserName.Trim();
        if (await userStore.FindByUserNameAsync(userName, ct) is not null)
            throw new BusinessRuleException(DuplicateUserNameMessage);
        if (!await db.Officer.AnyAsync(officer => officer.Id == request.OfficerId && officer.IsActive, ct))
            throw new BusinessRuleException(InvalidOfficerMessage);
        if (await userStore.HasActiveAccountAsync(request.OfficerId, ct: ct))
            throw new BusinessRuleException(OfficerAlreadyHasActiveAccountMessage);

        var roleIds = request.RoleIds.Distinct().ToList();
        var existingRoleIds = await db.Role
            .Where(role => roleIds.Contains(role.Id))
            .Select(role => role.Id)
            .ToListAsync(ct);
        if (existingRoleIds.Count != roleIds.Count)
            throw new BusinessRuleException(InvalidRoleMessage);

        var temporaryPassword = TemporaryPassword.Generate();
        var user = new User
        {
            UserName = userName,
            OfficerId = request.OfficerId,
            PasswordHash = passwordHasher.Hash(temporaryPassword),
            IsActive = true,
            MustChangePassword = true,
        };

        await using var transaction = await db.BeginTransactionAsync(ct);
        try
        {
            await userStore.AddAsync(user, ct);
        }
        catch (UniqueConstraintException ex)
        {
            // The filtered unique index or the sign-in name index rejected a concurrent create.
            throw new BusinessRuleException(AccountConflictMessage, ex);
        }

        foreach (var roleId in roleIds)
            db.UserRole.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new CreateAccountResult(user.Id, temporaryPassword);
    }

    public async Task UpdateRolesAsync(int userId, IReadOnlyCollection<int> roleIds, CancellationToken ct = default)
    {
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);

        var newRoleIds = roleIds.Distinct().ToList();
        if (newRoleIds.Count == 0)
            throw new BusinessRuleException(InvalidRoleMessage);

        // Serializable: the guard and the write must be one atomic unit, or two administrators could demote each
        // other at the same moment and both succeed.
        await using var transaction = await db.BeginTransactionAsync(TransactionIsolation.Serializable, ct);

        _ = await userStore.FindByIdAsync(userId, ct)
            ?? throw new BusinessRuleException(AccountNotFoundMessage);

        var existingRoleIds = await db.Role
            .Where(role => newRoleIds.Contains(role.Id))
            .Select(role => role.Id)
            .ToListAsync(ct);
        if (existingRoleIds.Count != newRoleIds.Count)
            throw new BusinessRuleException(InvalidRoleMessage);

        await lastAdministratorGuard.EnsureRoleChangeAllowedAsync(userId, newRoleIds, ct);

        var current = await db.UserRole.Where(userRole => userRole.UserId == userId).ToListAsync(ct);
        var codeById = await db.Role.AsNoTracking().ToDictionaryAsync(role => role.Id, role => role.Code, ct);
        var oldCodes = current.Select(userRole => codeById[userRole.RoleId]).Order(StringComparer.Ordinal).ToList();
        var newSet = newRoleIds.ToHashSet();

        db.UserRole.RemoveRange(current.Where(userRole => !newSet.Contains(userRole.RoleId)));
        foreach (var roleId in newRoleIds.Where(id => current.All(userRole => userRole.RoleId != id)))
            db.UserRole.Add(new UserRole { UserId = userId, RoleId = roleId });

        var newCodes = newRoleIds.Select(id => codeById[id]).Order(StringComparer.Ordinal).ToList();
        await db.SaveChangesAsync(ct);

        // The join rows are not IAuditable, so a real role change is logged explicitly with its before/after values.
        // Saving the same set is a no-op and must not leave a row that claims a change.
        if (!oldCodes.SequenceEqual(newCodes, StringComparer.Ordinal))
            await auditLog.WriteAsync(
                AuditAction.Update, "User", userId,
                new { Roles = oldCodes },
                new { Roles = newCodes }, ct);
        await transaction.CommitAsync(ct);
    }

    public async Task DeactivateAsync(int userId, CancellationToken ct = default)
    {
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);
        if (currentUser.UserId == userId)
            throw new BusinessRuleException(CannotDeactivateSelfMessage);

        await using var transaction = await db.BeginTransactionAsync(TransactionIsolation.Serializable, ct);
        var user = await userStore.FindByIdAsync(userId, ct)
            ?? throw new BusinessRuleException(AccountNotFoundMessage);

        await lastAdministratorGuard.EnsureDeactivationAllowedAsync(userId, ct);

        user.IsActive = false;
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }

    public async Task ReactivateAsync(int userId, CancellationToken ct = default)
    {
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);

        await using var transaction = await db.BeginTransactionAsync(ct);
        var user = await userStore.FindByIdAsync(userId, ct)
            ?? throw new BusinessRuleException(AccountNotFoundMessage);

        if (user.OfficerId is { } officerId)
        {
            // The officer may have left while the account was off.
            if (!await db.Officer.AnyAsync(officer => officer.Id == officerId && officer.IsActive, ct))
                throw new BusinessRuleException(InvalidOfficerMessage);

            // The person may have received a newer active account while this one was off.
            if (await userStore.HasActiveAccountAsync(officerId, excludedUserId: userId, ct: ct))
                throw new BusinessRuleException(OfficerAlreadyHasActiveAccountMessage);
        }

        user.IsActive = true;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintException ex)
        {
            throw new BusinessRuleException(OfficerAlreadyHasActiveAccountMessage, ex);
        }

        await transaction.CommitAsync(ct);
    }

    public async Task UnlockAsync(int userId, CancellationToken ct = default)
    {
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);

        var user = await userStore.FindByIdAsync(userId, ct)
            ?? throw new BusinessRuleException(AccountNotFoundMessage);

        user.RecordSuccessfulSignIn();
        await db.SaveChangesAsync(ct);

        // An already-unlocked account changes no column, so the interceptor writes nothing; this row makes every
        // unlock action visible in the audit log.
        await auditLog.WriteAsync(
            AuditAction.Update, "User", userId,
            new { Event = AccountEvent.Unlock }, ct);
    }

    public async Task<string> ResetPasswordAsync(int userId, CancellationToken ct = default)
    {
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);

        // The transaction starts before the first read, so the read, the write and the audit row are one unit.
        await using var transaction = await db.BeginTransactionAsync(ct);
        var user = await userStore.FindByIdAsync(userId, ct)
            ?? throw new BusinessRuleException(AccountNotFoundMessage);

        var temporaryPassword = TemporaryPassword.Generate();
        user.PasswordHash = passwordHasher.Hash(temporaryPassword);
        user.MustChangePassword = true;
        user.RecordSuccessfulSignIn();
        await db.SaveChangesAsync(ct);

        // The interceptor logs the flag's false → true change; this row makes the action itself unmistakable.
        await auditLog.WriteAsync(
            AuditAction.Update, "User", userId,
            new { Event = AccountEvent.ResetPassword }, ct);
        await transaction.CommitAsync(ct);

        return temporaryPassword;
    }

    private async Task EnsureValidAsync(CreateAccountRequest request, CancellationToken ct)
    {
        var result = await validator.ValidateAsync(request, ct);
        if (!result.IsValid)
            throw new RequestValidationException(result.Errors);
    }
}
