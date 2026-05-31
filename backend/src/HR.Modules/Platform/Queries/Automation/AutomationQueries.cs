using AutoMapper;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Automation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Automation;

public record GetAutomationRulesQuery : IRequest<PaginatedList<AutomationRuleDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record GetAutomationRuleByIdQuery(Guid Id) : IRequest<AutomationRuleDto>;

// Handlers

public class GetAutomationRulesQueryHandler : IRequestHandler<GetAutomationRulesQuery, PaginatedList<AutomationRuleDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAutomationRulesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AutomationRuleDto>> Handle(GetAutomationRulesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AutomationRules
            .Include(r => r.Triggers)
            .Include(r => r.Conditions)
            .Include(r => r.Actions.OrderBy(a => a.SortOrder))
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(r => r.Priority)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<AutomationRuleDto>
        {
            Items = _mapper.Map<List<AutomationRuleDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetAutomationRuleByIdQueryHandler : IRequestHandler<GetAutomationRuleByIdQuery, AutomationRuleDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAutomationRuleByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<AutomationRuleDto> Handle(GetAutomationRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.AutomationRules
            .Include(r => r.Triggers)
            .Include(r => r.Conditions)
            .Include(r => r.Actions.OrderBy(a => a.SortOrder))
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new Application.Common.Exceptions.NotFoundException("AutomationRule", request.Id);

        return _mapper.Map<AutomationRuleDto>(entity);
    }
}
