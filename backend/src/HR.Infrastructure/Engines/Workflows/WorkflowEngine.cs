using HR.Application.Common.Interfaces;
using HR.Application.Engines.Workflows;
using HR.Domain.Engines.Workflows;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Workflows;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public WorkflowEngine(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<WorkflowInstance> StartWorkflow(string definitionCode, string entityType, Guid entityId, CancellationToken ct = default)
    {
        var definition = await _context.WorkflowDefinitions
            .Include(d => d.Versions.Where(v => v.IsPublished))
                .ThenInclude(v => v.Nodes)
            .FirstOrDefaultAsync(d => d.Code == definitionCode && d.IsActive, ct)
            ?? throw new InvalidOperationException($"Workflow definition '{definitionCode}' not found or inactive.");

        var version = definition.Versions.FirstOrDefault(v => v.IsPublished)
            ?? throw new InvalidOperationException($"No published version for workflow '{definitionCode}'.");

        var instance = new WorkflowInstance
        {
            WorkflowDefinitionId = definition.Id,
            WorkflowVersionId = version.Id,
            EntityType = entityType,
            EntityId = entityId,
            Status = WorkflowStatus.Active,
            StartedAt = DateTime.UtcNow
        };

        // Create first step from Start node
        var startNode = version.Nodes.FirstOrDefault(n => n.NodeType == WorkflowNodeType.Start);
        if (startNode != null)
        {
            instance.Steps.Add(new WorkflowInstanceStep
            {
                WorkflowNodeId = startNode.Id,
                Status = WorkflowStepStatus.Completed,
                ActionTakenAt = DateTime.UtcNow
            });
        }

        _context.WorkflowInstances.Add(instance);
        await _context.SaveChangesAsync(ct);

        return instance;
    }

    public async Task ProcessStep(Guid instanceStepId, WorkflowActionType action, string? comment = null, CancellationToken ct = default)
    {
        var step = await _context.WorkflowInstanceSteps
            .Include(s => s.WorkflowInstance)
            .FirstOrDefaultAsync(s => s.Id == instanceStepId, ct)
            ?? throw new InvalidOperationException("Workflow step not found.");

        step.Status = WorkflowStepStatus.Completed;
        step.ActionType = action;
        step.ActionTakenAt = DateTime.UtcNow;
        step.Comment = comment;

        if (action == WorkflowActionType.Rejected)
        {
            step.WorkflowInstance.Status = WorkflowStatus.Rejected;
            step.WorkflowInstance.CompletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<WorkflowInstanceStep>> GetPendingApprovals(Guid userId, CancellationToken ct = default)
    {
        return await _context.WorkflowInstanceSteps
            .Include(s => s.WorkflowInstance)
            .Include(s => s.WorkflowNode)
            .Where(s => s.AssignedToId == userId && s.Status == WorkflowStepStatus.Pending)
            .OrderByDescending(s => s.WorkflowInstance.StartedAt)
            .ToListAsync(ct);
    }
}
