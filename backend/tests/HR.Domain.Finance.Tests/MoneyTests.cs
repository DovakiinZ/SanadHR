using FluentAssertions;
using HR.Domain.Engines.Finance;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class MoneyTests
{
    [Fact]
    public void Adds_and_subtracts_same_currency()
    {
        (new Money(100m, "SAR") + new Money(50m, "SAR")).Should().Be(new Money(150m, "SAR"));
        (new Money(100m, "SAR") - new Money(30m, "SAR")).Should().Be(new Money(70m, "SAR"));
    }

    [Fact]
    public void Multiplies_by_scalar()
    {
        (new Money(2000m, "SAR") * 0.0975m).Should().Be(new Money(195m, "SAR"));
    }

    [Fact]
    public void Rounds_away_from_zero()
    {
        new Money(3.145m, "SAR").Round(2).Amount.Should().Be(3.15m);
    }

    [Fact]
    public void Mixing_currencies_throws()
    {
        var act = () => new Money(100m, "SAR") + new Money(100m, "USD");
        act.Should().Throw<InvalidOperationException>().WithMessage("*Currency mismatch*");
    }

    [Fact]
    public void Normalizes_currency_code()
    {
        new Money(1m, "sar").Currency.Should().Be("SAR");
    }

    [Fact]
    public void Requires_a_currency()
    {
        var act = () => new Money(1m, " ");
        act.Should().Throw<ArgumentException>();
    }
}
