using FluentAssertions;
using HR.Domain.Engines.Settlement;
using HR.Domain.Enums;
using Xunit;

namespace HR.Modules.Employees.Tests;

/// <summary>Verifies the Saudi Labor Law end-of-service math: Art. 84 gratuity, Art. 85 resignation
/// tiers, and the Art. 77/80/81 scenarios. The calculator is pure, so these are fast and deterministic.</summary>
public class EndOfServiceCalculatorTests
{
    private const decimal Wage = 12_000m;
    private static readonly DateTime Termination = new(2025, 1, 1);

    /// <summary>Builds an input whose effective service is approximately <paramref name="years"/> years.</summary>
    private static SettlementInput ForYears(double years, TerminationScenario scenario,
        ContractTermType term = ContractTermType.Indefinite, DateTime? contractEnd = null, decimal unpaidDays = 0m)
        => new()
        {
            MonthlyWage = Wage,
            HireDate = Termination.AddDays(-Math.Round(years * 365.25)),
            TerminationDate = Termination,
            Scenario = scenario,
            ContractTermType = term,
            ContractEndDate = contractEnd,
            UnpaidLeaveDays = unpaidDays,
        };

    private static decimal ExpectedFullGratuity(decimal years)
        => 0.5m * Wage * Math.Min(years, 5m) + 1m * Wage * Math.Max(years - 5m, 0m);

    [Fact]
    public void NormalTermination_TenYears_UsesArt84Formula()
    {
        var r = EndOfServiceCalculator.Calculate(ForYears(10, TerminationScenario.NormalEmployerTermination));

        r.ServiceYears.Should().BeApproximately(10m, 0.02m);
        // ½·wage·5 + 1·wage·5 = 2.5 + 5 = 7.5 months ⇒ 90,000
        r.GratuityAmount.Should().BeApproximately(ExpectedFullGratuity(r.ServiceYears), 50m);
        r.Article77Award.Should().Be(0m);
        r.TotalAward.Should().Be(r.GratuityAmount);
    }

    [Fact]
    public void Article80_ForCause_AwardsNothing()
    {
        var r = EndOfServiceCalculator.Calculate(ForYears(8, TerminationScenario.Article80ForCause));

        r.GratuityAmount.Should().Be(0m);
        r.NoticeCompensation.Should().Be(0m);
        r.Article77Award.Should().Be(0m);
        r.TotalAward.Should().Be(0m);
        r.Lines.Should().Contain(l => l.ArticleRef == "Art. 80");
    }

    [Fact]
    public void Article81_EmployerBreach_PaysFullGratuity_NoResignationReduction()
    {
        const double years = 3; // resignation tier would otherwise cut this to one third
        var art81 = EndOfServiceCalculator.Calculate(ForYears(years, TerminationScenario.Article81EmployerBreachResignation));
        var resign = EndOfServiceCalculator.Calculate(ForYears(years, TerminationScenario.NormalResignation));

        art81.GratuityAmount.Should().BeApproximately(ExpectedFullGratuity(art81.ServiceYears), 50m);
        // Art. 81 pays the full gratuity; a normal resignation at 2–5 years pays only one third.
        resign.GratuityAmount.Should().BeApproximately(art81.GratuityAmount / 3m, 50m);
    }

    [Theory]
    [InlineData(1.0, 0.0)]      // < 2 years → nothing
    [InlineData(3.0, 1.0 / 3)]  // 2–5 years → one third
    [InlineData(7.0, 2.0 / 3)]  // 5–10 years → two thirds
    [InlineData(12.0, 1.0)]     // 10+ years → full
    public void NormalResignation_AppliesArt85Tiers(double years, double fraction)
    {
        var r = EndOfServiceCalculator.Calculate(ForYears(years, TerminationScenario.NormalResignation));
        var expected = ExpectedFullGratuity(r.ServiceYears) * (decimal)fraction;
        r.GratuityAmount.Should().BeApproximately(expected, 50m);
    }

    [Fact]
    public void Article77_Indefinite_ShortService_HitsTwoMonthFloor()
    {
        // 1 year indefinite: 15 days' wage ≈ half a month, well below the 2-month floor.
        var r = EndOfServiceCalculator.Calculate(ForYears(1, TerminationScenario.Article77InvalidTermination));

        r.Article77Award.Should().Be(2m * Wage); // floor wins
        r.GratuityAmount.Should().BeGreaterThan(0m); // gratuity is still due on top
        r.TotalAward.Should().Be(r.GratuityAmount + r.Article77Award);
        r.Lines.Should().Contain(l => l.ArticleRef == "Art. 77");
    }

    [Fact]
    public void Article77_Indefinite_LongService_Uses15DaysPerYear()
    {
        // 20 years indefinite: 15 days × 20 = 300 days' wage ≈ 10 months — above the 2-month floor.
        var r = EndOfServiceCalculator.Calculate(ForYears(20, TerminationScenario.Article77InvalidTermination));

        var dailyWage = Wage / 30m;
        var expected = 15m * dailyWage * r.ServiceYears;
        r.Article77Award.Should().BeApproximately(expected, 50m);
        r.Article77Award.Should().BeGreaterThan(2m * Wage);
    }

    [Fact]
    public void Article77_FixedTerm_PaysWagesForRemainderOfTerm()
    {
        // Fixed term ending 10 months after termination → remaining wages ≈ 10 months, above the floor.
        var contractEnd = Termination.AddDays(305);
        var r = EndOfServiceCalculator.Calculate(
            ForYears(2, TerminationScenario.Article77InvalidTermination, ContractTermType.FixedTerm, contractEnd));

        var dailyWage = Wage / 30m;
        var expectedRemainder = 305m * dailyWage;
        r.Article77Award.Should().BeApproximately(expectedRemainder, 1m);
        r.Article77Award.Should().BeGreaterThan(2m * Wage);
    }

    [Fact]
    public void UnpaidLeave_ReducesEffectiveServiceAndGratuity()
    {
        var without = EndOfServiceCalculator.Calculate(ForYears(6, TerminationScenario.NormalEmployerTermination));
        var with = EndOfServiceCalculator.Calculate(ForYears(6, TerminationScenario.NormalEmployerTermination, unpaidDays: 180m));

        with.EffectiveServiceDays.Should().BeLessThan(without.EffectiveServiceDays);
        with.ServiceYears.Should().BeApproximately(without.ServiceYears - (180m / 365.25m), 0.01m);
        with.GratuityAmount.Should().BeLessThan(without.GratuityAmount);
    }
}
