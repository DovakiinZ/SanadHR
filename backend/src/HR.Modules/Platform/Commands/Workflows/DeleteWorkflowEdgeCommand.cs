using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record DeleteWorkflowEdgeCommand(Guid Id) : IRequest;

public class DeleteWorkflowEdgeCommandHandler : IRequestHandler<DeleteWorkflowEdgeCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteWorkflowEdgeCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkflowEdgeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowEdges.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowEdge", request.Id);

        _context.WorkflowEdges.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
