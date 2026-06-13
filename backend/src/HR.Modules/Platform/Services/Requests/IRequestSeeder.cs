namespace HR.Modules.Platform.Services.Requests;

public interface IRequestSeeder
{
    /// <summary>
    /// Idempotently provisions the built-in System Requests for the current tenant — each
    /// with a real Form, Workflow, Impact Mapping and (where relevant) Print Template.
    /// Guarantees "if visible, it is usable". Returns the count newly created.
    /// </summary>
    Task<int> SeedSystemRequestsAsync(CancellationToken ct);
}
