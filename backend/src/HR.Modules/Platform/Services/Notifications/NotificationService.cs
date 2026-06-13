using HR.Application.Common.Interfaces;
using HR.Domain.Engines.Notifications;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Notifications;

public sealed class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;

    public NotificationService(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    public async Task NotifyAsync(Guid userId, string titleAr, string titleEn, string bodyAr, string bodyEn,
        string category, Guid? entityId, string link, DateTime? dueAt = null, bool email = true, CancellationToken ct = default)
    {
        // 1) In-app (bell) notification
        _db.Notifications.Add(new Notification
        {
            UserId = userId, TitleAr = titleAr, TitleEn = titleEn, BodyAr = bodyAr, BodyEn = bodyEn,
            Category = category, EntityId = entityId, Link = link, IsRead = false,
        });

        // 2) Email — queued, never sent inline (decoupled from request code)
        if (email)
        {
            var to = await _db.Users.Where(u => u.Id == userId).Select(u => u.Email).FirstOrDefaultAsync(ct);
            if (!string.IsNullOrWhiteSpace(to))
            {
                var bodyText = bodyAr + (dueAt is { } d ? $"\nموعد الاستحقاق: {d:yyyy-MM-dd HH:mm}" : "") + $"\n{link}";
                _db.EmailQueue.Add(new EmailNotificationQueue
                {
                    UserId = userId, ToEmail = to!, Subject = titleAr, Body = bodyText,
                    Category = category, EntityId = entityId, Link = link, DueAt = dueAt,
                    Status = HR.Domain.Enums.EmailQueueStatus.Pending,
                });
            }
        }
    }
}
