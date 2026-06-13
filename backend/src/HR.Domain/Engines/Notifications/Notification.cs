using HR.Domain.Common;

namespace HR.Domain.Engines.Notifications;

/// <summary>An in-app notification for a user (request submitted, approval needed, decision).</summary>
public class Notification : TenantEntity
{
    public Guid UserId { get; set; }
    public string TitleAr { get; set; } = null!;
    public string TitleEn { get; set; } = null!;
    public string? BodyAr { get; set; }
    public string? BodyEn { get; set; }
    public string? Category { get; set; }
    public string? Link { get; set; }
    public Guid? EntityId { get; set; }
    public bool IsRead { get; set; }
}
