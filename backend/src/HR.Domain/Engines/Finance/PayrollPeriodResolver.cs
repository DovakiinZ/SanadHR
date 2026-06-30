namespace HR.Domain.Engines.Finance;

/// <summary>Resolves which payroll period a dated transaction belongs to, honoring the definition
/// version's cutoff. A transaction whose EffectiveDate falls after the cutoff day carries to the next
/// period when the version allows it. Pure — no DB, no I/O — reused by the run-time consumer and the
/// create-time impact preview so the two never drift.</summary>
public static class PayrollPeriodResolver
{
    public static (int Year, int Month) Resolve(DateTime effectiveDate, int cutoffDay, bool carryToNextPeriod)
    {
        var year = effectiveDate.Year;
        var month = effectiveDate.Month;
        if (carryToNextPeriod && effectiveDate.Day > cutoffDay)
        {
            month++;
            if (month > 12) { month = 1; year++; }
        }
        return (year, month);
    }
}
