namespace HR.Application.Engines.Scope;

/// <summary>Implemented by the module that OWNS a dimension's data. Payroll never implements these.</summary>
public interface IScopeDimensionProvider
{
    string DimensionKey { get; }
    ScopeDimensionInfo Info { get; }
    Task<ISet<Guid>> ResolveEmployeesAsync(IReadOnlyCollection<Guid> valueIds, CancellationToken ct);
}

/// <summary>Owns the "all active employees" base population (mode = All).</summary>
public interface IBasePopulationProvider
{
    Task<ISet<Guid>> ResolveAllAsync(CancellationToken ct);
}
