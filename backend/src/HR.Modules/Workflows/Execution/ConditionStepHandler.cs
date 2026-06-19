using System.Globalization;
using System.Text.Json;
using HR.Domain.Engines.FlowBuilder;

namespace HR.Modules.Workflows.Execution;

/// <summary>
/// Handles <see cref="WorkflowStepType.Condition"/> steps. Non-blocking: evaluates a predicate
/// (field / operator / value) against the request payload and routes to the Success branch when the
/// predicate is true, otherwise the Failure branch. Supported operators:
/// eq, neq, gt, gte, lt, lte, contains.
/// </summary>
public class ConditionStepHandler : IWorkflowStepHandler
{
    public WorkflowStepType StepType => WorkflowStepType.Condition;

    public Task<StepExecutionResult> ExecuteAsync(StepExecutionContext context, CancellationToken ct)
    {
        ConditionConfig cfg;
        try
        {
            cfg = JsonSerializer.Deserialize<ConditionConfig>(context.Step.Config,
                      new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? new ConditionConfig();
        }
        catch (JsonException)
        {
            cfg = new ConditionConfig();
        }

        var matched = Evaluate(cfg, context.Payload);
        var branch = matched ? WorkflowBranch.Success : WorkflowBranch.Failure;
        return Task.FromResult(StepExecutionResult.Continue(
            branch, "ConditionEvaluated", $"{cfg.Field} {cfg.Operator} {cfg.Value} => {matched}"));
    }

    private static bool Evaluate(ConditionConfig cfg, JsonElement payload)
    {
        if (string.IsNullOrWhiteSpace(cfg.Field) || payload.ValueKind != JsonValueKind.Object)
            return false;
        if (!payload.TryGetProperty(cfg.Field, out var actualEl))
            return false;

        var actual = actualEl.ValueKind == JsonValueKind.String
            ? actualEl.GetString() ?? string.Empty
            : actualEl.ToString();
        var expected = cfg.Value ?? string.Empty;

        var op = (cfg.Operator ?? "eq").ToLowerInvariant();

        // Numeric comparison when both sides parse as numbers; otherwise string/ordinal.
        var bothNumeric = double.TryParse(actual, NumberStyles.Any, CultureInfo.InvariantCulture, out var a)
                          & double.TryParse(expected, NumberStyles.Any, CultureInfo.InvariantCulture, out var b);

        return op switch
        {
            "eq" => bothNumeric ? a == b : string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "neq" => bothNumeric ? a != b : !string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase),
            "gt" => bothNumeric && a > b,
            "gte" => bothNumeric && a >= b,
            "lt" => bothNumeric && a < b,
            "lte" => bothNumeric && a <= b,
            "contains" => actual.Contains(expected, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private sealed class ConditionConfig
    {
        public string? Field { get; set; }
        public string? Operator { get; set; }
        public string? Value { get; set; }
    }
}
