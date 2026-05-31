using HR.Domain.Common;

namespace HR.Domain.Engines.Audit;

public class AuditEntry : TenantEntity
{
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = null!;
    public string Module { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string? OldValues { get; set; } // JSONB
    public string? NewValues { get; set; } // JSONB
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}
