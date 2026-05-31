using AutoMapper;
using HR.Domain.Engines.CompanyConfig;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.CompanyConfig;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.CompanyConfig;

// ─── Queries ─────────────────────────────────────────────────────────────────

public record GetCompanyProfileQuery : IRequest<CompanyProfileDto?>;

public record GetPositionsQuery : IRequest<List<PositionDto>>
{
    public Guid? DepartmentId { get; init; }
}

public record GetPositionByIdQuery(Guid Id) : IRequest<PositionDto>;

public record GetGradesQuery : IRequest<List<GradeDto>>;

public record GetCostCentersQuery : IRequest<List<CostCenterDto>>;

public record GetCostCenterByIdQuery(Guid Id) : IRequest<CostCenterDto>;

public record GetCalendarSettingsQuery : IRequest<List<CalendarSettingDto>>;

public record GetFiscalPeriodsQuery : IRequest<List<FiscalPeriodDto>>
{
    public int? Year { get; init; }
}

// ─── Handlers ────────────────────────────────────────────────────────────────

public class GetCompanyProfileQueryHandler : IRequestHandler<GetCompanyProfileQuery, CompanyProfileDto?>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCompanyProfileQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CompanyProfileDto?> Handle(GetCompanyProfileQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<CompanyProfile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        return entity is null ? null : _mapper.Map<CompanyProfileDto>(entity);
    }
}

public class GetPositionsQueryHandler : IRequestHandler<GetPositionsQuery, List<PositionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPositionsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PositionDto>> Handle(GetPositionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<Position>()
            .AsNoTracking()
            .Include(p => p.ChildPositions)
            .AsQueryable();

        if (request.DepartmentId.HasValue)
            query = query.Where(p => p.DepartmentId == request.DepartmentId.Value);

        var entities = await query.OrderBy(p => p.SortOrder).ToListAsync(cancellationToken);
        return _mapper.Map<List<PositionDto>>(entities);
    }
}

public class GetPositionByIdQueryHandler : IRequestHandler<GetPositionByIdQuery, PositionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPositionByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PositionDto> Handle(GetPositionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<Position>()
            .AsNoTracking()
            .Include(p => p.ChildPositions)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Position not found");

        return _mapper.Map<PositionDto>(entity);
    }
}

public class GetGradesQueryHandler : IRequestHandler<GetGradesQuery, List<GradeDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetGradesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<GradeDto>> Handle(GetGradesQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.Set<Grade>()
            .AsNoTracking()
            .OrderBy(g => g.Level)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<GradeDto>>(entities);
    }
}

public class GetCostCentersQueryHandler : IRequestHandler<GetCostCentersQuery, List<CostCenterDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCostCentersQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CostCenterDto>> Handle(GetCostCentersQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.Set<CostCenter>()
            .AsNoTracking()
            .Include(c => c.ChildCostCenters)
            .OrderBy(c => c.SortOrder)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<CostCenterDto>>(entities);
    }
}

public class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, CostCenterDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCostCenterByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<CostCenterDto> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<CostCenter>()
            .AsNoTracking()
            .Include(c => c.ChildCostCenters)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Cost center not found");

        return _mapper.Map<CostCenterDto>(entity);
    }
}

public class GetCalendarSettingsQueryHandler : IRequestHandler<GetCalendarSettingsQuery, List<CalendarSettingDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCalendarSettingsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CalendarSettingDto>> Handle(GetCalendarSettingsQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.Set<CalendarSetting>()
            .AsNoTracking()
            .OrderBy(c => c.Code)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<CalendarSettingDto>>(entities);
    }
}

public class GetFiscalPeriodsQueryHandler : IRequestHandler<GetFiscalPeriodsQuery, List<FiscalPeriodDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetFiscalPeriodsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<FiscalPeriodDto>> Handle(GetFiscalPeriodsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<FiscalPeriod>()
            .AsNoTracking()
            .AsQueryable();

        if (request.Year.HasValue)
            query = query.Where(f => f.Year == request.Year.Value);

        var entities = await query.OrderBy(f => f.PeriodNumber).ToListAsync(cancellationToken);
        return _mapper.Map<List<FiscalPeriodDto>>(entities);
    }
}
