using AutoMapper;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.DTOs.Timeline;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Timeline;

public record GetTimelineQuery : IRequest<PaginatedList<TimelineEventDto>>
{
    public string EntityType { get; init; } = null!;
    public Guid EntityId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

// Handler

public class GetTimelineQueryHandler : IRequestHandler<GetTimelineQuery, PaginatedList<TimelineEventDto>>
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetTimelineQueryHandler(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<TimelineEventDto>> Handle(GetTimelineQuery request, CancellationToken cancellationToken)
    {
        var query = _context.TimelineEvents
            .Where(e => e.EntityType == request.EntityType && e.EntityId == request.EntityId)
            .OrderByDescending(e => e.OccurredAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedList<TimelineEventDto>
        {
            Items = _mapper.Map<List<TimelineEventDto>>(items),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
