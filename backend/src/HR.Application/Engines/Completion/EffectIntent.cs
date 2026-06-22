namespace HR.Application.Engines.Completion;

/// <summary>
/// A resolved "intent to change" produced before execution: which effect to run, in what order,
/// and the structured JSON payload it carries. The engine materializes these into CompletionEffect
/// rows and routes each to its executor.
/// </summary>
/// <param name="EffectType">e.g. "Leave.CreateApprovedLeave".</param>
/// <param name="Sequence">1-based execution order within the request.</param>
/// <param name="Payload">JSON object string describing the effect's inputs.</param>
public sealed record EffectIntent(string EffectType, int Sequence, string Payload);
