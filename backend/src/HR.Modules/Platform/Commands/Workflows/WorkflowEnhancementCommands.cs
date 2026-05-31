using AutoMapper;
using HR.Domain.Engines.Workflows;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Workflows;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Commands.Workflows;

// === Dynamic Approver Commands ===

public record AddDynamicApproverCommand : IRequest<WorkflowDynamicApproverDto>
{
    public Guid WorkflowNodeId { get; init; }
    public string ApproverStrategy { get; init; } = null!;
    public Guid? SpecificEntityId { get; init; }
    public int ChainLevel { get; init; }
    public string? FallbackStrategy { get; init; }
    public Guid? FallbackEntityId { get; init; }
    public int SortOrder { get; init; }
}

public record UpdateDynamicApproverCommand : IRequest<WorkflowDynamicApproverDto>
{
    public Guid Id { get; init; }
    public string ApproverStrategy { get; init; } = null!;
    public Guid? SpecificEntityId { get; init; }
    public int ChainLevel { get; init; }
    public string? FallbackStrategy { get; init; }
    public Guid? FallbackEntityId { get; init; }
    public int SortOrder { get; init; }
}

public record DeleteDynamicApproverCommand(Guid Id) : IRequest;

// === Dynamic Condition Commands ===

public record AddDynamicConditionCommand : IRequest<WorkflowDynamicConditionDto>
{
    public Guid WorkflowNodeId { get; init; }
    public string ConditionType { get; init; } = null!;
    public string FieldPath { get; init; } = null!;
    public string Operator { get; init; } = null!;
    public string Value { get; init; } = null!;
    public string? LogicalOperator { get; init; }
    public int SortOrder { get; init; }
}

public record UpdateDynamicConditionCommand : IRequest<WorkflowDynamicConditionDto>
{
    public Guid Id { get; init; }
    public string ConditionType { get; init; } = null!;
    public string FieldPath { get; init; } = null!;
    public string Operator { get; init; } = null!;
    public string Value { get; init; } = null!;
    public string? LogicalOperator { get; init; }
    public int SortOrder { get; init; }
}

public record DeleteDynamicConditionCommand(Guid Id) : IRequest;

// === Workflow Action Commands ===

public record AddWorkflowActionCommand : IRequest<WorkflowActionDto>
{
    public Guid WorkflowNodeId { get; init; }
    public string ActionType { get; init; } = null!;
    public string? Configuration { get; init; }
    public int SortOrder { get; init; }
}

public record UpdateWorkflowActionCommand : IRequest<WorkflowActionDto>
{
    public Guid Id { get; init; }
    public string ActionType { get; init; } = null!;
    public string? Configuration { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; }
}

public record DeleteWorkflowActionCommand(Guid Id) : IRequest;

// === Workflow Simulation Commands ===

public record RunWorkflowSimulationCommand : IRequest<WorkflowSimulationDto>
{
    public Guid WorkflowVersionId { get; init; }
    public string InputData { get; init; } = null!;
    public Guid SimulatedById { get; init; }
}

public record DeleteWorkflowSimulationCommand(Guid Id) : IRequest;

// === Handlers ===

public class AddDynamicApproverCommandHandler : IRequestHandler<AddDynamicApproverCommand, WorkflowDynamicApproverDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddDynamicApproverCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowDynamicApproverDto> Handle(AddDynamicApproverCommand request, CancellationToken cancellationToken)
    {
        var entity = new WorkflowDynamicApprover
        {
            WorkflowNodeId = request.WorkflowNodeId,
            ApproverStrategy = request.ApproverStrategy,
            SpecificEntityId = request.SpecificEntityId,
            ChainLevel = request.ChainLevel,
            FallbackStrategy = request.FallbackStrategy,
            FallbackEntityId = request.FallbackEntityId,
            SortOrder = request.SortOrder
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkflowDynamicApproverDto>(entity);
    }
}

public class UpdateDynamicApproverCommandHandler : IRequestHandler<UpdateDynamicApproverCommand, WorkflowDynamicApproverDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateDynamicApproverCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowDynamicApproverDto> Handle(UpdateDynamicApproverCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<WorkflowDynamicApprover>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"WorkflowDynamicApprover with Id '{request.Id}' not found.");

        entity.ApproverStrategy = request.ApproverStrategy;
        entity.SpecificEntityId = request.SpecificEntityId;
        entity.ChainLevel = request.ChainLevel;
        entity.FallbackStrategy = request.FallbackStrategy;
        entity.FallbackEntityId = request.FallbackEntityId;
        entity.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkflowDynamicApproverDto>(entity);
    }
}

