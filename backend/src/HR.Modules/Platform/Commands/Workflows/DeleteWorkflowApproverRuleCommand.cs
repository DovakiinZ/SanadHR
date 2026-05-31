using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record DeleteWorkflowApproverRuleCommand(Guid Id) : IRequest;

public class DeleteWorkflowApproverRuleCommandHandler : IRequestHandler<DeleteWorkflowApproverRuleCommand>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public DeleteWorkflowApproverRuleCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkflowApproverRuleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.WorkflowApproverRules.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowApproverRule", request.Id);

        _context.WorkflowApproverRules.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
