using HR.Domain.Common;

namespace HR.Domain.Engines.Automation;

public class AutomationCondition : BaseEntity
{
    public Guid AutomationRuleId { get; set; }
    public string Field { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? LogicalOperator { get; set; } // AND / OR

    public AutomationRule AutomationRule { get; set; } = null!;
}
