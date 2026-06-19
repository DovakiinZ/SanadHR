using HR.Domain.Common;

namespace HR.Domain.Engines.Notifications;

/// <summary>
/// Dedup ledger for rule-driven notifications: records that a given rule already notified a given
/// recipient about a given source entity (e.g. an employee document), so re-runs of the expiry scan
/// don't spam the same person repeatedly.
/// </summary>
public class NotificationDispatch : TenantEntity
{
    public Guid RuleId { get; set; }
    /// <summary>The entity that triggered the notification (e.g. the EmployeeDocument id).</summary>
    public Guid SourceEntityId { get; set; }
    public Guid UserId { get; set; }
    public DateTime DispatchedAt { get; set; } = DateTime.UtcNow;
}
