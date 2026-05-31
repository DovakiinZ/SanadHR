using HR.Modules.Platform.DTOs.Workflows;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record AddWorkflowEdgeCommand : IRequest<WorkflowEdgeDto>
{
    public Guid WorkflowVersionId { get; init; }
    public Guid SourceNodeId { get; init; }
    public Guid TargetNodeId { get; init; }
    public string? Condition { get; init; }
    public int SortOrder { get; init; }
}

public class AddWorkflowEdgeCommandHandler : IRequestHandler<AddWorkflowEdgeCommand, WorkflowEdgeDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddWorkflowEdgeCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowEdgeDto> Handle(AddWorkflowEdgeCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.WorkflowVersions.FindAsync(new object[] { request.WorkflowVersionId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowVersion", request.WorkflowVersionId);
        _ = await _context.WorkflowNodes.FindAsync(new object[] { request.SourceNodeId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowNode (Source)", request.SourceNodeId);
        _ = await _context.WorkflowNodes.FindAsync(new object[] { request.TargetNodeId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowNode (Target)", request.TargetNodeId);

        var entity = new HR.Domain.Engines.Workflows.WorkflowEdge
        {
            WorkflowVersionId = request.WorkflowVersionId,
            SourceNodeId = request.SourceNodeId,
            TargetNodeId = request.TargetNodeId,
            Condition = request.Condition,
            SortOrder = request.SortOrder
        };

        _context.WorkflowEdges.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowEdgeDto>(entity);
    }
}
