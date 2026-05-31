using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Workflows;

public record DeleteWorkflowNodeCommand(Guid Id) : IRequest;

public class DeleteWorkflowNodeCommandHandler : IRequestHandler<DeleteWorkflowNodeCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteWorkflowNodeCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkflowNodeCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowNodes.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowNode", request.Id);

        // Remove connected edges
        var connectedEdges = await _context.WorkflowEdges
            .Where(e => e.SourceNodeId == request.Id || e.TargetNodeId == request.Id)
            .ToListAsync(cancellationToken);

        _context.WorkflowEdges.RemoveRange(connectedEdges);
        _context.WorkflowNodes.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
