namespace HR.Application.Engines.Completion;

/// <summary>
/// Builds an effect-type → executor map from every <see cref="IEffectExecutor"/> registered in DI.
/// Because executors are discovered from the container, any module (current or future) that
/// registers an executor is wired in automatically — no central edit required.
/// </summary>
public sealed class EffectExecutorRegistry : IEffectExecutorRegistry
{
    private readonly IReadOnlyDictionary<string, IEffectExecutor> _byType;

    public EffectExecutorRegistry(IEnumerable<IEffectExecutor> executors)
    {
        var map = new Dictionary<string, IEffectExecutor>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in executors)
        {
            // Last registration wins for a duplicate type (lets a module override another's effect).
            map[e.EffectType] = e;
        }
        _byType = map;
    }

    public IEffectExecutor Resolve(string effectType)
        => _byType.TryGetValue(effectType, out var e)
            ? e
            : throw new InvalidOperationException(
                $"No effect executor is registered for effect type '{effectType}'. " +
                "Register an IEffectExecutor in the owning module.");

    public bool TryResolve(string effectType, out IEffectExecutor executor)
    {
        if (_byType.TryGetValue(effectType, out var e)) { executor = e; return true; }
        executor = null!;
        return false;
    }
}
