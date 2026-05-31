using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record DeleteWorkflowConditionCommand(Guid Id) : IRequest;

public class DeleteWorkflowConditionCommandHandler : IRequestHandler<DeleteWorkflowConditionCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteWorkflowConditionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkflowConditionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowConditions.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowCondition", request.Id);

        _context.WorkflowConditions.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
