using HR.Domain.Engines.Notifications;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;

namespace HR.Modules.Workflows.Execution;

/// <summary>
/// Default <see cref="IWorkflowEmailSender"/>: enqueues onto <see cref="EmailNotificationQueue"/>.
/// A separate sender drains the queue when SMTP is configured (no hardcoded SMTP here).
/// </summary>
public class QueueWorkflowEmailSender : IWorkflowEmailSender
{
    private readonly ApplicationDbContext _context;

    public QueueWorkflowEmailSender(ApplicationDbContext context) => _context = context;

    public Task SendAsync(string toEmail, string subject, string body, Guid? relatedRequestId, CancellationToken ct)
    {
        _context.EmailQueue.Add(new EmailNotificationQueue
        {
            ToEmail = toEmail,
            Subject = subject,
            Body = body,
            Category = "Workflow",
            EntityId = relatedRequestId,
            Status = EmailQueueStatus.Pending
        });
        // Persisted together with the request transition by the engine's single SaveChanges.
        return Task.CompletedTask;
    }
}
