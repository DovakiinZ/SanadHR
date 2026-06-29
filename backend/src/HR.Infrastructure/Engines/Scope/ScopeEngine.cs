using HR.Application.Engines.Scope;

namespace HR.Infrastructure.Engines.Scope;

/// <summary>Aggregates every registered dimension provider into a Key→provider map (last-wins override,
/// same as the completion EffectExecutorRegistry) and resolves a SelectionScope into an employee set.
/// Payroll depends only on IScopeEngine; it never touches Employee columns.</summary>
public sealed class ScopeEngine : IScopeEngine
{
    private readonly Dictionary<string, IScopeDimensionProvider> _providers;
    private readonly IBasePopulationProvider _basePopulation;

    public ScopeEngine(IEnumerable<IScopeDimensionProvider> providers, IBasePopulationProvider basePopulation)
    {
        _providers = new(StringComparer.OrdinalIgnoreCase);
        foreach (var p in providers) _providers[p.DimensionKey] = p; // last wins
        _basePopulation = basePopulation;
    }

    public IReadOnlyList<ScopeDimensionInfo> Dimensions()
    {
        var available = _providers.Values.Select(p => p.Info).ToList();
        var keys = available.Select(i => i.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var disabled = StaticDisabledDimensions.All.Where(d => !keys.Contains(d.Key));
        return available.Concat(disabled).OrderByDescending(d => d.IsAvailable).ThenBy(d => d.NameEn).ToList();
    }

    public async Task<ScopeResolution> ResolveAsync(SelectionScope scope, CancellationToken ct)
    {
        var warnings = new List<string>();

        // 1. Base set.
        ISet<Guid> set = string.Equals(scope.Mode, "All", StringComparison.OrdinalIgnoreCase)
            ? await _basePopulation.ResolveAllAsync(ct)
            : new HashSet<Guid>();

        // 2. Includes: OR within a dimension, AND across dimensions.
        if (!string.Equals(scope.Mode, "All", StringComparison.OrdinalIgnoreCase))
        {
            var first = true;
            foreach (var c in scope.Include)
            {
                if (!_providers.TryGetValue(c.Dimension, out var provider))
                { warnings.Add($"Dimension '{c.Dimension}' is not available and was skipped."); continue; }
                var matched = await provider.ResolveEmployeesAsync(c.ValueIds, ct);
                if (first) { set = new HashSet<Guid>(matched); first = false; }
                else set.IntersectWith(matched);
            }
        }

        // 3. Explicit include ids unioned in.
        foreach (var id in scope.IncludeEmployeeIds) set.Add(id);

        // 4. Excludes (exclude always wins).
        var excluded = new List<ScopeExclusion>();
        foreach (var c in scope.Exclude)
        {
            if (!_providers.TryGetValue(c.Dimension, out var provider))
            { warnings.Add($"Dimension '{c.Dimension}' is not available and was skipped."); continue; }
            foreach (var id in await provider.ResolveEmployeesAsync(c.ValueIds, ct))
                if (set.Remove(id)) excluded.Add(new ScopeExclusion(id, c.Dimension));
        }
        foreach (var id in scope.ExcludeEmployeeIds)
            if (set.Remove(id)) excluded.Add(new ScopeExclusion(id, "EmployeeId"));

        return new ScopeResolution(set.ToList(), excluded, warnings);
    }
}
