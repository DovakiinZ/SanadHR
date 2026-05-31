using AutoMapper;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Permissions;

public record GetPermissionTemplatesQuery : IRequest<PaginatedList<PermissionTemplateDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}

public record GetPermissionTemplateByIdQuery(Guid Id) : IRequest<PermissionTemplateDto>;

// Handlers

public class GetPermissionTemplatesQueryHandler : IRequestHandler<GetPermissionTemplatesQuery, PaginatedList<PermissionTemplateDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPermissionTemplatesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<PermissionTemplateDto>> Handle(GetPermissionTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.PermissionTemplates
            .Include(t => t.Items)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(t => t.NameEn.Contains(request.Search) || t.NameAr.Contains(request.Search));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(t => t.NameEn)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<PermissionTemplateDto>
        {
            Items = _mapper.Map<List<PermissionTemplateDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}

public class GetPermissionTemplateByIdQueryHandler : IRequestHandler<GetPermissionTemplateByIdQuery, PermissionTemplateDto>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetPermissionTemplateByIdQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PermissionTemplateDto> Handle(GetPermissionTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.PermissionTemplates
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new Application.Common.Exceptions.NotFoundException("PermissionTemplate", request.Id);

        return _mapper.Map<PermissionTemplateDto>(entity);
    }
}
