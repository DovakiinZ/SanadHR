namespace HR.Modules.Platform.DTOs.Automation;

public class AutomationRuleDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public List<AutomationTriggerDto> Triggers { get; set; } = new();
    public List<AutomationConditionDto> Conditions { get; set; } = new();
    public List<AutomationActionDto> Actions { get; set; } = new();
}

public class AutomationTriggerDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public string? Configuration { get; set; }
}

public class AutomationConditionDto
{
    public Guid Id { get; set; }
    public string Field { get; set; } = null!;
    public string Operator { get; set; } = null!;
    public string Value { get; set; } = null!;
    public string? LogicalOperator { get; set; }
}

public class AutomationActionDto
{
    public Guid Id { get; set; }
    public string ActionType { get; set; } = null!;
    public string? Configuration { get; set; }
    public int SortOrder { get; set; }
}
