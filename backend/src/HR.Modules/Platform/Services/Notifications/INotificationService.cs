namespace HR.Modules.Platform.Services.Notifications;

/// <summary>
/// Central notification engine. Creates the in-app (bell) notification AND queues an email
/// — so modules never hardcode SMTP. Rows are added to the context; the caller saves
/// (keeps the unit of work batched). When SMTP is configured a sender drains the queue.
/// </summary>
public interface INotificationService
{
    Task NotifyAsync(Guid userId, string titleAr, string titleEn, string bodyAr, string bodyEn,
        string category, Guid? entityId, string link, DateTime? dueAt = null, bool email = true, CancellationToken ct = default);
}
