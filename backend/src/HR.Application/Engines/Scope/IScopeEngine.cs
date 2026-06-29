namespace HR.Application.Engines.Scope;

public interface IScopeEngine
{
    /// <summary>Every dimension known to the system — available and disabled-with-note — for the UI.</summary>
    IReadOnlyList<ScopeDimensionInfo> Dimensions();

    Task<ScopeResolution> ResolveAsync(SelectionScope scope, CancellationToken ct);
}
