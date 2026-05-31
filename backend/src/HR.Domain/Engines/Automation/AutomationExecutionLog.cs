using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Automation;

public class AutomationExecutionLog : BaseEntity
{
    public Guid AutomationRuleId { get; set; }
    public string TriggerEventType { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public AutomationExecutionStatus Status { get; set; }
    public DateTime ExecutedAt { get; set; }
    public string? Error { get; set; }

    public AutomationRule AutomationRule { get; set; } = null!;
}
