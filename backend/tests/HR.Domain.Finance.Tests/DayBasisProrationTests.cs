using HR.Domain.Enums;
using HR.Infrastructure.Engines.Finance;
using Xunit;

namespace HR.Domain.Finance.Tests;

public class DayBasisProrationTests
{
    // monthlyWage = 3000, Feb 2026 has 28 days, 20 working days (example).
    [Theory]
    [InlineData(DayBasis.Fixed30, 3000, 2026, 2, 20, 100.0)]            // 3000/30
    [InlineData(DayBasis.CalendarMonth, 3000, 2026, 2, 20, 107.1429)]  // 3000/28
    [InlineData(DayBasis.WorkingDays, 3000, 2026, 2, 20, 150.0)]       // 3000/20
    public void DailyWage_matches_basis(DayBasis basis, decimal monthlyWage, int year, int month, int workingDays, double expected)
    {
        var daily = PayrollFactProvider.DailyWageFor(basis, monthlyWage, year, month, workingDays);
        Assert.Equal((decimal)expected, decimal.Round(daily, 4));
    }
}
