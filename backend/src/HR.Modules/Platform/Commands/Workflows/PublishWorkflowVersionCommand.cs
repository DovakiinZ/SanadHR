using HR.Modules.Platform.DTOs.Workflows;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Workflows;

public record PublishWorkflowVersionCommand(Guid VersionId) : IRequest<WorkflowVersionDto>;

public class PublishWorkflowVersionCommandHandler : IRequestHandler<PublishWorkflowVersionCommand, WorkflowVersionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public PublishWorkflowVersionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowVersionDto> Handle(PublishWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowVersions.FindAsync(new object[] { request.VersionId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowVersion", request.VersionId);

        // Unpublish all other versions of the same workflow
        var otherVersions = await _context.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == entity.WorkflowDefinitionId && v.Id != entity.Id && v.IsPublished)
            .ToListAsync(cancellationToken);

        foreach (var v in otherVersions)
        {
            v.IsPublished = false;
        }

        entity.IsPublished = true;
        entity.PublishedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowVersionDto>(entity);
    }
}
