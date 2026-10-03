namespace LuuKyCanTin.Domain.Common;

/// <summary>
/// Keeps a property's value out of the audit-log JSON, for secrets such as password hashes. A change to it is
/// still logged; only the value is left out.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class NotAuditedAttribute : Attribute;
