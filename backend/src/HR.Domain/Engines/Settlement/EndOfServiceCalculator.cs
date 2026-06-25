using HR.Domain.Enums;

namespace HR.Domain.Engines.Settlement;

/// <summary>Input to the end-of-service calculation. All side-effect-free — the caller is responsible
/// for loading the employee, resolving the monthly wage (basic + eligible allowances) and summing
/// unpaid-leave days.</summary>
public record SettlementInput
{
    public decimal MonthlyWage { get; init; }
    public DateTime HireDate { get; init; }
    public DateTime TerminationDate { get; init; }
    public ContractTermType ContractTermType { get; init; } = ContractTermType.Indefinite;
    public DateTime? ContractEndDate { get; init; }
    public TerminationScenario Scenario { get; init; } = TerminationScenario.NormalEmployerTermination;
    /// <summary>Total unpaid-leave (LWP) days over the service period. Shifts the seniority end-date:
    /// accrued service for the gratuity excludes these days.</summary>
    public decimal UnpaidLeaveDays { get; init; }
}

/// <summary>Pure computation of a Saudi Labor Law end-of-service settlement.
///
/// Gratuity (Art. 84): half a month's wage for each of the first five years of service, and a full
/// month's wage for each year thereafter, pro-rated for partial years.
///
/// Art. 85 resignation reductions apply to a normal resignation only: under 2 years → nothing,
/// 2–5 years → one third, 5–10 years → two thirds, 10+ years → full gratuity.
///
/// Art. 80 (dismissal for cause): no gratuity and no notice compensation.
/// Art. 81 (immediate resignation for employer breach): full gratuity, as if the employer had
/// terminated — the Art. 85 reduction does NOT apply.
/// Art. 77 (invalid termination by either party): a compensation award on top of the gratuity —
/// fixed-term contracts get the wages for the remainder of the term; indefinite contracts get
/// 15 days' wage for each year of service; in both cases not less than two months' wage.</summary>
public static class EndOfServiceCalculator
{
    private const decimal DaysPerYear = 365.25m;
    private const decimal DaysPerMonth = 30m;   // Saudi convention for converting a monthly wage to a daily wage

