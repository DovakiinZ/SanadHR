using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Application.Engines.Workflows;
using HR.Domain.Engines.Workflows;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Workflows;
using MediatR;

namespace HR.Modules.Platform.Commands.Workflows;

public record CreateWorkflowDefinitionCommand : IRequest<WorkflowDefinitionDto>
{
    public string Code { get; init; } = null!;
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public string TriggerEntityType { get; init; } = null!;
}

public record StartWorkflowCommand : IRequest<WorkflowInstanceDto>
{
    public string DefinitionCode { get; init; } = null!;
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
}

public record ProcessWorkflowStepCommand : IRequest
{
    public Guid InstanceStepId { get; init; }
    public WorkflowActionType Action { get; init; }
    public string? Comment { get; init; }
}

// Handlers

public class CreateWorkflowDefinitionCommandHandler : IRequestHandler<CreateWorkflowDefinitionCommand, WorkflowDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateWorkflowDefinitionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowDefinitionDto> Handle(CreateWorkflowDefinitionCommand request, CancellationToken cancellationToken)
    {
        var entity = new WorkflowDefinition
        {
            Code = request.Code,
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            TriggerEntityType = request.TriggerEntityType
        };

        _context.WorkflowDefinitions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkflowDefinitionDto>(entity);
    }
}

public class StartWorkflowCommandHandler : IRequestHandler<StartWorkflowCommand, WorkflowInstanceDto>
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly IMapper _mapper;

    public StartWorkflowCommandHandler(IWorkflowEngine workflowEngine, IMapper mapper)
    {
        _workflowEngine = workflowEngine;
        _mapper = mapper;
    }

    public async Task<WorkflowInstanceDto> Handle(StartWorkflowCommand request, CancellationToken cancellationToken)
    {
        var instance = await _workflowEngine.StartWorkflow(request.DefinitionCode, request.EntityType, request.EntityId, cancellationToken);
        return _mapper.Map<WorkflowInstanceDto>(instance);
    }
}

public class ProcessWorkflowStepCommandHandler : IRequestHandler<ProcessWorkflowStepCommand>
{
    private readonly IWorkflowEngine _workflowEngine;

    public ProcessWorkflowStepCommandHandler(IWorkflowEngine workflowEngine)
    {
        _workflowEngine = workflowEngine;
    }

    public async Task Handle(ProcessWorkflowStepCommand request, CancellationToken cancellationToken)
    {
        await _workflowEngine.ProcessStep(request.InstanceStepId, request.Action, request.Comment, cancellationToken);
    }
}
