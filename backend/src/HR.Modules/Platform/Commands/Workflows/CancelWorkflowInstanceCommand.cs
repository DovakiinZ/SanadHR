using HR.Domain.Enums;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record CancelWorkflowInstanceCommand(Guid InstanceId) : IRequest;

public class CancelWorkflowInstanceCommandHandler : IRequestHandler<CancelWorkflowInstanceCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public CancelWorkflowInstanceCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelWorkflowInstanceCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowInstances.FindAsync(new object[] { request.InstanceId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowInstance", request.InstanceId);

        if (entity.Status != WorkflowStatus.Active)
            throw new InvalidOperationException("Only active workflow instances can be cancelled");

        entity.Status = WorkflowStatus.Cancelled;
        entity.CompletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
