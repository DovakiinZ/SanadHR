namespace HR.Domain.Engines.Finance;

/// <summary>The pay period a run covers. A value object — half-open conceptually but stored as inclusive
/// start/end dates with the owning year/month for monthly cycles.</summary>
public readonly record struct PayrollPeriod
{
    public DateTime Start { get; }
    public DateTime End { get; }
    public int Year { get; }
    public int Month { get; }
    public string? Label { get; }

    public PayrollPeriod(DateTime start, DateTime end, string? label = null)
    {
        if (end < start) throw new ArgumentException("Period end cannot be before start.", nameof(end));
        Start = start.Date;
        End = end.Date;
        Year = start.Year;
        Month = start.Month;
        Label = label;
    }

    /// <summary>A whole calendar month, e.g. Monthly(2026, 6) → 2026-06-01 .. 2026-06-30.</summary>
    public static PayrollPeriod Monthly(int year, int month)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1).AddDays(-1);
        return new PayrollPeriod(start, end, $"{year}-{month:D2}");
    }

    public bool Overlaps(DateTime otherStart, DateTime otherEnd) =>
        Start <= otherEnd.Date && otherStart.Date <= End;

    public override string ToString() => Label ?? $"{Start:yyyy-MM-dd}..{End:yyyy-MM-dd}";
}
