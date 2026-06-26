using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>A reusable, versioned library of calculation rules. A payroll definition version points at a
/// specific <see cref="RuleSetVersion"/>, so the formulas that produced any historical run are frozen and
/// auditable. Nothing is hardcoded — every calculation lives here as an editable expression.</summary>
public class RuleSet : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }

    public RuleSetStatus Status { get; set; } = RuleSetStatus.Draft;
    public Guid? CurrentVersionId { get; set; }

    public ICollection<RuleSetVersion> Versions { get; set; } = new List<RuleSetVersion>();
}

/// <summary>An immutable version of a rule set, owning the concrete <see cref="Rule"/>s. Editing forks a
/// new Draft; publishing supersedes the previous version.</summary>
public class RuleSetVersion : TenantEntity
{
    public Guid RuleSetId { get; set; }
    public RuleSet? RuleSet { get; set; }

    public int VersionNumber { get; set; }
    public VersionStatus Status { get; set; } = VersionStatus.Draft;

    public DateTime? PublishedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }

    public ICollection<Rule> Rules { get; set; } = new List<Rule>();
}

/// <summary>A single calculation rule. <see cref="Code"/> is the variable it writes (referenceable by
/// other rules); <see cref="ConditionText"/> is an optional guard ("IF …"); <see cref="ExpressionText"/>
/// is the value formula. Both are stored as authored source AND compiled AST JSON so they evaluate
/// identically years later. Execution order is derived from inter-rule dependencies, not from
/// <see cref="Sequence"/> (which is only an authoring hint / tie-breaker).</summary>
public class Rule : TenantEntity
{
    public Guid RuleSetVersionId { get; set; }
    public RuleSetVersion? Version { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }

    public PayComponentKind Kind { get; set; } = PayComponentKind.Earning;

    /// <summary>Authoring hint and deterministic tie-breaker; not the source of execution order.</summary>
    public int Sequence { get; set; }

    public string? ConditionText { get; set; }
    public string? ConditionAstJson { get; set; }

    public string ExpressionText { get; set; } = string.Empty;
    public string? ExpressionAstJson { get; set; }

    /// <summary>The pay/financial component this rule produces (e.g. BONUS, HOUSING, GOSI_EE).</summary>
    public string OutputComponentCode { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

/// <summary>A reusable named formula ("function") callable from any rule expression — the tenant-defined
/// extension of the built-in function library. Versioned for reproducibility.</summary>
public class FormulaFunction : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Comma-separated ordered parameter names referenced inside the body.</summary>
    public string ParametersCsv { get; set; } = string.Empty;

    public string ExpressionText { get; set; } = string.Empty;
    public string? ExpressionAstJson { get; set; }

    public VersionStatus Status { get; set; } = VersionStatus.Draft;
    public int Version { get; set; } = 1;
}
