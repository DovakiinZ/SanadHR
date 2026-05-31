using HR.Modules.Platform.DTOs.Workflows;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record AddWorkflowConditionCommand : IRequest<WorkflowConditionDto>
{
    public Guid WorkflowNodeId { get; init; }
    public string Field { get; init; } = null!;
    public string Operator { get; init; } = null!;
    public string Value { get; init; } = null!;
    public string? LogicalOperator { get; init; }
}

public class AddWorkflowConditionCommandHandler : IRequestHandler<AddWorkflowConditionCommand, WorkflowConditionDto>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;
    private readonly AutoMapper.IMapper _mapper;

    public AddWorkflowConditionCommandHandler(HR.Infrastructure.Persistence.ApplicationDbContext context, AutoMapper.IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowConditionDto> Handle(AddWorkflowConditionCommand request, CancellationToken cancellationToken)
    {
        _ = await _context.WorkflowNodes.FindAsync(new object[] { request.WorkflowNodeId }, cancellationToken)
            ?? throw new HR.Application.Common.Exceptions.NotFoundException("WorkflowNode", request.WorkflowNodeId);

        var entity = new HR.Domain.Engines.Workflows.WorkflowCondition
        {
            WorkflowNodeId = request.WorkflowNodeId,
            Field = request.Field,
            Operator = request.Operator,
            Value = request.Value,
            LogicalOperator = request.LogicalOperator
        };

        _context.WorkflowConditions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkflowConditionDto>(entity);
    }
}
