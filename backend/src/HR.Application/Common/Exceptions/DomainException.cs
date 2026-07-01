namespace HR.Application.Common.Exceptions;

/// <summary>
/// A business-rule violation that is the caller's fault but is not a field-validation,
/// not-found, forbidden, or conflict error (e.g. "type is inactive", "amount must be
/// non-negative"). Surfaces as HTTP 422 with the message shown to the user.
/// Prefer this over <see cref="InvalidOperationException"/> for new domain checks so the
/// reason reaches the client instead of being swallowed as a generic 500.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
