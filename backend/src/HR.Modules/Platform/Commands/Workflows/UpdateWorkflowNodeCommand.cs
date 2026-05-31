using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Workflows;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record UpdateWorkflowNodeCommand : IRequest<WorkflowNodeDto>
{
    public Guid Id { get; init; }
    public WorkflowNodeType NodeType { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string? Configuration { get; init; }
    public int PositionX { get; init; }
    public int PositionY { get; init; }
}

public class UpdateWorkflowNodeCommandHandler : IRequestHandler<UpdateWorkflowNodeCommand, WorkflowNodeDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateWorkflowNodeCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowNodeDto> Handle(UpdateWorkflowNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowNodes.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowNode", request.Id);

        entity.NodeType = request.NodeType;
        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.Configuration = request.Configuration;
        entity.PositionX = request.PositionX;
        entity.PositionY = request.PositionY;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowNodeDto>(entity);
    }
}
