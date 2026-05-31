using HR.Domain.Engines.Workflows;
using HR.Domain.Enums;

namespace HR.Application.Engines.Workflows;

public interface IWorkflowEngine
{
    Task<WorkflowInstance> StartWorkflow(string definitionCode, string entityType, Guid entityId, CancellationToken ct = default);
    Task ProcessStep(Guid instanceStepId, WorkflowActionType action, string? comment = null, CancellationToken ct = default);
    Task<List<WorkflowInstanceStep>> GetPendingApprovals(Guid userId, CancellationToken ct = default);
}
