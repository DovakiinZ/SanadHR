using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Domain.Engines.Finance.Entities;

/// <summary>A payroll definition is a complete, versioned policy object — not a bag of settings. It is the
/// template from which payroll runs are produced. The logical definition owns an ordered set of immutable
/// <see cref="PayrollDefinitionVersion"/>s; <see cref="CurrentVersionId"/> points at the published one.
/// Unlimited definitions per tenant support Monthly, Weekly, Executive, Bonus, Settlement, Intern,
/// Commission payrolls side by side.</summary>
public class PayrollDefinition : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? NameAr { get; set; }
    public string? Description { get; set; }

    public PayrollScope Scope { get; set; } = PayrollScope.Company;
    public PayrollDefinitionStatus Status { get; set; } = PayrollDefinitionStatus.Draft;

    /// <summary>Master-data PayrollTypeCategory item id (Regular / Mudad / Cash / Bonus / EOS / Off-cycle).</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>The currently published version used when launching new runs.</summary>
    public Guid? CurrentVersionId { get; set; }

    public ICollection<PayrollDefinitionVersion> Versions { get; set; } = new List<PayrollDefinitionVersion>();
}

/// <summary>An immutable snapshot of a payroll definition's configuration. Editing a published definition
/// forks a new Draft version; a payroll run pins the exact version it was produced from, so historical
/// runs are never affected by later edits.</summary>
public class PayrollDefinitionVersion : TenantEntity
{
    public Guid PayrollDefinitionId { get; set; }
    public PayrollDefinition? Definition { get; set; }

    public int VersionNumber { get; set; }
    public VersionStatus Status { get; set; } = VersionStatus.Draft;

    public PayFrequency Frequency { get; set; } = PayFrequency.Monthly;

    /// <summary>Specification (JSON) selecting the employee population, e.g. departments, grades, filters.</summary>
    public string? EmployeeFilterJson { get; set; }

    /// <summary>Cycle configuration (JSON): period anchor, cutoff day, pay day, proration policy, etc.</summary>
    public string? CycleConfigJson { get; set; }

    // --- Payroll Type configuration (sub-project 1). Typed columns for hot/queryable fields. ---
    public int CutoffDay { get; set; } = 27;
    public DayBasis DayBasis { get; set; } = DayBasis.CalendarMonth;
    public DateTime? ClosingDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool CarryToNextPeriod { get; set; } = true;
    public Guid? DefaultExportFormatId { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsSimulation { get; set; }

    /// <summary>Rich employee selection (include/exclude across registered scope dimensions).</summary>
    public string? SelectionScopeJson { get; set; }
    /// <summary>Calculation toggles (include/exclude allowances/additions/etc.). DayBasis is the typed column.</summary>
    public string? CalcSettingsJson { get; set; }
    /// <summary>Allowed master-data PaymentMethod ids for this type.</summary>
    public string? PaymentMethodScopeJson { get; set; }

    public Guid? PaymentMethodId { get; set; }
    public Guid? WorkingCalendarId { get; set; }
    public Guid? ApprovalWorkflowId { get; set; }

    /// <summary>The exact rule-set version this definition computes with — pinned for reproducibility.</summary>
    public Guid? RuleSetVersionId { get; set; }

    public string Currency { get; set; } = "SAR";
    public string? Notes { get; set; }

    public DateTime? PublishedAt { get; set; }
    public Guid? PublishedByUserId { get; set; }
}