public class DeleteDynamicApproverCommandHandler : IRequestHandler<DeleteDynamicApproverCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteDynamicApproverCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteDynamicApproverCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<WorkflowDynamicApprover>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"WorkflowDynamicApprover with Id '{request.Id}' not found.");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class AddDynamicConditionCommandHandler : IRequestHandler<AddDynamicConditionCommand, WorkflowDynamicConditionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddDynamicConditionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowDynamicConditionDto> Handle(AddDynamicConditionCommand request, CancellationToken cancellationToken)
    {
        var entity = new WorkflowDynamicCondition
        {
            WorkflowNodeId = request.WorkflowNodeId,
            ConditionType = request.ConditionType,
            FieldPath = request.FieldPath,
            Operator = request.Operator,
            Value = request.Value,
            LogicalOperator = request.LogicalOperator,
            SortOrder = request.SortOrder
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkflowDynamicConditionDto>(entity);
    }
}

public class UpdateDynamicConditionCommandHandler : IRequestHandler<UpdateDynamicConditionCommand, WorkflowDynamicConditionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateDynamicConditionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowDynamicConditionDto> Handle(UpdateDynamicConditionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<WorkflowDynamicCondition>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"WorkflowDynamicCondition with Id '{request.Id}' not found.");

        entity.ConditionType = request.ConditionType;
        entity.FieldPath = request.FieldPath;
        entity.Operator = request.Operator;
        entity.Value = request.Value;
        entity.LogicalOperator = request.LogicalOperator;
        entity.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkflowDynamicConditionDto>(entity);
    }
}

public class DeleteDynamicConditionCommandHandler : IRequestHandler<DeleteDynamicConditionCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteDynamicConditionCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteDynamicConditionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<WorkflowDynamicCondition>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"WorkflowDynamicCondition with Id '{request.Id}' not found.");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class AddWorkflowActionCommandHandler : IRequestHandler<AddWorkflowActionCommand, WorkflowActionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public AddWorkflowActionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowActionDto> Handle(AddWorkflowActionCommand request, CancellationToken cancellationToken)
    {
        var entity = new WorkflowAction
        {
            WorkflowNodeId = request.WorkflowNodeId,
            ActionType = request.ActionType,
            Configuration = request.Configuration,
            SortOrder = request.SortOrder,
            IsActive = true
        };

        _context.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkflowActionDto>(entity);
    }
}

public class UpdateWorkflowActionCommandHandler : IRequestHandler<UpdateWorkflowActionCommand, WorkflowActionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateWorkflowActionCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowActionDto> Handle(UpdateWorkflowActionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<WorkflowAction>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"WorkflowAction with Id '{request.Id}' not found.");

        entity.ActionType = request.ActionType;
        entity.Configuration = request.Configuration;
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkflowActionDto>(entity);
    }
}

public class DeleteWorkflowActionCommandHandler : IRequestHandler<DeleteWorkflowActionCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteWorkflowActionCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkflowActionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<WorkflowAction>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"WorkflowAction with Id '{request.Id}' not found.");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class RunWorkflowSimulationCommandHandler : IRequestHandler<RunWorkflowSimulationCommand, WorkflowSimulationDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public RunWorkflowSimulationCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<WorkflowSimulationDto> Handle(RunWorkflowSimulationCommand request, CancellationToken cancellationToken)
    {
        var simulation = new WorkflowSimulation
        {
            WorkflowVersionId = request.WorkflowVersionId,
            InputData = request.InputData,
            Result = "{}",  // Will be populated by workflow engine
            SimulatedAt = DateTime.UtcNow,
            SimulatedById = request.SimulatedById
        };

        _context.Add(simulation);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<WorkflowSimulationDto>(simulation);
    }
}

public class DeleteWorkflowSimulationCommandHandler : IRequestHandler<DeleteWorkflowSimulationCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteWorkflowSimulationCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteWorkflowSimulationCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<WorkflowSimulation>().FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new InvalidOperationException($"WorkflowSimulation with Id '{request.Id}' not found.");

        _context.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
