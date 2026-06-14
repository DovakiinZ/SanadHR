using HR.Domain.Common;

namespace HR.Domain.Engines.Notifications;

/// <summary>
/// An admin-configured notification rule. The first supported event is <c>DocumentExpiry</c>:
/// when an employee document is within <see cref="DaysBefore"/> days of expiring, the configured
/// recipients receive a notification on the enabled channels. Email/SMS are queued for the future;
/// the bell channel fires immediately.
/// </summary>
public class NotificationRule : TenantEntity
{
    public string Name { get; set; } = null!;

    /// <summary>Trigger event. Currently: <c>DocumentExpiry</c>.</summary>
    public string Event { get; set; } = "DocumentExpiry";

    /// <summary>How many days before the expiry date to notify (e.g. 10 / 20 / 30 / custom).</summary>
    public int DaysBefore { get; set; } = 30;

    /// <summary>Optional document-type filter (Iqama, Passport, …). Null = all types.</summary>
    public string? DocumentType { get; set; }

    // Recipients
    public bool NotifyEmployee { get; set; }
    public bool NotifyDirectManager { get; set; }
    public bool NotifyDepartmentManager { get; set; }
    /// <summary>A specific employee to also notify (e.g. HR officer).</summary>
    public Guid? ExtraEmployeeId { get; set; }
    /// <summary>A role/permission group — every user in the role is notified.</summary>
    public Guid? RoleId { get; set; }

    // Channels
    public bool ChannelBell { get; set; } = true;
    public bool ChannelEmail { get; set; }
    public bool ChannelSms { get; set; }

    public bool IsActive { get; set; } = true;
}
