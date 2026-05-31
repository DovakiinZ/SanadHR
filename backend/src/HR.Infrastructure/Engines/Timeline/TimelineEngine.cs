using System.Text.Json;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Application.Engines.Timeline;
using HR.Domain.Engines.Timeline;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Timeline;

public class TimelineEngine : ITimelineEngine
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public TimelineEngine(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task PublishEvent(string category, string entityType, Guid entityId, string action,
        string? descriptionEn = null, string? descriptionAr = null, object? metadata = null, CancellationToken ct = default)
    {
        var timelineEvent = new TimelineEvent
        {
            Category = category,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            DescriptionEn = descriptionEn,
            DescriptionAr = descriptionAr,
            ActorId = _currentUser.UserId,
            ActorName = _currentUser.Email,
            Metadata = metadata != null ? JsonSerializer.Serialize(metadata) : null,
            OccurredAt = DateTime.UtcNow
        };

        _context.TimelineEvents.Add(timelineEvent);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<PaginatedList<TimelineEvent>> GetTimeline(string entityType, Guid entityId,
        int pageNumber = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _context.TimelineEvents
            .Where(e => e.EntityType == entityType && e.EntityId == entityId)
            .OrderByDescending(e => e.OccurredAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PaginatedList<TimelineEvent>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
