namespace HR.Domain.Common.ValueObjects;

public record Address(
    string? Street,
    string? City,
    string? Region,
    string? PostalCode,
    string Country = "SA"
);
