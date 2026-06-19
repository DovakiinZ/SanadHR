using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.FlowBuilder;
using HR.Infrastructure.Persistence;
using HR.Modules.Workflows.DTOs;
using HR.Modules.Workflows.Execution;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Workflows.Commands;

/// <summary>
/// Shared helper: runs an action that mutates tracked entities inside a single atomic transaction
/// (when the provider is relational) and commits with one SaveChanges. On the in-memory provider
/// used by unit tests it degrades to a plain SaveChanges (which is itself atomic per call).
/// </summary>
internal static class WorkflowTransaction
{
    public static async Task CommitAsync(ApplicationDbContext context, Func<Task> mutate, CancellationToken ct)
    {
        if (context.Database.IsRelational())
        {
            var strategy = context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var tx = await context.Database.BeginTransactionAsync(ct);
                await mutate();
                await context.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);
            });
        }
        else
        {
            await mutate();
            await context.SaveChangesAsync(ct);
        }
    }
}

// ----------------------------------------------------------------------------------------------
// Start a request
// ----------------------------------------------------------------------------------------------

public record StartWorkflowRequestCommand : IRequest<WorkflowRequestDto>
{
    public Guid DefinitionId { get; init; }
    /// <summary>JSON payload (e.g. leave dates). Defaults to an empty object.</summary>
    public string Payload { get; init; } = "{}";
    /// <summary>Optional explicit requester; defaults to the current user.</summary>
    public Guid? RequesterId { get; init; }
}

public class StartWorkflowRequestCommandHandler : IRequestHandler<StartWorkflowRequestCommand, WorkflowRequestDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IWorkflowRunner _runner;

    public StartWorkflowRequestCommandHandler(
        ApplicationDbContext context, ICurrentUserService currentUser, IWorkflowRunner runner)
    {
        _context = context;
        _currentUser = currentUser;
        _runner = runner;
    }

    public async Task<WorkflowRequestDto> Handle(StartWorkflowRequestCommand request, CancellationToken ct)
    {
        var definition = await _context.FlowDefinitions
            .Include(d => d.Steps)
            .FirstOrDefaultAsync(d => d.Id == request.DefinitionId, ct)
            ?? throw new NotFoundException(nameof(WorkflowDefinition), request.DefinitionId);

        if (!definition.IsActive)
            throw new ConflictException("This workflow is not active and cannot accept new requests.");

        var requesterId = request.RequesterId ?? _currentUser.UserId;

        var instance = new WorkflowRequest
        {
            TenantId = definition.TenantId,
            RequestNumber = await GenerateRequestNumberAsync(ct),
            DefinitionId = definition.Id,
            Definition = definition,
            RequesterId = requesterId,
            CurrentStepId = definition.RootStepId,
            Status = WorkflowRequestStatus.Pending,
            Payload = string.IsNullOrWhiteSpace(request.Payload) ? "{}" : request.Payload,
            StartedAt = DateTime.UtcNow
        };

        // Submission event.
        instance.AuditTrail.Add(new WorkflowAuditTrail
        {
            TenantId = definition.TenantId,
            RequestId = instance.Id,
            ToStepId = definition.RootStepId,
            Action = "Submitted",
            Result = "Request submitted",
            ActorId = requesterId,
            OccurredAt = DateTime.UtcNow
        });

        _context.FlowRequests.Add(instance);

        // Auto-advance through any leading non-blocking steps until it parks or completes.
        await WorkflowTransaction.CommitAsync(_context, () => _runner.AdvanceAsync(instance, null, ct), ct);

        return instance.ToDto();
    }

    private async Task<string> GenerateRequestNumberAsync(CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var count = await _context.FlowRequests.CountAsync(ct);
        return $"WF-{year}-{count + 1:D6}";
    }
}

// ----------------------------------------------------------------------------------------------
// Execute the current step (the core command from the spec)
// ----------------------------------------------------------------------------------------------

/// <summary>
/// Applies an approver's decision to the step a request is currently parked on, then lets the engine
/// advance. Guarded so it is idempotent: it only acts when the request is genuinely awaiting a
/// decision on an Approval step; re-sending against an already-advanced/finished request is rejected
/// rather than double-applied.
/// </summary>
public record ExecuteWorkflowStepCommand : IRequest<WorkflowRequestDto>
{
    public Guid RequestId { get; init; }
    public bool Approved { get; init; }
    public string? Comment { get; init; }
}

public class ExecuteWorkflowStepCommandHandler : IRequestHandler<ExecuteWorkflowStepCommand, WorkflowRequestDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IWorkflowRunner _runner;

    public ExecuteWorkflowStepCommandHandler(
        ApplicationDbContext context, ICurrentUserService currentUser, IWorkflowRunner runner)
    {
        _context = context;
        _currentUser = currentUser;
        _runner = runner;
    }

    public async Task<WorkflowRequestDto> Handle(ExecuteWorkflowStepCommand request, CancellationToken ct)
    {
        var instance = await _context.FlowRequests
            .Include(r => r.Definition).ThenInclude(d => d.Steps)
            .Include(r => r.AuditTrail)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, ct)
            ?? throw new NotFoundException(nameof(WorkflowRequest), request.RequestId);

        if (instance.Status != WorkflowRequestStatus.InProgress || instance.CurrentStepId is null)
            throw new ConflictException("This request is not awaiting a decision.");

        var currentStep = instance.Definition.Steps.FirstOrDefault(s => s.Id == instance.CurrentStepId);
        if (currentStep is null || currentStep.Type != WorkflowStepType.Approval)
            throw new ConflictException("The current step is not an approval and cannot be decided.");

        var decision = new WorkflowDecision(request.Approved, request.Comment, _currentUser.UserId);

        await WorkflowTransaction.CommitAsync(_context, () => _runner.AdvanceAsync(instance, decision, ct), ct);

        return instance.ToDto();
    }
}

// ----------------------------------------------------------------------------------------------
// Cancel a request
// ----------------------------------------------------------------------------------------------

public record CancelWorkflowRequestCommand : IRequest<WorkflowRequestDto>
{
    public Guid RequestId { get; init; }
    public string? Comment { get; init; }
}

public class CancelWorkflowRequestCommandHandler : IRequestHandler<CancelWorkflowRequestCommand, WorkflowRequestDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CancelWorkflowRequestCommandHandler(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<WorkflowRequestDto> Handle(CancelWorkflowRequestCommand request, CancellationToken ct)
    {
        var instance = await _context.FlowRequests
            .Include(r => r.Definition).ThenInclude(d => d.Steps)
            .Include(r => r.AuditTrail)
            .FirstOrDefaultAsync(r => r.Id == request.RequestId, ct)
            ?? throw new NotFoundException(nameof(WorkflowRequest), request.RequestId);

        // Idempotent: cancelling an already-finished request is a no-op.
        if (instance.Status is WorkflowRequestStatus.Completed
            or WorkflowRequestStatus.Cancelled
            or WorkflowRequestStatus.Rejected)
            return instance.ToDto();

        instance.Status = WorkflowRequestStatus.Cancelled;
        instance.CurrentStepId = null;
        instance.CompletedAt = DateTime.UtcNow;
        instance.AuditTrail.Add(new WorkflowAuditTrail
        {
            TenantId = instance.TenantId,
            RequestId = instance.Id,
            Action = "Cancelled",
            Result = "Request cancelled",
            ActorId = _currentUser.UserId,
            Comment = request.Comment,
            OccurredAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);
        return instance.ToDto();
    }
}
