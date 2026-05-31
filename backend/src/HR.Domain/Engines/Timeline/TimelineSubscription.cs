using HR.Domain.Common;

namespace HR.Domain.Engines.Timeline;

public class TimelineSubscription : TenantEntity
{
    public Guid UserId { get; set; }
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
}
