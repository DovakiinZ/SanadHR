namespace HR.Modules.Platform.Services.Requests;

/// <summary>One submitted form field value.</summary>
public sealed class RequestValueInput
{
    public Guid? FormFieldId { get; set; }
    public string FieldCode { get; set; } = null!;
    public string? Value { get; set; }
    public string? FileUrl { get; set; }
}

/// <summary>Approval chain step as stored in a request workflow's version Configuration JSON.</summary>
public sealed class WorkflowStepConfig
{
    public int ApproverType { get; set; }       // maps to HR.Domain.Enums.ApproverType
    public string NameAr { get; set; } = "";
    public string NameEn { get; set; } = "";
    public Guid? SpecificUserId { get; set; }
}

public sealed class WorkflowChainConfig
{
    public List<WorkflowStepConfig> Steps { get; set; } = new();
}

/// <summary>Well-known form field codes the engine understands for system requests.</summary>
public static class RequestFieldCodes
{
    public const string StartDate = "startDate";
    public const string EndDate = "endDate";
    public const string Days = "days";
    public const string Amount = "amount";
    public const string Reason = "reason";
    public const string Notes = "notes";
}
