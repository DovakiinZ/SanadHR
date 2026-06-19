using System.Text.Json;
using HR.Domain.Engines.FlowBuilder;

namespace HR.Modules.Workflows.Execution;

/// <summary>
/// Handles <see cref="WorkflowStepType.Action"/> steps whose config declares an e-mail action.
/// Non-blocking: it performs the side effect then routes to the Success branch. Tokens of the form
/// <c>{{field}}</c> in the subject/body are substituted from the request payload.
/// </summary>
public class EmailActionHandler : IWorkflowStepHandler
{
    private readonly IWorkflowEmailSender _email;

    public EmailActionHandler(IWorkflowEmailSender email) => _email = email;

    public WorkflowStepType StepType => WorkflowStepType.Action;

    public async Task<StepExecutionResult> ExecuteAsync(StepExecutionContext context, CancellationToken ct)
    {
        ActionConfig cfg;
        try
        {
            cfg = JsonSerializer.Deserialize<ActionConfig>(context.Step.Config,
                      new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                  ?? new ActionConfig();
        }
        catch (JsonException)
        {
            cfg = new ActionConfig();
        }

        // Only the e-mail action type is implemented today; other action types still advance
        // (so the graph never deadlocks) but are recorded as skipped.
        if (!string.Equals(cfg.ActionType, "email", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(cfg.ToEmail))
        {
            return StepExecutionResult.Continue(WorkflowBranch.Success, "ActionExecuted", "No e-mail action configured; skipped");
        }

        var subject = Substitute(cfg.Subject ?? "Workflow notification", context.Payload);
        var body = Substitute(cfg.Body ?? string.Empty, context.Payload);

        await _email.SendAsync(cfg.ToEmail!, subject, body, context.Request.Id, ct);

        return StepExecutionResult.Continue(WorkflowBranch.Success, "ActionExecuted", $"E-mail queued to {cfg.ToEmail}");
    }

    private static string Substitute(string template, JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object || string.IsNullOrEmpty(template))
            return template;

        foreach (var prop in payload.EnumerateObject())
        {
            var token = "{{" + prop.Name + "}}";
            if (template.Contains(token, StringComparison.OrdinalIgnoreCase))
                template = template.Replace(token, prop.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        return template;
    }

    private sealed class ActionConfig
    {
        public string? ActionType { get; set; }
        public string? ToEmail { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
    }
}
