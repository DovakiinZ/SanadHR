using HR.Domain.Common;

namespace HR.Domain.Engines.Automation;

public class AutomationTrigger : BaseEntity
{
    public Guid AutomationRuleId { get; set; }
    public string EventType { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string? Configuration { get; set; } // JSONB

    public AutomationRule AutomationRule { get; set; } = null!;
}
