using FluentAssertions;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Enums;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollTransactionMergeTests
{
    [Fact]
    public void Apply_adds_addition_as_earning_and_deduction_and_recomputes_totals()
    {
        var baseEval = new RuleSetEvaluation(
            new List<ComponentResult> { new("BASIC", "BASIC", PayComponentKind.Earning, 1000m, true) },
            new List<string> { "BASIC" }, 1000m, 0m, 1000m);

        var txns = new List<ConsumableTransaction>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), PayrollTransactionKind.Addition, "BONUS", 200m, default),
            new(Guid.NewGuid(), Guid.NewGuid(), PayrollTransactionKind.Deduction, "PENALTY", 50m, default),
        };

        var result = PayrollTransactionMerge.Apply(baseEval, txns);

        result.Components.Should().HaveCount(3);
        result.GrossEarnings.Should().Be(1200m);
        result.TotalDeductions.Should().Be(50m);
        result.NetAmount.Should().Be(1150m);
        result.Components[1].ComponentCode.Should().Be("BONUS");
        result.Components[1].Code.Should().StartWith("TXN:");
        result.Components[1].Kind.Should().Be(PayComponentKind.Earning);
        result.Components[2].Kind.Should().Be(PayComponentKind.Deduction);
    }

    [Fact]
    public void Apply_with_no_transactions_returns_evaluation_unchanged()
    {
        var baseEval = new RuleSetEvaluation(
            new List<ComponentResult> { new("BASIC", "BASIC", PayComponentKind.Earning, 1000m, true) },
            new List<string> { "BASIC" }, 1000m, 0m, 1000m);

        PayrollTransactionMerge.Apply(baseEval, new List<ConsumableTransaction>()).Should().BeSameAs(baseEval);
    }
}
