using HR.Domain.Enums;
using HR.Modules.Platform.DTOs.Workflows;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record SetWorkflowApproverRuleCommand : IRequest<WorkflowApproverRuleDto>
{
    public Guid WorkflowNodeId { get; init; }
    public ApproverType ApproverType { get; init; }
    public Guid? SpecificUserId { get; init; }
    public Guid? SpecificRoleId { get; init; }
}

public class SetWorkflowApproverRuleCommandHandler : IRequestHandler<SetWorkflowApproverRuleCommand, WorkflowApproverRuleDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public SetWorkflowApproverRuleCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowApproverRuleDto> Handle(SetWorkflowApproverRuleCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.WorkflowNodes.FindAsync(new object[] { request.WorkflowNodeId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowNode", request.WorkflowNodeId);

        var entity = new HR.Domain.Engines.Workflows.WorkflowApproverRule
        {
            WorkflowNodeId = request.WorkflowNodeId,
            ApproverType = request.ApproverType,
            SpecificUserId = request.SpecificUserId,
            SpecificRoleId = request.SpecificRoleId
        };

        _context.WorkflowApproverRules.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowApproverRuleDto>(entity);
    }
}
