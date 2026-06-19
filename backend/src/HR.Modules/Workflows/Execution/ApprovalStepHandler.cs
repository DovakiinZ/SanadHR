using HR.Domain.Engines.FlowBuilder;

namespace HR.Modules.Workflows.Execution;

/// <summary>
/// Handles <see cref="WorkflowStepType.Approval"/> steps. The step is blocking: with no decision
/// it parks the request (Halt). When a decision arrives it routes to the Success branch on approve
/// and the Failure branch on reject.
/// </summary>
public class ApprovalStepHandler : IWorkflowStepHandler
{
    public WorkflowStepType StepType => WorkflowStepType.Approval;

    public Task<StepExecutionResult> ExecuteAsync(StepExecutionContext context, CancellationToken ct)
    {
        var decision = context.Decision;

        if (decision is null)
            return Task.FromResult(StepExecutionResult.Wait("Pending"));

        var result = decision.Approved
            ? StepExecutionResult.Continue(WorkflowBranch.Success, "Approved", decision.Comment)
            : StepExecutionResult.Continue(WorkflowBranch.Failure, "Rejected", decision.Comment);

        return Task.FromResult(result);
    }
}
