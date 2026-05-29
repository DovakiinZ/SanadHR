using HR.Domain.Common;

namespace HR.Modules.Notifications.Entities;

// TODO: Implement notification entity
public class Notification : TenantEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = null!;
    public string? Body { get; set; }
    public bool IsRead { get; set; }
    public string? ActionUrl { get; set; }
}
