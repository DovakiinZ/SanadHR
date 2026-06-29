using HR.Application.Engines.Finance;

namespace HR.Modules.Payroll.DTOs;

public sealed class PayrollTypeListItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? NameAr { get; set; }
    public Guid? CategoryId { get; set; }
    public string Status { get; set; } = "";
    public Guid? CurrentVersionId { get; set; }
    public int VersionCount { get; set; }
}

public sealed class PayrollTypeDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public string? NameAr { get; set; }
    public Guid? CategoryId { get; set; }
    public string Status { get; set; } = "";
    public Guid? CurrentVersionId { get; set; }
    public List<PayrollVersionDto> Versions { get; set; } = new();
}

public sealed class PayrollVersionDto
{
    public Guid Id { get; set; }
    public int VersionNumber { get; set; }
    public string Status { get; set; } = "";
    public int CutoffDay { get; set; }
    public string DayBasis { get; set; } = "";
    public DateTime? ClosingDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool CarryToNextPeriod { get; set; }
    public Guid? DefaultExportFormatId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public Guid? ApprovalWorkflowId { get; set; }
    public Guid? RuleSetVersionId { get; set; }
    public string Currency { get; set; } = "SAR";
    public string Frequency { get; set; } = "";
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? SelectionScopeJson { get; set; }
    public string? CalcSettingsJson { get; set; }
    public string? PaymentMethodScopeJson { get; set; }
}

public sealed class CreateTypeRequest { public string Code { get; set; } = ""; public string Name { get; set; } = ""; public string? NameAr { get; set; } public Guid? CategoryId { get; set; } }
public sealed class UpdateHeaderRequest { public string Name { get; set; } = ""; public string? NameAr { get; set; } public Guid? CategoryId { get; set; } public string Status { get; set; } = "Active"; }
public sealed class UpdateVersionRequest : UpdatePayrollVersionDtoBase { }
public sealed class ScopeDimensionDto { public string Key { get; set; } = ""; public string NameEn { get; set; } = ""; public string NameAr { get; set; } = ""; public string ValueSourceKind { get; set; } = ""; public string? ValueSourceRef { get; set; } public bool IsAvailable { get; set; } public string? UnavailableNote { get; set; } }
public sealed class ResolveScopeRequest { public string ScopeJson { get; set; } = ""; }
public sealed class ResolveScopeResult { public int IncludedCount { get; set; } public int ExcludedCount { get; set; } public List<string> Warnings { get; set; } = new(); }
public sealed class SimulateRequest { public int Year { get; set; } public int Month { get; set; } }

/// <summary>Mirror of UpdatePayrollVersionArgs over the wire.</summary>
public abstract class UpdatePayrollVersionDtoBase
{
    public int? CutoffDay { get; set; }
    public string? DayBasis { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public bool? CarryToNextPeriod { get; set; }
    public Guid? DefaultExportFormatId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public Guid? ApprovalWorkflowId { get; set; }
    public Guid? RuleSetVersionId { get; set; }
    public string? Currency { get; set; }
    public string? Frequency { get; set; }
    public string? SelectionScopeJson { get; set; }
    public string? CalcSettingsJson { get; set; }
    public string? PaymentMethodScopeJson { get; set; }
}
