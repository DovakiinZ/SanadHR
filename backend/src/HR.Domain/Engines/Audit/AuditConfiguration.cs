using HR.Domain.Common;

namespace HR.Domain.Engines.Audit;

public class AuditConfiguration : BaseEntity
{
    public string EntityType { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
    public string? TrackedFields { get; set; } // JSONB
}
