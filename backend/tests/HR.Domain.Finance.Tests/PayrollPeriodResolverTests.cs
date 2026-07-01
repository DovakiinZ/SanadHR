using FluentAssertions;
using HR.Domain.Engines.Finance;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollPeriodResolverTests
{
    [Fact]
    public void Before_cutoff_stays_in_same_month() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 7, 20), 27, true).Should().Be((2026, 7));

    [Fact]
    public void On_cutoff_day_stays_in_same_month() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 7, 27), 27, true).Should().Be((2026, 7));

    [Fact]
    public void After_cutoff_with_carry_moves_to_next_month() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 7, 28), 27, true).Should().Be((2026, 8));

    [Fact]
    public void After_cutoff_without_carry_stays_in_same_month() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 7, 28), 27, false).Should().Be((2026, 7));

    [Fact]
    public void December_after_cutoff_rolls_into_next_year() =>
        PayrollPeriodResolver.Resolve(new DateTime(2026, 12, 31), 27, true).Should().Be((2027, 1));
}
