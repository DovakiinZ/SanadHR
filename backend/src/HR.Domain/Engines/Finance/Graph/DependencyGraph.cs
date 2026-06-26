namespace HR.Domain.Engines.Finance.Graph;

/// <summary>Raised when a dependency graph contains a cycle (e.g. rule A reads B which reads A). Carries
/// the participating keys so the conflict can be reported to the author.</summary>
public sealed class DependencyCycleException : Exception
{
    public IReadOnlyList<string> Cycle { get; }

    public DependencyCycleException(IReadOnlyList<string> cycle)
        : base($"Dependency cycle detected: {string.Join(" → ", cycle)}.")
    {
        Cycle = cycle;
    }
}

/// <summary>A generic dependency graph with deterministic topological ordering (Kahn's algorithm). The
/// payroll engine never hardcodes calculation order — it builds this graph from what each rule reads vs
/// writes and derives the order dynamically.</summary>
/// <typeparam name="T">The node payload type.</typeparam>
public sealed class DependencyGraph<T>
{
    private readonly Dictionary<string, T> _nodes = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, HashSet<string>> _dependsOn = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _insertionOrder = new();

    /// <summary>Add a node keyed by <paramref name="key"/>. Adding the same key twice replaces the payload.</summary>
    public void AddNode(string key, T payload)
    {
        if (!_dependsOn.ContainsKey(key))
        {
            _dependsOn[key] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _insertionOrder.Add(key);
        }
        _nodes[key] = payload;
    }

    /// <summary>Declare that <paramref name="key"/> depends on (must run after) <paramref name="dependencyKey"/>.
    /// Dependencies on unknown keys (external facts) are ignored.</summary>
    public void AddDependency(string key, string dependencyKey)
    {
        if (string.Equals(key, dependencyKey, StringComparison.OrdinalIgnoreCase)) return;
        if (!_nodes.ContainsKey(dependencyKey)) return; // external fact, not a node — no edge
        if (!_dependsOn.TryGetValue(key, out var deps))
        {
            deps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _dependsOn[key] = deps;
        }
        deps.Add(dependencyKey);
    }

    /// <summary>Return the node payloads in a valid execution order (dependencies first). Ties are broken
    /// by insertion order for determinism. Throws <see cref="DependencyCycleException"/> if cyclic.</summary>
    public IReadOnlyList<T> TopologicalSort()
    {
        var inDegree = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in _insertionOrder) inDegree[key] = 0;
        foreach (var (key, deps) in _dependsOn)
            foreach (var _ in deps) inDegree[key]++;

        // Seed with zero-in-degree nodes, preserving insertion order.
        var ready = new List<string>(_insertionOrder.Where(k => inDegree[k] == 0));
        var ordered = new List<T>();
        var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        while (ready.Count > 0)
        {
            var key = ready[0];
            ready.RemoveAt(0);
            processed.Add(key);
            ordered.Add(_nodes[key]);

            // Any node that depended on `key` loses an in-edge; if it hits zero, it's ready.
            foreach (var candidate in _insertionOrder)
            {
                if (processed.Contains(candidate) || ready.Contains(candidate)) continue;
                if (_dependsOn[candidate].Contains(key))
                {
                    inDegree[candidate]--;
                    if (inDegree[candidate] == 0) ready.Add(candidate);
                }
            }
        }

        if (ordered.Count != _nodes.Count)
        {
            var remaining = _insertionOrder.Where(k => !processed.Contains(k)).ToList();
            throw new DependencyCycleException(remaining);
        }

        return ordered;
    }

    public IReadOnlyCollection<string> Keys => _nodes.Keys;
}
