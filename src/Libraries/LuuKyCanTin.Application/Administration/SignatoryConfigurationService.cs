using LuuKyCanTin.Application.Abstractions;
using LuuKyCanTin.Application.Common;
using LuuKyCanTin.Application.Reporting;
using LuuKyCanTin.Domain.Administration;
using Microsoft.EntityFrameworkCore;

namespace LuuKyCanTin.Application.Administration;

/// <summary>
/// The one engine that writes the signer lines of the print templates. A save replaces the template's whole row set
/// inside one transaction and writes one explicit audit row, because the rows are deleted and cannot be
/// <c>IAuditable</c>.
/// </summary>
public sealed class SignatoryConfigurationService(
    IAppDbContext db,
    IPermissionChecker permissionChecker,
    IAuditLogWriter auditLog) : ISignatoryConfigurationService
{
    public const int TitleMaxLength = 100;
    public const int MaxSignatories = byte.MaxValue;
    public const string UnknownTemplateMessage = "Mẫu in không hợp lệ.";
    public const string AtLeastOneSignatoryMessage = "Mẫu in phải có ít nhất một người ký";
    public const string TooManySignatoriesMessage = "Mẫu in tối đa 255 dòng ký.";
    public const string TitleRequiredMessage = "Chức danh không được để trống.";
    public const string TitleMaxLengthMessage = "Chức danh tối đa 100 ký tự.";
    public const string OfficerNotFoundMessage = "Không tìm thấy cán bộ.";
    public const string InactiveOfficerMessage = "Cán bộ đã nghỉ không thể được chọn làm người ký.";

    private const string TableName = "SignatoryConfiguration";

    public async Task<IReadOnlyList<SignatoryRowDto>> GetByTemplateAsync(string templateCode, CancellationToken ct = default)
    {
        // A code outside the catalogue is a programming error, not user input: the caller passed the wrong constant.
        if (TemplateCodes.All.All(t => t.Code != templateCode))
            throw new InvalidOperationException($"Unknown template code '{templateCode}'.");

        var rows = await (
            from signatory in db.SignatoryConfiguration.AsNoTracking()
            join officer in db.Officer.AsNoTracking() on signatory.OfficerId equals officer.Id into officerGroup
            from officer in officerGroup.DefaultIfEmpty()
            where signatory.TemplateCode == templateCode
            orderby signatory.Ordinal
            select new
            {
                signatory.Ordinal,
                signatory.Title,
                signatory.OfficerId,
                // The left join yields null for a blank line; materialize through nullable types.
                FullName = (string?)officer.FullName,
                IsActive = (bool?)officer.IsActive,
            }).ToListAsync(ct);

        return rows
            .Select(r => new SignatoryRowDto(r.Ordinal, r.Title, r.OfficerId, r.FullName, r.IsActive ?? false))
            .ToList();
    }

    public async Task SaveAsync(string templateCode, IReadOnlyList<SignatoryLine> signatories, CancellationToken ct = default)
    {
        // Authorization first, before any read or transaction, so a refused call writes nothing at all.
        await permissionChecker.RequireAsync(PermissionCodes.Administration.Update, ct);
        ArgumentNullException.ThrowIfNull(signatories);

        if (TemplateCodes.All.All(t => t.Code != templateCode))
            throw new BusinessRuleException(UnknownTemplateMessage);
        if (signatories.Count == 0)
            throw new BusinessRuleException(AtLeastOneSignatoryMessage);
        // Ordinal is tinyint: cap the list so the cast below can never wrap.
        if (signatories.Count > MaxSignatories)
            throw new BusinessRuleException(TooManySignatoriesMessage);

        var lines = signatories
            .Select(line => new SignatoryLine((line.Title ?? "").Trim(), line.OfficerId))
            .ToList();
        foreach (var line in lines)
        {
            if (line.Title.Length == 0)
                throw new BusinessRuleException(TitleRequiredMessage);
            if (line.Title.Length > TitleMaxLength)
                throw new BusinessRuleException(TitleMaxLengthMessage);
        }

        await using var transaction = await db.BeginTransactionAsync(ct);

        var existing = await (
            from signatory in db.SignatoryConfiguration.AsNoTracking()
            join officer in db.Officer.AsNoTracking() on signatory.OfficerId equals officer.Id into officerGroup
            from officer in officerGroup.DefaultIfEmpty()
            where signatory.TemplateCode == templateCode
            orderby signatory.Ordinal
            select new
            {
                signatory.Ordinal,
                signatory.Title,
                signatory.OfficerId,
                FullName = (string?)officer.FullName,
            }).ToListAsync(ct);
        var existingOfficerIds = existing
            .Where(r => r.OfficerId is not null)
            .Select(r => r.OfficerId!.Value)
            .ToHashSet();

        var newOfficerIds = lines
            .Where(line => line.OfficerId is not null)
            .Select(line => line.OfficerId!.Value)
            .Distinct()
            .ToList();
        var officers = await db.Officer.AsNoTracking()
            .Where(officer => newOfficerIds.Contains(officer.Id))
            .ToDictionaryAsync(officer => officer.Id, ct);
        foreach (var officerId in newOfficerIds)
        {
            if (!officers.TryGetValue(officerId, out var officer))
                throw new BusinessRuleException(OfficerNotFoundMessage);
            // A person who has left may stay as an existing default, but cannot be newly chosen.
            if (!officer.IsActive && !existingOfficerIds.Contains(officerId))
                throw new BusinessRuleException(InactiveOfficerMessage);
        }

        // Replace the set in two saves inside one transaction: updating Ordinals in place would hit the
        // (TemplateCode, Ordinal) unique index midway, while delete-then-insert never contends with itself.
        var current = await db.SignatoryConfiguration.Where(s => s.TemplateCode == templateCode).ToListAsync(ct);
        db.SignatoryConfiguration.RemoveRange(current);
        await db.SaveChangesAsync(ct);

        for (var i = 0; i < lines.Count; i++)
        {
            db.SignatoryConfiguration.Add(new SignatoryConfiguration
            {
                TemplateCode = templateCode,
                Ordinal = (byte)(i + 1),
                Title = lines[i].Title,
                OfficerId = lines[i].OfficerId,
            });
        }

        await db.SaveChangesAsync(ct);

        await auditLog.WriteAsync(
            AuditAction.Update, TableName, recordId: null,
            new
            {
                TemplateCode = templateCode,
                Signatories = existing.Select(r => new { r.Ordinal, r.Title, r.OfficerId, r.FullName }).ToList(),
            },
            new
            {
                TemplateCode = templateCode,
                Signatories = lines.Select((line, index) => new
                {
                    Ordinal = (byte)(index + 1),
                    line.Title,
                    line.OfficerId,
                    FullName = line.OfficerId is { } officerId ? officers[officerId].FullName : null,
                }).ToList(),
            },
            ct);

        await transaction.CommitAsync(ct);
    }
}
