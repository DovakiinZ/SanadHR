using AutoMapper;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Dashboards;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Dashboards;

public record GetDashboardsQuery : IRequest<PaginatedList<DashboardDefinitionDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Scope { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Search { get; init; }
}

public record GetDashboardByIdQuery(Guid Id) : IRequest<DashboardDefinitionDto>;
public record GetDashboardCategoriesQuery : IRequest<List<DashboardCategoryDto>>;
public record GetDashboardTemplatesQuery : IRequest<List<DashboardTemplateDto>>;
public record GetWidgetDefinitionsQuery : IRequest<List<WidgetDefinitionDto>>;
public record GetMyDashboardsQuery(Guid UserId) : IRequest<List<DashboardDefinitionDto>>;

public class GetDashboardsQueryHandler : IRequestHandler<GetDashboardsQuery, PaginatedList<DashboardDefinitionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetDashboardsQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<PaginatedList<DashboardDefinitionDto>> Handle(GetDashboardsQuery request, CancellationToken ct)
    {
        var query = _context.DashboardDefinitions.Include(d => d.Widgets).ThenInclude(w => w.Layout).Include(d => d.Widgets).ThenInclude(w => w.Filters).Include(d => d.Category).AsQueryable();
        if (!string.IsNullOrEmpty(request.Search)) query = query.Where(d => d.NameEn.Contains(request.Search) || d.NameAr.Contains(request.Search));
        if (request.CategoryId.HasValue) query = query.Where(d => d.CategoryId == request.CategoryId);
        var totalCount = await query.CountAsync(ct);
        var items = await query.OrderBy(d => d.SortOrder).Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize).ToListAsync(ct);
        return new PaginatedList<DashboardDefinitionDto> { Items = _mapper.Map<List<DashboardDefinitionDto>>(items), PageNumber = request.PageNumber, PageSize = request.PageSize, TotalCount = totalCount };
    }
}

public class GetDashboardByIdQueryHandler : IRequestHandler<GetDashboardByIdQuery, DashboardDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetDashboardByIdQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<DashboardDefinitionDto> Handle(GetDashboardByIdQuery request, CancellationToken ct)
    {
        var entity = await _context.DashboardDefinitions.Include(d => d.Category).Include(d => d.Widgets).ThenInclude(w => w.Layout).Include(d => d.Widgets).ThenInclude(w => w.Filters).Include(d => d.Widgets).ThenInclude(w => w.Drilldowns).Include(d => d.Shares).FirstOrDefaultAsync(d => d.Id == request.Id, ct) ?? throw new HR.Application.Common.Exceptions.NotFoundException("DashboardDefinition", request.Id);
        return _mapper.Map<DashboardDefinitionDto>(entity);
    }
}

public class GetDashboardCategoriesQueryHandler : IRequestHandler<GetDashboardCategoriesQuery, List<DashboardCategoryDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetDashboardCategoriesQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<List<DashboardCategoryDto>> Handle(GetDashboardCategoriesQuery request, CancellationToken ct)
    {
        var items = await _context.DashboardCategories.OrderBy(c => c.SortOrder).ToListAsync(ct);
        return _mapper.Map<List<DashboardCategoryDto>>(items);
    }
}

public class GetDashboardTemplatesQueryHandler : IRequestHandler<GetDashboardTemplatesQuery, List<DashboardTemplateDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetDashboardTemplatesQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<List<DashboardTemplateDto>> Handle(GetDashboardTemplatesQuery request, CancellationToken ct)
    {
        var items = await _context.DashboardTemplates.Where(t => t.IsActive).OrderBy(t => t.SortOrder).ToListAsync(ct);
        return _mapper.Map<List<DashboardTemplateDto>>(items);
    }
}

public class GetWidgetDefinitionsQueryHandler : IRequestHandler<GetWidgetDefinitionsQuery, List<WidgetDefinitionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetWidgetDefinitionsQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<List<WidgetDefinitionDto>> Handle(GetWidgetDefinitionsQuery request, CancellationToken ct)
    {
        var items = await _context.WidgetDefinitions.Include(w => w.DataSources).Where(w => w.IsActive).OrderBy(w => w.SortOrder).ToListAsync(ct);
        return _mapper.Map<List<WidgetDefinitionDto>>(items);
    }
}

public class GetMyDashboardsQueryHandler : IRequestHandler<GetMyDashboardsQuery, List<DashboardDefinitionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetMyDashboardsQueryHandler(ApplicationDbContext context, IMapper mapper) { _context = context; _mapper = mapper; }

    public async Task<List<DashboardDefinitionDto>> Handle(GetMyDashboardsQuery request, CancellationToken ct)
    {
        var items = await _context.DashboardDefinitions.Include(d => d.Widgets).ThenInclude(w => w.Layout).Where(d => d.OwnerId == request.UserId || d.Shares.Any(s => s.SharedWithUserId == request.UserId)).OrderBy(d => d.SortOrder).ToListAsync(ct);
        return _mapper.Map<List<DashboardDefinitionDto>>(items);
    }
}
