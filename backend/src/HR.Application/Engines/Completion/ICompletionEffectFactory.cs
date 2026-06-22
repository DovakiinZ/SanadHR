namespace HR.Application.Engines.Completion;

/// <summary>
/// Resolves the ordered list of completion effects ("intents to change") for an approved request.
/// Kept separate from the engine so the source of intents can evolve (today: derived from the
/// request type's declarative impact mapping) without touching orchestration.
/// </summary>
public interface ICompletionEffectFactory
{
    Task<IReadOnlyList<EffectIntent>> BuildAsync(Guid requestInstanceId, CancellationToken ct);
}
