namespace HR.Modules.Platform.DTOs.Timeline;

public class TimelineEventDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = null!;
    public string? DescriptionEn { get; set; }
    public string? DescriptionAr { get; set; }
    public Guid? ActorId { get; set; }
    public string? ActorName { get; set; }
    public string? Metadata { get; set; }
    public DateTime OccurredAt { get; set; }
}
