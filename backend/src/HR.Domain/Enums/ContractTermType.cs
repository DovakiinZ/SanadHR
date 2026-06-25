namespace HR.Domain.Enums;

/// <summary>Whether the employment contract runs for a fixed term (with an end date) or
/// indefinitely. Drives the Article 77 invalid-termination compensation rule: fixed-term
/// awards the wages for the remainder of the term, indefinite awards 15 days' wage per year.</summary>
public enum ContractTermType
{
    Indefinite = 1,
    FixedTerm = 2,
}
