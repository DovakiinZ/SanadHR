using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record DeleteWorkflowDefinitionCommand(Guid Id) : IRequest;

public class DeleteWorkflowDefinitionCommandHandler : IRequestHandler<DeleteWorkflowDefinitionCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteWorkflowDefinitionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowDefinitions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowDefinition", request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
