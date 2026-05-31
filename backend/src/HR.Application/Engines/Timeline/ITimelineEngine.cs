using HR.Application.Common.Models;
using HR.Domain.Engines.Timeline;

namespace HR.Application.Engines.Timeline;

public interface ITimelineEngine
{
    Task PublishEvent(string category, string entityType, Guid entityId, string action,
        string? descriptionEn = null, string? descriptionAr = null, object? metadata = null, CancellationToken ct = default);

    Task<PaginatedList<TimelineEvent>> GetTimeline(string entityType, Guid entityId,
        int pageNumber = 1, int pageSize = 20, CancellationToken ct = default);
}
