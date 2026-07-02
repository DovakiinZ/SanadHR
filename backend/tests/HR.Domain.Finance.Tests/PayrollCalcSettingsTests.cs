using FluentAssertions;
using HR.Infrastructure.Engines.Finance;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class PayrollCalcSettingsTests
{
    [Fact]
    public void Rates_default_when_absent()
    {
        var r = PayrollCalcSettings.Rates(null);
        r.Absence.Should().Be(1.0m); r.Late.Should().Be(1.0m); r.Shortage.Should().Be(1.0m); r.Overtime.Should().Be(1.5m);
    }

    [Fact]
    public void Rates_read_overrides()
    {
        var json = "{\"attendanceRates\":{\"absenceMultiplier\":2,\"lateMultiplier\":1.25,\"shortageMultiplier\":0.5,\"overtimeMultiplier\":2}}";
        var r = PayrollCalcSettings.Rates(json);
        r.Absence.Should().Be(2m); r.Late.Should().Be(1.25m); r.Shortage.Should().Be(0.5m); r.Overtime.Should().Be(2m);
    }

    [Fact]
    public void IncludeOvertime_defaults_false_and_reads_true()
    {
        PayrollCalcSettings.IncludeOvertime(null).Should().BeFalse();
        PayrollCalcSettings.IncludeOvertime("{\"includeOvertime\":true}").Should().BeTrue();
    }
}