    public static SettlementResult Calculate(SettlementInput input)
    {
        var monthlyWage = Math.Max(0m, input.MonthlyWage);
        var dailyWage = monthlyWage / DaysPerMonth;

        var calendarDays = (decimal)(input.TerminationDate.Date - input.HireDate.Date).TotalDays;
        if (calendarDays < 0) calendarDays = 0;
        var effectiveDays = Math.Max(0m, calendarDays - Math.Max(0m, input.UnpaidLeaveDays));
        var serviceYears = effectiveDays / DaysPerYear;

        var lines = new List<SettlementLine>();

        // --- Base gratuity (Art. 84): ½-month/yr for first 5 years, 1-month/yr thereafter ---
        var firstFive = Math.Min(serviceYears, 5m);
        var beyondFive = Math.Max(serviceYears - 5m, 0m);
        var fullGratuity = (0.5m * monthlyWage * firstFive) + (1m * monthlyWage * beyondFive);

        decimal gratuity;
        decimal notice = 0m;
        decimal article77 = 0m;

        switch (input.Scenario)
        {
            case TerminationScenario.Article80ForCause:
                // Dismissal for cause — no gratuity, no notice compensation.
                gratuity = 0m;
                notice = 0m;
                lines.Add(new SettlementLine(
                    "End-of-service gratuity forfeited (dismissal for cause)",
                    "سقوط مكافأة نهاية الخدمة (فصل لسبب مشروع)",
                    "Art. 80", 0m));
                break;

            case TerminationScenario.NormalResignation:
                // Resignation — gratuity reduced by the Art. 85 tier.
                var factor = ResignationFactor(serviceYears);
                gratuity = Math.Round(fullGratuity * factor, 2);
                lines.Add(new SettlementLine(
                    $"End-of-service gratuity (resignation, {FactorLabelEn(factor)})",
                    $"مكافأة نهاية الخدمة (استقالة، {FactorLabelAr(factor)})",
                    "Art. 84 / 85", gratuity));
                break;

            case TerminationScenario.Article81EmployerBreachResignation:
                // Immediate resignation for employer breach — full gratuity, no Art. 85 reduction.
                gratuity = Math.Round(fullGratuity, 2);
                lines.Add(new SettlementLine(
                    "End-of-service gratuity (full — resignation for employer breach)",
                    "مكافأة نهاية الخدمة (كاملة — ترك العمل لإخلال صاحب العمل)",
                    "Art. 81 / 84", gratuity));
                break;

            case TerminationScenario.Article77InvalidTermination:
                // Invalid termination — full gratuity is still due, plus the Art. 77 award.
                gratuity = Math.Round(fullGratuity, 2);
                lines.Add(new SettlementLine(
                    "End-of-service gratuity (full)",
                    "مكافأة نهاية الخدمة (كاملة)",
                    "Art. 84", gratuity));
                article77 = Math.Round(ComputeArticle77Award(input, monthlyWage, dailyWage, serviceYears), 2);
                lines.Add(new SettlementLine(
                    input.ContractTermType == ContractTermType.FixedTerm
                        ? "Invalid-termination compensation (wages for remainder of term, min 2 months)"
                        : "Invalid-termination compensation (15 days' wage per year, min 2 months)",
                    input.ContractTermType == ContractTermType.FixedTerm
                        ? "تعويض الفصل غير المشروع (أجر المدة المتبقية، بحد أدنى شهرين)"
                        : "تعويض الفصل غير المشروع (أجر 15 يوماً عن كل سنة، بحد أدنى شهرين)",
                    "Art. 77", article77));
                break;

            case TerminationScenario.NormalEmployerTermination:
            default:
                gratuity = Math.Round(fullGratuity, 2);
                lines.Add(new SettlementLine(
                    "End-of-service gratuity",
                    "مكافأة نهاية الخدمة",
                    "Art. 84", gratuity));
                break;
        }

        var total = gratuity + article77 + notice;

        return new SettlementResult
        {
            Scenario = input.Scenario,
            ContractTermType = input.ContractTermType,
            MonthlyWage = monthlyWage,
            DailyWage = Math.Round(dailyWage, 2),
            EffectiveServiceDays = Math.Round(effectiveDays, 2),
            UnpaidLeaveDays = Math.Round(Math.Max(0m, input.UnpaidLeaveDays), 2),
            ServiceYears = Math.Round(serviceYears, 4),
            GratuityAmount = gratuity,
            Article77Award = article77,
            NoticeCompensation = notice,
            TotalAward = Math.Round(total, 2),
            Lines = lines,
        };
    }

    private static decimal ComputeArticle77Award(SettlementInput input, decimal monthlyWage, decimal dailyWage, decimal serviceYears)
    {
        decimal award;
        if (input.ContractTermType == ContractTermType.FixedTerm)
        {
            // Wages for the remainder of the fixed term.
            var remainingDays = input.ContractEndDate is { } end
                ? Math.Max(0m, (decimal)(end.Date - input.TerminationDate.Date).TotalDays)
                : 0m;
            award = remainingDays * dailyWage;
        }
        else
        {
            // 15 days' wage for each year of service.
            award = 15m * dailyWage * serviceYears;
        }

        // Statutory floor: not less than two months' wage.
        return Math.Max(award, 2m * monthlyWage);
    }

    /// <summary>Art. 85 resignation gratuity fraction by years of service.</summary>
    private static decimal ResignationFactor(decimal years)
    {
        if (years < 2m) return 0m;
        if (years < 5m) return 1m / 3m;
        if (years < 10m) return 2m / 3m;
        return 1m;
    }

    private static string FactorLabelEn(decimal factor) =>
        factor == 0m ? "no entitlement" : factor == 1m ? "full" : factor < 0.5m ? "one third" : "two thirds";

    private static string FactorLabelAr(decimal factor) =>
        factor == 0m ? "لا يستحق" : factor == 1m ? "كاملة" : factor < 0.5m ? "الثلث" : "الثلثين";
}
