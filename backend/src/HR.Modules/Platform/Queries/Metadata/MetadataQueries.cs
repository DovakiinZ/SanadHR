using AutoMapper;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Metadata;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Metadata;

public record GetMetadataDefinitionsQuery : IRequest<PaginatedList<MetadataDefinitionDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
    public string? Module { get; init; }
}

public record GetMetadataDefinitionByIdQuery(Guid Id) : IRequest<MetadataDefinitionDto>;

public record GetMetadataValuesQuery(string EntityType, Guid EntityId) : IRequest<List<MetadataValueDto>>;

// Handlers

public class GetMetadataDefinitionsQueryHandler : IRequestHandler<GetMetadataDefinitionsQuery, PaginatedList<MetadataDefinitionDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMetadataDefinitionsQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MetadataDefinitionDto>> Handle(GetMetadataDefinitionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.MetadataDefinitions
            .Include(d => d.Fields.OrderBy(f => f.SortOrder))
                .ThenInclude(f => f.Options.OrderBy(o => o.SortOrder))
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(d => d.NameEn.Contains(request.Search) || d.NameAr.Contains(request.Search) || d.Code.Contains(request.Search));

        if (!string.IsNullOrEmpty(request.Module))
            query = query.Where(d => d.Module == request.Module);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(d => d.SortOrder)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<MetadataDefinitionDto>
        {
            Items = _mapper.Map<List<MetadataDefinitionDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetMetadataDefinitionByIdQueryHandler : IRequestHandler<GetMetadataDefinitionByIdQuery, MetadataDefinitionDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMetadataDefinitionByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<MetadataDefinitionDto> Handle(GetMetadataDefinitionByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.MetadataDefinitions
            .Include(d => d.Fields.OrderBy(f => f.SortOrder))
                .ThenInclude(f => f.Options.OrderBy(o => o.SortOrder))
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new Application.Common.Exceptions.NotFoundException("MetadataDefinition", request.Id);

        return _mapper.Map<MetadataDefinitionDto>(entity);
    }
}

public class GetMetadataValuesQueryHandler : IRequestHandler<GetMetadataValuesQuery, List<MetadataValueDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMetadataValuesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MetadataValueDto>> Handle(GetMetadataValuesQuery request, CancellationToken cancellationToken)
    {
        var values = await _context.MetadataValues
            .Where(v => v.EntityType == request.EntityType && v.EntityId == request.EntityId)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<MetadataValueDto>>(values);
    }
}
