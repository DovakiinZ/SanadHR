using HR.Modules.Platform.DTOs.Workflows;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record UpdateWorkflowDefinitionCommand : IRequest<WorkflowDefinitionDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string TriggerEntityType { get; init; } = null!;
    public bool IsActive { get; init; }
}

public class UpdateWorkflowDefinitionCommandHandler : IRequestHandler<UpdateWorkflowDefinitionCommand, WorkflowDefinitionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public UpdateWorkflowDefinitionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowDefinitionDto> Handle(UpdateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowDefinition", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.TriggerEntityType = request.TriggerEntityType;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowDefinitionDto>(entity);
    }
}
