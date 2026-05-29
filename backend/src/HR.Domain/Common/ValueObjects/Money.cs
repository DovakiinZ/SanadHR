namespace HR.Domain.Common.ValueObjects;

public record Money(decimal Amount, string Currency = "SAR");
