using HR.Domain.Common;

namespace HR.Domain.Engines.Timeline;

public class TimelineEvent : TenantEntity
{
    public string Category { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string? Metadata { get; set; } // JSONB
    public DateTime OccurredAt { get; set; }
}
