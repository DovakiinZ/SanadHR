using HR.Domain.Engines.Settlement;

namespace HR.Modules.Employees.DTOs;

/// <summary>One line of a settlement breakdown, tagged with its Saudi Labor Law article.</summary>
public record SettlementLineDto(string LabelEn, string LabelAr, string ArticleRef, decimal Amount);

/// <summary>API shape of an end-of-service settlement (preview or persisted).</summary>
public record SettlementResultDto
{
    /// <summary>Set only when the settlement has been persisted (terminate), null for a preview.</summary>
    public Guid? SettlementId { get; init; }

    public string Scenario { get; init; } = null!;
    public string ContractTermType { get; init; } = null!;
    public string Currency { get; init; } = "SAR";

    public decimal MonthlyWage { get; init; }
    public decimal DailyWage { get; init; }
    public decimal ServiceYears { get; init; }
    public decimal EffectiveServiceDays { get; init; }
    public decimal UnpaidLeaveDays { get; init; }

    public decimal GratuityAmount { get; init; }
    public decimal Article77Award { get; init; }
    public decimal NoticeCompensation { get; init; }
    public decimal TotalAward { get; init; }

    public List<SettlementLineDto> Lines { get; init; } = new();

    public static SettlementResultDto From(SettlementResult r, Guid? settlementId = null) => new()
    {
        SettlementId = settlementId,
        Scenario = r.Scenario.ToString(),
        ContractTermType = r.ContractTermType.ToString(),
        Currency = r.Currency,
        MonthlyWage = r.MonthlyWage,
        DailyWage = r.DailyWage,
        ServiceYears = r.ServiceYears,
        EffectiveServiceDays = r.EffectiveServiceDays,
        UnpaidLeaveDays = r.UnpaidLeaveDays,
        GratuityAmount = r.GratuityAmount,
        Article77Award = r.Article77Award,
        NoticeCompensation = r.NoticeCompensation,
        TotalAward = r.TotalAward,
        Lines = r.Lines.Select(l => new SettlementLineDto(l.LabelEn, l.LabelAr, l.ArticleRef, l.Amount)).ToList(),
    };
}
