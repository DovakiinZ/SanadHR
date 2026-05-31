using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Automation;

public class AutomationAction : BaseEntity
{
    public Guid AutomationRuleId { get; set; }
    public AutomationActionType ActionType { get; set; }
    public string? Configuration { get; set; } // JSONB
    public int SortOrder { get; set; }

    public AutomationRule AutomationRule { get; set; } = null!;
}
