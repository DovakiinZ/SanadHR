using AutoMapper;
using HR.Domain.Engines.MasterData;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.MasterData;
using HR.Modules.Platform.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.MasterData;

// ─── Queries ─────────────────────────────────────────────────────────────────

public record GetMasterDataItemsQuery : IRequest<List<MasterDataItemDto>>
{
    public string ObjectType { get; init; } = null!;
    public string? Search { get; init; }
    public bool IncludeInactive { get; init; }
}

public record GetMasterDataItemByIdQuery(Guid Id) : IRequest<MasterDataItemDto>;

public record GetLookupItemsQuery(string ObjectType, bool ActiveOnly = true) : IRequest<List<LookupItemDto>>;

public record GetMasterDataObjectTypesQuery : IRequest<List<MasterDataObjectTypeDto>>;

public record GetMasterDataUsageQuery(Guid Id) : IRequest<MasterDataUsageDto>;

// ─── Handlers ────────────────────────────────────────────────────────────────

public class GetMasterDataItemsQueryHandler : IRequestHandler<GetMasterDataItemsQuery, List<MasterDataItemDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMasterDataItemsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MasterDataItemDto>> Handle(GetMasterDataItemsQuery request, CancellationToken cancellationToken)
    {
        var canonical = MasterDataObjectType.Normalize(request.ObjectType)
                        ?? MasterDataObjectType.FromSlug(request.ObjectType);
        if (canonical is null)
            return new List<MasterDataItemDto>();

        var query = _context.MasterDataItems
            .AsNoTracking()
            .Where(x => x.ObjectType == canonical);

        if (!request.IncludeInactive)
            query = query.Where(x => x.IsActive);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            query = query.Where(x =>
                x.NameEn.Contains(term) || x.NameAr.Contains(term) || x.Code.Contains(term));
        }

        var entities = await query
            .OrderBy(x => x.SortOrder).ThenBy(x => x.NameEn)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<MasterDataItemDto>>(entities);
    }
}

public class GetMasterDataItemByIdQueryHandler : IRequestHandler<GetMasterDataItemByIdQuery, MasterDataItemDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMasterDataItemByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MasterDataItemDto> Handle(GetMasterDataItemByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.MasterDataItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Master data item not found");

        return _mapper.Map<MasterDataItemDto>(entity);
    }
}

public class GetLookupItemsQueryHandler : IRequestHandler<GetLookupItemsQuery, List<LookupItemDto>>
{
    private readonly ILookupService _lookup;

    public GetLookupItemsQueryHandler(ILookupService lookup)
    {
        _lookup = lookup;
    }

    public Task<List<LookupItemDto>> Handle(GetLookupItemsQuery request, CancellationToken cancellationToken)
        => _lookup.GetLookupAsync(request.ObjectType, request.ActiveOnly, cancellationToken);
}

public class GetMasterDataObjectTypesQueryHandler : IRequestHandler<GetMasterDataObjectTypesQuery, List<MasterDataObjectTypeDto>>
{
    private readonly ApplicationDbContext _context;

    public GetMasterDataObjectTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<MasterDataObjectTypeDto>> Handle(GetMasterDataObjectTypesQuery request, CancellationToken cancellationToken)
    {
        var counts = await _context.MasterDataItems
            .AsNoTracking()
            .GroupBy(x => x.ObjectType)
            .Select(g => new { ObjectType = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var countMap = counts.ToDictionary(c => c.ObjectType, c => c.Count);

        return MasterDataObjectType.All
            .Select(t => new MasterDataObjectTypeDto
            {
                ObjectType = t,
                Slug = MasterDataObjectType.ToSlug(t),
                Count = countMap.TryGetValue(t, out var c) ? c : 0
            })
            .ToList();
    }
}

public class GetMasterDataUsageQueryHandler : IRequestHandler<GetMasterDataUsageQuery, MasterDataUsageDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IUsageTrackingService _usage;

    public GetMasterDataUsageQueryHandler(ApplicationDbContext context, IUsageTrackingService usage)
    {
        _context = context;
        _usage = usage;
    }

    public async Task<MasterDataUsageDto> Handle(GetMasterDataUsageQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.MasterDataItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
            ?? throw new InvalidOperationException("Master data item not found");

        return await _usage.GetUsageAsync(entity.ObjectType, entity.Id, cancellationToken);
    }
}
