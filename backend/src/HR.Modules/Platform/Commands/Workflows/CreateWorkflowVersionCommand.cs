using HR.Modules.Platform.DTOs.Workflows;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Workflows;

public record CreateWorkflowVersionCommand : IRequest<WorkflowVersionDto>
{
    public Guid WorkflowDefinitionId { get; init; }
}

public class CreateWorkflowVersionCommandHandler : IRequestHandler<CreateWorkflowVersionCommand, WorkflowVersionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public CreateWorkflowVersionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowVersionDto> Handle(CreateWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.WorkflowDefinitions.FindAsync(new object[] { request.WorkflowDefinitionId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowDefinition", request.WorkflowDefinitionId);

        var maxVersion = await _context.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == request.WorkflowDefinitionId)
            .MaxAsync(v => (int?)v.VersionNumber, cancellationToken) ?? 0;

        var entity = new HR.Domain.Engines.Workflows.WorkflowVersion
        {
            WorkflowDefinitionId = request.WorkflowDefinitionId,
            VersionNumber = maxVersion + 1,
            IsPublished = false
        };

        _context.WorkflowVersions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowVersionDto>(entity);
    }
}
