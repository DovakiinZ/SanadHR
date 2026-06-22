namespace HR.Application.Engines.Completion;

/// <summary>Runtime routing: resolves the executor registered for a given effect type.</summary>
public interface IEffectExecutorRegistry
{
    IEffectExecutor Resolve(string effectType);
    bool TryResolve(string effectType, out IEffectExecutor executor);
}
