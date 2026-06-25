using HR.Domain.Enums;

namespace HR.Domain.Engines.Settlement;

/// <summary>One line of an end-of-service settlement breakdown, tagged with the Saudi Labor Law
/// article it derives from so the basis for every figure is auditable.</summary>
public record SettlementLine(string LabelEn, string LabelAr, string ArticleRef, decimal Amount);

/// <summary>The computed result of an end-of-service settlement: the wage base, derived service
/// length, each statutory component, and the itemized breakdown. Side-effect free — produced by
/// <see cref="EndOfServiceCalculator"/> and persisted as a TerminationSettlement.</summary>
public record SettlementResult
{
    public TerminationScenario Scenario { get; init; }
    public ContractTermType ContractTermType { get; init; }
    public string Currency { get; init; } = "SAR";

    public decimal MonthlyWage { get; init; }
    public decimal DailyWage { get; init; }

    /// <summary>Calendar service minus unpaid-leave days, in days.</summary>
    public decimal EffectiveServiceDays { get; init; }
    public decimal UnpaidLeaveDays { get; init; }
    /// <summary>Effective service expressed in fractional years (effective days / 365.25).</summary>
    public decimal ServiceYears { get; init; }

    /// <summary>End-of-service gratuity (Art. 84), after any Art. 85 resignation reduction.</summary>
    public decimal GratuityAmount { get; init; }
    /// <summary>Invalid-termination compensation (Art. 77) — zero unless the scenario is Article 77.</summary>
    public decimal Article77Award { get; init; }
    /// <summary>Notice-period compensation — explicitly zero under Art. 80.</summary>
    public decimal NoticeCompensation { get; init; }

    public decimal TotalAward { get; init; }

    public IReadOnlyList<SettlementLine> Lines { get; init; } = new List<SettlementLine>();
}
