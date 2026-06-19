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

    /// <summary>Legacy: a pre-resolved user id (kept for backward compatibility with seeded chains).</summary>
    public Guid? SpecificUserId { get; set; }

    /// <summary>Entity the approver type points at — meaning depends on ApproverType:
    /// SpecificUser → employee id, Role → role id, (future) department/branch id.</summary>
    public Guid? SpecificEntityId { get; set; }

    /// <summary>For ManagerChain: how many levels up the management chain (1 = direct manager).</summary>
    public int ChainLevel { get; set; } = 1;

    // Step rules (business-user toggles). Stored on the resolved RequestApproval row.
    public bool Required { get; set; } = true;
    public bool CanReject { get; set; } = true;
    public bool CanReturn { get; set; } = true;
    public bool CanDelegate { get; set; }

    /// <summary>Optional no-code conditions; ALL must hold for this step to apply (else it is skipped).</summary>
    public List<StepConditionConfig> Conditions { get; set; } = new();
}

/// <summary>One no-code condition row: a known property compared to a value.</summary>
public sealed class StepConditionConfig
{
    public string Field { get; set; } = "";      // leaveDays, amount, department, branch, leaveType, employmentType, jobTitle, or a form field code
    public string Operator { get; set; } = "eq"; // eq, neq, gt, gte, lt, lte, contains
    public string Value { get; set; } = "";
}

public sealed class WorkflowChainConfig
{
    public List<WorkflowStepConfig> Steps { get; set; } = new();
}

/// <summary>
/// Pure no-code condition evaluation, shared by the engine and unit-testable in isolation.
/// A step joins the chain only when ALL of its conditions hold.
/// </summary>
public static class RequestConditions
{
    public static bool Met(IEnumerable<StepConditionConfig>? conditions, IReadOnlyDictionary<string, string?> ctx)
    {
        if (conditions is null) return true;
        foreach (var c in conditions)
            if (!Evaluate(ctx.TryGetValue(c.Field, out var v) ? v : null, c.Operator, c.Value)) return false;
        return true;
    }

    public static bool Evaluate(string? actual, string? op, string? expected)
    {
        actual ??= string.Empty; expected ??= string.Empty;
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        var numeric = double.TryParse(actual, System.Globalization.NumberStyles.Any, ci, out var a)
                      & double.TryParse(expected, System.Globalization.NumberStyles.Any, ci, out var b);
        return (op ?? "eq").ToLowerInvariant() switch
        {
            "eq" => numeric ? a == b : string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "neq" => numeric ? a != b : !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "gt" => numeric && a > b,
            "gte" => numeric && a >= b,
            "lt" => numeric && a < b,
            "lte" => numeric && a <= b,
            "contains" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }
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
    public const string LeaveType = "leaveType";   // generic sub-type: selected LeaveType master-data id
    public const string Attachment = "attachment";
}
