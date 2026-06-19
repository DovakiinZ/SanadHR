using FluentValidation.Results;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.FlowBuilder;
using HR.Infrastructure.Persistence;
using HR.Modules.Workflows.DTOs;
using HR.Modules.Workflows.Execution;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Workflows.Commands;

// ----------------------------------------------------------------------------------------------
// Create
// ----------------------------------------------------------------------------------------------

public record CreateWorkflowDefinitionCommand : IRequest<WorkflowDefinitionDto>
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
}

public class CreateWorkflowDefinitionCommandHandler
    : IRequestHandler<CreateWorkflowDefinitionCommand, WorkflowDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateWorkflowDefinitionCommandHandler(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<WorkflowDefinitionDto> Handle(CreateWorkflowDefinitionCommand request, CancellationToken ct)
    {
        var code = request.Code.Trim();
        if (string.IsNullOrWhiteSpace(code))
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.Code), "Code is required.") });

        var exists = await _context.FlowDefinitions.AnyAsync(d => d.Code == code, ct);
        if (exists)
            throw new ConflictException($"A workflow with code '{code}' already exists.");

        var definition = new WorkflowDefinition
        {
            TenantId = _currentUser.TenantId,
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description,
            Version = 1,
            IsActive = true
        };

        _context.FlowDefinitions.Add(definition);
        await _context.SaveChangesAsync(ct);

        return definition.ToDto();
    }
}

// ----------------------------------------------------------------------------------------------
// Update (saves the whole step graph in one shot; validates before persisting)
// ----------------------------------------------------------------------------------------------

public record UpdateWorkflowDefinitionCommand : IRequest<WorkflowDefinitionDto>
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public Guid? RootStepId { get; init; }
    public List<WorkflowStepInput> Steps { get; init; } = new();
}

public class UpdateWorkflowDefinitionCommandHandler
    : IRequestHandler<UpdateWorkflowDefinitionCommand, WorkflowDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IWorkflowGraphValidator _validator;

    public UpdateWorkflowDefinitionCommandHandler(ApplicationDbContext context, IWorkflowGraphValidator validator)
    {
        _context = context;
        _validator = validator;
    }

    public async Task<WorkflowDefinitionDto> Handle(UpdateWorkflowDefinitionCommand request, CancellationToken ct)
    {
        var definition = await _context.FlowDefinitions
            .Include(d => d.Steps)
            .FirstOrDefaultAsync(d => d.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(WorkflowDefinition), request.Id);

        // Validate the proposed graph first — never persist an invalid (e.g. cyclic) workflow.
        var nodes = request.Steps
            .Select(s => new WorkflowGraphNode(s.Id, s.Type, s.Name, s.NextStepIdSuccess, s.NextStepIdFailure))
            .ToList();
        var validation = _validator.Validate(request.RootStepId, nodes);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors.Select(e => new ValidationFailure("graph", e)));

        var incomingIds = request.Steps.Select(s => s.Id).ToHashSet();

        // Remove steps that are no longer present.
        var toRemove = definition.Steps.Where(s => !incomingIds.Contains(s.Id)).ToList();
        if (toRemove.Count > 0)
            _context.FlowSteps.RemoveRange(toRemove);

        var existing = definition.Steps.ToDictionary(s => s.Id);
        foreach (var input in request.Steps)
        {
            if (existing.TryGetValue(input.Id, out var step))
            {
                step.Type = input.Type;
                step.Name = input.Name;
                step.Config = string.IsNullOrWhiteSpace(input.Config) ? "{}" : input.Config;
                step.NextStepIdSuccess = input.NextStepIdSuccess;
                step.NextStepIdFailure = input.NextStepIdFailure;
                step.SortOrder = input.SortOrder;
            }
            else
            {
                _context.FlowSteps.Add(new WorkflowStep
                {
                    Id = input.Id, // preserve client id so success/failure pointers stay valid
                    TenantId = definition.TenantId,
                    DefinitionId = definition.Id,
                    Type = input.Type,
                    Name = input.Name,
                    Config = string.IsNullOrWhiteSpace(input.Config) ? "{}" : input.Config,
                    NextStepIdSuccess = input.NextStepIdSuccess,
                    NextStepIdFailure = input.NextStepIdFailure,
                    SortOrder = input.SortOrder
                });
            }
        }

        definition.Name = request.Name.Trim();
        definition.Description = request.Description;
        definition.IsActive = request.IsActive;
        definition.RootStepId = request.RootStepId;
        definition.Version += 1;

        await _context.SaveChangesAsync(ct);

        // Reload graph for an accurate DTO (covers adds/removes within the tracked context).
        var saved = await _context.FlowDefinitions
            .Include(d => d.Steps)
            .FirstAsync(d => d.Id == definition.Id, ct);
        return saved.ToDto();
    }
}

// ----------------------------------------------------------------------------------------------
// Delete (soft delete)
// ----------------------------------------------------------------------------------------------

public record DeleteWorkflowDefinitionCommand(Guid Id) : IRequest<Unit>;

public class DeleteWorkflowDefinitionCommandHandler : IRequestHandler<DeleteWorkflowDefinitionCommand, Unit>
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public DeleteWorkflowDefinitionCommandHandler(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(DeleteWorkflowDefinitionCommand request, CancellationToken ct)
    {
        var definition = await _context.FlowDefinitions
            .FirstOrDefaultAsync(d => d.Id == request.Id, ct)
            ?? throw new NotFoundException(nameof(WorkflowDefinition), request.Id);

        definition.IsDeleted = true;
        definition.DeletedAt = DateTime.UtcNow;
        definition.DeletedBy = _currentUser.Email;
        definition.IsActive = false;

        await _context.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
