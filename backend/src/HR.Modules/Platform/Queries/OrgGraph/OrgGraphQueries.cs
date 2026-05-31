using AutoMapper;
using HR.Application.Common.Exceptions;
using HR.Domain.Engines.OrgGraph;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.OrgGraph;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.OrgGraph;

public record GetOrgGraphTreeQuery : IRequest<OrgGraphTreeDto>
{
    public string? GraphType { get; init; }
}

public record GetOrgNodeByIdQuery(Guid Id) : IRequest<OrgNodeDto>;

public record GetOrgGraphLayoutsQuery : IRequest<List<OrgGraphLayoutDto>>;

public record GetEmployeeReportingLinesQuery : IRequest<List<EmployeeReportingLineDto>>
{
    public Guid? EmployeeId { get; init; }
}

public record GetSubordinatesQuery(Guid ManagerId) : IRequest<List<EmployeeReportingLineDto>>;

// --- Handlers ---

public class GetOrgGraphTreeQueryHandler : IRequestHandler<GetOrgGraphTreeQuery, OrgGraphTreeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrgGraphTreeQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgGraphTreeDto> Handle(GetOrgGraphTreeQuery request, CancellationToken cancellationToken)
    {
        var nodesQuery = _context.Set<OrgNode>()
            .AsNoTracking()
            .Include(n => n.ChildNodes)
            .Where(n => n.IsActive);

        if (!string.IsNullOrEmpty(request.GraphType))
        {
            nodesQuery = nodesQuery.Where(n => n.NodeType == request.GraphType);
        }

        var nodes = await nodesQuery
            .OrderBy(n => n.Level)
            .ThenBy(n => n.SortOrder)
            .ToListAsync(cancellationToken);

        var edges = await _context.Set<OrgEdge>()
            .AsNoTracking()
            .Where(e => e.IsActive)
            .ToListAsync(cancellationToken);

        return new OrgGraphTreeDto
        {
            Nodes = _mapper.Map<List<OrgNodeDto>>(nodes),
            Edges = _mapper.Map<List<OrgEdgeDto>>(edges)
        };
    }
}

public class GetOrgNodeByIdQueryHandler : IRequestHandler<GetOrgNodeByIdQuery, OrgNodeDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrgNodeByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<OrgNodeDto> Handle(GetOrgNodeByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.Set<OrgNode>()
            .AsNoTracking()
            .Include(n => n.ChildNodes)
            .FirstOrDefaultAsync(n => n.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("OrgNode", request.Id);

        return _mapper.Map<OrgNodeDto>(entity);
    }
}

public class GetOrgGraphLayoutsQueryHandler : IRequestHandler<GetOrgGraphLayoutsQuery, List<OrgGraphLayoutDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetOrgGraphLayoutsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<OrgGraphLayoutDto>> Handle(GetOrgGraphLayoutsQuery request, CancellationToken cancellationToken)
    {
        var layouts = await _context.Set<OrgGraphLayout>()
            .AsNoTracking()
            .OrderBy(l => l.NameEn)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<OrgGraphLayoutDto>>(layouts);
    }
}

public class GetEmployeeReportingLinesQueryHandler : IRequestHandler<GetEmployeeReportingLinesQuery, List<EmployeeReportingLineDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetEmployeeReportingLinesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<EmployeeReportingLineDto>> Handle(GetEmployeeReportingLinesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Set<EmployeeReportingLine>()
            .AsNoTracking()
            .Where(r => r.IsActive);

        if (request.EmployeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);
        }

        var lines = await query
            .OrderBy(r => r.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<EmployeeReportingLineDto>>(lines);
    }
}

public class GetSubordinatesQueryHandler : IRequestHandler<GetSubordinatesQuery, List<EmployeeReportingLineDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetSubordinatesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<EmployeeReportingLineDto>> Handle(GetSubordinatesQuery request, CancellationToken cancellationToken)
    {
        var lines = await _context.Set<EmployeeReportingLine>()
            .AsNoTracking()
            .Where(r => r.ManagerId == request.ManagerId && r.IsActive)
            .OrderBy(r => r.ReportingType)
            .ThenBy(r => r.EffectiveFrom)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<EmployeeReportingLineDto>>(lines);
    }
}
