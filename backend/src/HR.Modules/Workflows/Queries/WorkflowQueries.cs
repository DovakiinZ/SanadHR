using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Engines.FlowBuilder;
using HR.Infrastructure.Persistence;
using HR.Modules.Workflows.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Workflows.Queries;

// ----------------------------------------------------------------------------------------------
// Definitions
// ----------------------------------------------------------------------------------------------

public record GetWorkflowDefinitionsQuery : IRequest<List<WorkflowDefinitionSummaryDto>>
{
    public bool? IsActive { get; init; }
}

public class GetWorkflowDefinitionsQueryHandler
    : IRequestHandler<GetWorkflowDefinitionsQuery, List<WorkflowDefinitionSummaryDto>>
{
    private readonly ApplicationDbContext _context;
    public GetWorkflowDefinitionsQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<List<WorkflowDefinitionSummaryDto>> Handle(GetWorkflowDefinitionsQuery request, CancellationToken ct)
    {
        var query = _context.FlowDefinitions.AsNoTracking().AsQueryable();
        if (request.IsActive is not null)
            query = query.Where(d => d.IsActive == request.IsActive);

        return await query
            .OrderBy(d => d.Name)
            .Select(d => new WorkflowDefinitionSummaryDto
            {
                Id = d.Id,
                Code = d.Code,
                Name = d.Name,
                Version = d.Version,
                IsActive = d.IsActive,
                StepCount = d.Steps.Count,
                RequestCount = d.Requests.Count
            })
            .ToListAsync(ct);
    }
}

public record GetWorkflowDefinitionByIdQuery(Guid Id) : IRequest<WorkflowDefinitionDto>;

public class GetWorkflowDefinitionByIdQueryHandler
    : IRequestHandler<GetWorkflowDefinitionByIdQuery, WorkflowDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    public GetWorkflowDefinitionByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<WorkflowDefinitionDto> Handle(GetWorkflowDefinitionByIdQuery request, CancellationToken ct)
    {
        var definition = await _context.FlowDefinitions
            .AsNoTracking()
            .Include(d => d.Steps)
            .FirstOrDefaultAsync(d => d.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(WorkflowDefinition), request.Id);

        return definition.ToDto();
    }
}

// ----------------------------------------------------------------------------------------------
// Requests
// ----------------------------------------------------------------------------------------------

public record GetWorkflowRequestsQuery : IRequest<PaginatedList<WorkflowRequestDto>>
{
    public WorkflowRequestStatus? Status { get; init; }
    public Guid? DefinitionId { get; init; }
    public Guid? RequesterId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 25;
}

public class GetWorkflowRequestsQueryHandler
    : IRequestHandler<GetWorkflowRequestsQuery, PaginatedList<WorkflowRequestDto>>
{
    private readonly ApplicationDbContext _context;
    public GetWorkflowRequestsQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<PaginatedList<WorkflowRequestDto>> Handle(GetWorkflowRequestsQuery request, CancellationToken ct)
    {
        var query = _context.FlowRequests
            .AsNoTracking()
            .Include(r => r.Definition).ThenInclude(d => d.Steps)
            .Include(r => r.AuditTrail)
            .AsQueryable();

        if (request.Status is not null) query = query.Where(r => r.Status == request.Status);
        if (request.DefinitionId is not null) query = query.Where(r => r.DefinitionId == request.DefinitionId);
        if (request.RequesterId is not null) query = query.Where(r => r.RequesterId == request.RequesterId);

        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var pageNumber = Math.Max(1, request.PageNumber);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.StartedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedList<WorkflowRequestDto>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}

public record GetWorkflowRequestByIdQuery(Guid Id) : IRequest<WorkflowRequestDto>;

public class GetWorkflowRequestByIdQueryHandler : IRequestHandler<GetWorkflowRequestByIdQuery, WorkflowRequestDto>
{
    private readonly ApplicationDbContext _context;
    public GetWorkflowRequestByIdQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<WorkflowRequestDto> Handle(GetWorkflowRequestByIdQuery request, CancellationToken ct)
    {
        var instance = await _context.FlowRequests
            .AsNoTracking()
            .Include(r => r.Definition).ThenInclude(d => d.Steps)
            .Include(r => r.AuditTrail)
            .FirstOrDefaultAsync(r => r.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(WorkflowRequest), request.Id);

        return instance.ToDto();
    }
}

/// <summary>Requests currently parked on a blocking step — the approver's queue.</summary>
public record GetPendingWorkflowApprovalsQuery : IRequest<List<WorkflowRequestDto>>;

public class GetPendingWorkflowApprovalsQueryHandler
    : IRequestHandler<GetPendingWorkflowApprovalsQuery, List<WorkflowRequestDto>>
{
    private readonly ApplicationDbContext _context;
    public GetPendingWorkflowApprovalsQueryHandler(ApplicationDbContext context) => _context = context;

    public async Task<List<WorkflowRequestDto>> Handle(GetPendingWorkflowApprovalsQuery request, CancellationToken ct)
    {
        var items = await _context.FlowRequests
            .AsNoTracking()
            .Include(r => r.Definition).ThenInclude(d => d.Steps)
            .Include(r => r.AuditTrail)
            .Where(r => r.Status == WorkflowRequestStatus.InProgress && r.CurrentStepId != null)
            .OrderByDescending(r => r.StartedAt)
            .ToListAsync(ct);

        return items.Select(r => r.ToDto()).ToList();
    }
}
