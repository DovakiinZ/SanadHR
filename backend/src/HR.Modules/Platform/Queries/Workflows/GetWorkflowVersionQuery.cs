using HR.Modules.Platform.DTOs.Workflows;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Workflows;

public record GetWorkflowVersionQuery(Guid VersionId) : IRequest<WorkflowVersionDetailDto>;

public record GetWorkflowInstanceByIdQuery(Guid InstanceId) : IRequest<WorkflowInstanceDto>;

public class GetWorkflowVersionQueryHandler : IRequestHandler<GetWorkflowVersionQuery, WorkflowVersionDetailDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public GetWorkflowVersionQueryHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowVersionDetailDto> Handle(GetWorkflowVersionQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowVersions
            .Include(v => v.Nodes)
                .ThenInclude(n => n.Conditions)
            .Include(v => v.Nodes)
                .ThenInclude(n => n.ApproverRules)
            .Include(v => v.Edges)
            .FirstOrDefaultAsync(v => v.Id == request.VersionId, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowVersion", request.VersionId);

        return _mapper.Map<WorkflowVersionDetailDto>(entity);
    }
}

public class GetWorkflowInstanceByIdQueryHandler : IRequestHandler<GetWorkflowInstanceByIdQuery, WorkflowInstanceDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public GetWorkflowInstanceByIdQueryHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowInstanceDto> Handle(GetWorkflowInstanceByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowInstances
            .Include(i => i.Steps)
            .FirstOrDefaultAsync(i => i.Id == request.InstanceId, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowInstance", request.InstanceId);

        return _mapper.Map<WorkflowInstanceDto>(entity);
    }
}
