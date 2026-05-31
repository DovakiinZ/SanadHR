using AutoMapper;
using HR.Domain.Engines.Workflows;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Workflows;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Workflows;

// === Query Records ===

public record GetDynamicApproversByNodeQuery(Guid WorkflowNodeId) : IRequest<List<WorkflowDynamicApproverDto>>;

public record GetDynamicConditionsByNodeQuery(Guid WorkflowNodeId) : IRequest<List<WorkflowDynamicConditionDto>>;

public record GetWorkflowActionsByNodeQuery(Guid WorkflowNodeId) : IRequest<List<WorkflowActionDto>>;

public record GetWorkflowSimulationsQuery(Guid WorkflowVersionId) : IRequest<List<WorkflowSimulationDto>>;

public record GetWorkflowSimulationByIdQuery(Guid Id) : IRequest<WorkflowSimulationDto>;

// === Handlers ===

public class GetDynamicApproversByNodeQueryHandler : IRequestHandler<GetDynamicApproversByNodeQuery, List<WorkflowDynamicApproverDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDynamicApproversByNodeQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<WorkflowDynamicApproverDto>> Handle(GetDynamicApproversByNodeQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.Set<WorkflowDynamicApprover>()
            .AsNoTracking()
            .Where(x => x.WorkflowNodeId == request.WorkflowNodeId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<WorkflowDynamicApproverDto>>(entities);
    }
}

public class GetDynamicConditionsByNodeQueryHandler : IRequestHandler<GetDynamicConditionsByNodeQuery, List<WorkflowDynamicConditionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDynamicConditionsByNodeQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<WorkflowDynamicConditionDto>> Handle(GetDynamicConditionsByNodeQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.Set<WorkflowDynamicCondition>()
            .AsNoTracking()
            .Where(x => x.WorkflowNodeId == request.WorkflowNodeId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<WorkflowDynamicConditionDto>>(entities);
    }
}

public class GetWorkflowActionsByNodeQueryHandler : IRequestHandler<GetWorkflowActionsByNodeQuery, List<WorkflowActionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWorkflowActionsByNodeQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<WorkflowActionDto>> Handle(GetWorkflowActionsByNodeQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.Set<WorkflowAction>()
            .AsNoTracking()
            .Where(x => x.WorkflowNodeId == request.WorkflowNodeId)
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<WorkflowActionDto>>(entities);
    }
}

public class GetWorkflowSimulationsQueryHandler : IRequestHandler<GetWorkflowSimulationsQuery, List<WorkflowSimulationDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWorkflowSimulationsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<WorkflowSimulationDto>> Handle(GetWorkflowSimulationsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.Set<WorkflowSimulation>()
            .AsNoTracking()
            .Where(x => x.WorkflowVersionId == request.WorkflowVersionId)
            .OrderByDescending(x => x.SimulatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<WorkflowSimulationDto>>(entities);
    }
}

public class GetWorkflowSimulationByIdQueryHandler : IRequestHandler<GetWorkflowSimulationByIdQuery, WorkflowSimulationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWorkflowSimulationByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowSimulationDto> Handle(GetWorkflowSimulationByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<WorkflowSimulation>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"WorkflowSimulation with Id '{request.Id}' not found.");

        return _mapper.Map<WorkflowSimulationDto>(entity);
    }
}
