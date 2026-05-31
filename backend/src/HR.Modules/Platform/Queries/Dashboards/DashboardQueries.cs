using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Dashboards;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Dashboards;

public record GetDashboardDefinitionsQuery : IRequest<PaginatedList<DashboardDefinitionDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public record GetDashboardDefinitionByIdQuery(Guid Id) : IRequest<DashboardDefinitionDto>;

// Handlers

public class GetDashboardDefinitionsQueryHandler : IRequestHandler<GetDashboardDefinitionsQuery, PaginatedList<DashboardDefinitionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDashboardDefinitionsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<DashboardDefinitionDto>> Handle(GetDashboardDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.DashboardDefinitions
            .Include(d => d.Widgets.OrderBy(w => w.SortOrder))
                .ThenInclude(w => w.Layout)
            .Include(d => d.Widgets)
                .ThenInclude(w => w.Filters)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(d => d.Code)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<DashboardDefinitionDto>
        {
            Items = _mapper.Map<List<DashboardDefinitionDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetDashboardDefinitionByIdQueryHandler : IRequestHandler<GetDashboardDefinitionByIdQuery, DashboardDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetDashboardDefinitionByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<DashboardDefinitionDto> Handle(GetDashboardDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.DashboardDefinitions
            .Include(d => d.Widgets.OrderBy(w => w.SortOrder))
                .ThenInclude(w => w.Layout)
            .Include(d => d.Widgets)
                .ThenInclude(w => w.Filters)
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("DashboardDefinition", request.Id);

        return _mapper.Map<DashboardDefinitionDto>(entity);
    }
}
