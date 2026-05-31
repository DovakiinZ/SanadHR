using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Workflows;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record AddWorkflowNodeCommand : IRequest<WorkflowNodeDto>
{
    public Guid WorkflowVersionId { get; init; }
    public WorkflowNodeType NodeType { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Configuration { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
}

public class AddWorkflowNodeCommandHandler : IRequestHandler<AddWorkflowNodeCommand, WorkflowNodeDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddWorkflowNodeCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowNodeDto> Handle(AddWorkflowNodeCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.WorkflowVersions.FindAsync(new object[] { request.WorkflowVersionId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowVersion", request.WorkflowVersionId);

        var entity = new HR.Domain.Engines.Workflows.WorkflowNode
        {
            WorkflowVersionId = request.WorkflowVersionId,
            NodeType = request.NodeType,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Configuration = request.Configuration,
            PositionX = request.PositionX,
            PositionY = request.PositionY
        };

        _context.WorkflowNodes.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowNodeDto>(entity);
    }
}
