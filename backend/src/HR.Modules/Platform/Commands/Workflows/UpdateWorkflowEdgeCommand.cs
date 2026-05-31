using HR.Modules.Platform.DTOs.Workflows;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record UpdateWorkflowEdgeCommand : IRequest<WorkflowEdgeDto>
{
    public Guid Id { get; init; }
    public string? Condition { get; init; }
    public int SortOrder { get; init; }
}

public class UpdateWorkflowEdgeCommandHandler : IRequestHandler<UpdateWorkflowEdgeCommand, WorkflowEdgeDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateWorkflowEdgeCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowEdgeDto> Handle(UpdateWorkflowEdgeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowEdges.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowEdge", request.Id);

        entity.Condition = request.Condition;
        entity.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowEdgeDto>(entity);
    }
}
