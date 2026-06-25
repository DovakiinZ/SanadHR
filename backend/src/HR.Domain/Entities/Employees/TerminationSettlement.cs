using HR.Domain.Common;
using HR.Domain.Enums;

namespace HR.Modules.Employees.Entities;

/// <summary>A persisted end-of-service settlement computed when an employee is terminated or resigns.
/// Snapshots the wage base, derived service length and each statutory component (gratuity, Art. 77
/// award, notice) so the figures are reproducible and auditable. The itemized basis lives in
/// <see cref="Items"/>.</summary>
public class TerminationSettlement : TenantEntity
{
    public Guid EmployeeId { get; set; }

    public DateTime HireDate { get; set; }
    public DateTime TerminationDate { get; set; }

    public TerminationScenario Scenario { get; set; }
    public ContractTermType ContractTermType { get; set; }

    public decimal MonthlyWage { get; set; }
    public decimal DailyWage { get; set; }
    public decimal ServiceYears { get; set; }
    public decimal EffectiveServiceDays { get; set; }
    public decimal UnpaidLeaveDays { get; set; }

    public decimal GratuityAmount { get; set; }
    public decimal Article77Award { get; set; }
    public decimal NoticeCompensation { get; set; }
    public decimal TotalAward { get; set; }
    public string Currency { get; set; } = "SAR";

    public Guid? ComputedByUserId { get; set; }
    public DateTime ComputedAt { get; set; }

    public string? Notes { get; set; }

    public ICollection<TerminationSettlementItem> Items { get; set; } = new List<TerminationSettlementItem>();
}
