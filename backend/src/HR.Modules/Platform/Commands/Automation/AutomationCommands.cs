using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.Automation;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Automation;
using MediatR;

namespace HR.Modules.Platform.Commands.Automation;

public record CreateAutomationRuleCommand : IRequest<AutomationRuleDto>
{
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public int Priority { get; init; }
}

public record UpdateAutomationRuleCommand : IRequest<AutomationRuleDto>
{
    public Guid Id { get; init; }
    public string NameEn { get; init; } = null!;
    public string NameAr { get; init; } = null!;
    public bool IsActive { get; init; }
    public int Priority { get; init; }
}

public record DeleteAutomationRuleCommand(Guid Id) : IRequest;

// Handlers

public class CreateAutomationRuleCommandHandler : IRequestHandler<CreateAutomationRuleCommand, AutomationRuleDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public CreateAutomationRuleCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AutomationRuleDto> Handle(CreateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var entity = new AutomationRule
        {
            NameEn = request.NameEn,
            NameAr = request.NameAr,
            Priority = request.Priority
        };

        _context.AutomationRules.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<AutomationRuleDto>(entity);
    }
}

public class UpdateAutomationRuleCommandHandler : IRequestHandler<UpdateAutomationRuleCommand, AutomationRuleDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public UpdateAutomationRuleCommandHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AutomationRuleDto> Handle(UpdateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AutomationRules.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("AutomationRule", request.Id);

        entity.NameEn = request.NameEn;
        entity.NameAr = request.NameAr;
        entity.IsActive = request.IsActive;
        entity.Priority = request.Priority;

        await _context.SaveChangesAsync(cancellationToken);
        return _mapper.Map<AutomationRuleDto>(entity);
    }
}

public class DeleteAutomationRuleCommandHandler : IRequestHandler<DeleteAutomationRuleCommand>
{
    private readonly ApplicationDbContext _context;

    public DeleteAutomationRuleCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AutomationRules.FindAsync(new object[] { request.Id }, cancellationToken)
            ?? throw new NotFoundException("AutomationRule", request.Id);

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
