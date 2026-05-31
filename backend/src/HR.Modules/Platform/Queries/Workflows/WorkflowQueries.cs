using AutoMapper;
using HR.Application.Common.Models;
using HR.Application.Engines.Workflows;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Workflows;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Workflows;

public record GetWorkflowDefinitionsQuery : IRequest<PaginatedList<WorkflowDefinitionDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record GetWorkflowInstancesQuery : IRequest<PaginatedList<WorkflowInstanceDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? EntityType { get; init; }
}

public record GetPendingApprovalsQuery(Guid UserId) : IRequest<List<WorkflowInstanceStepDto>>;

// Handlers

public class GetWorkflowDefinitionsQueryHandler : IRequestHandler<GetWorkflowDefinitionsQuery, PaginatedList<WorkflowDefinitionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWorkflowDefinitionsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<WorkflowDefinitionDto>> Handle(GetWorkflowDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkflowDefinitions.Include(d => d.Versions).AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(d => d.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<WorkflowDefinitionDto>
        {
            Items = _mapper.Map<List<WorkflowDefinitionDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetWorkflowInstancesQueryHandler : IRequestHandler<GetWorkflowInstancesQuery, PaginatedList<WorkflowInstanceDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWorkflowInstancesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<WorkflowInstanceDto>> Handle(GetWorkflowInstancesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.WorkflowInstances
            .Include(i => i.Steps)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(i => i.EntityType == request.EntityType);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(i => i.StartedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<WorkflowInstanceDto>
        {
            Items = _mapper.Map<List<WorkflowInstanceDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, List<WorkflowInstanceStepDto>>
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IMapper _mapper;

    public GetPendingApprovalsQueryHandler(IWorkflowEngine workflowEngine, IMapper mapper)
    {
        _workflowEngine = workflowEngine;
        _mapper = mapper;
    }

    public async Task<List<WorkflowInstanceStepDto>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var steps = await _workflowEngine.GetPendingApprovals(request.UserId, cancellationToken);
        return _mapper.Map<List<WorkflowInstanceStepDto>>(steps);
    }
}
