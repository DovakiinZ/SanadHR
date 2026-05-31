using AutoMapper;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Audit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Audit;

public record GetAuditEntriesQuery : IRequest<PaginatedList<AuditEntryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public string? Action { get; init; }
    public Guid? UserId { get; init; }
}

// Handler

public class GetAuditEntriesQueryHandler : IRequestHandler<GetAuditEntriesQuery, PaginatedList<AuditEntryDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetAuditEntriesQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<AuditEntryDto>> Handle(GetAuditEntriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditEntries.AsQueryable();

        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(a => a.EntityType == request.EntityType);

        if (request.EntityId.HasValue)
            query = query.Where(a => a.EntityId == request.EntityId);

        if (!string.IsNullOrEmpty(request.Action))
            query = query.Where(a => a.Action == request.Action);

        if (request.UserId.HasValue)
            query = query.Where(a => a.UserId == request.UserId);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(a => a.Timestamp)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<AuditEntryDto>
        {
            Items = _mapper.Map<List<AuditEntryDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
