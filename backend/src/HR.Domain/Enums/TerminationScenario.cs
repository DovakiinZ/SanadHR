namespace HR.Domain.Enums;

/// <summary>The legal trigger for an end-of-service settlement. Determines which Saudi Labor Law
/// articles apply to the award computation.</summary>
public enum TerminationScenario
{
    /// <summary>Employer ends the contract for a valid reason — standard end-of-service gratuity (Art. 84).</summary>
    NormalEmployerTermination = 1,

    /// <summary>Employee resigns under normal circumstances — gratuity reduced per the Art. 85 tiers.</summary>
    NormalResignation = 2,

    /// <summary>Termination by either party for an invalid reason — Art. 77 compensation on top of gratuity.</summary>
    Article77InvalidTermination = 3,

    /// <summary>Employer dismisses for cause (Art. 80 violations) — zero gratuity, zero notice compensation.</summary>
    Article80ForCause = 4,

    /// <summary>Employee leaves immediately due to an employer breach (Art. 81) — full gratuity, no Art. 85 reduction.</summary>
    Article81EmployerBreachResignation = 5,
}
