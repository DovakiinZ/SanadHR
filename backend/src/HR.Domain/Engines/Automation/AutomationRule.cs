using HR.Domain.Common;

namespace HR.Domain.Engines.Automation;

public class AutomationRule : TenantEntity
{
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; }

    public ICollection<AutomationTrigger> Triggers { get; set; } = new List<AutomationTrigger>();
    public ICollection<AutomationCondition> Conditions { get; set; } = new List<AutomationCondition>();
    public ICollection<AutomationAction> Actions { get; set; } = new List<AutomationAction>();
    public ICollection<AutomationExecutionLog> ExecutionLogs { get; set; } = new List<AutomationExecutionLog>();
}
