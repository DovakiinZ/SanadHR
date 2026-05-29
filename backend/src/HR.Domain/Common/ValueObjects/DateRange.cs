namespace HR.Domain.Common.ValueObjects;

public record DateRange(DateTime Start, DateTime End)
{
    public int TotalDays => (End - Start).Days;
    public bool Contains(DateTime date) => date >= Start && date <= End;
    public bool Overlaps(DateRange other) => Start < other.End && End > other.Start;
}
