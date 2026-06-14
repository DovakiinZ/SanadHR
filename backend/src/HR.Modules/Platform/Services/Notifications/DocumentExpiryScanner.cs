using HR.Domain.Engines.Notifications;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Notifications;

/// <summary>
/// Scans employee documents against active <see cref="NotificationRule"/>s of event
/// <c>DocumentExpiry</c> and creates notifications for the configured recipients when a document is
/// within the rule's window. Tenant-agnostic (bypasses query filters and scopes by each row's
/// TenantId) so it works both from the background service (no current user) and an admin trigger.
/// A <see cref="NotificationDispatch"/> ledger dedupes, so re-runs never double-notify.
/// </summary>
public interface IDocumentExpiryScanner
{
    Task<int> RunAsync(CancellationToken ct);
}

public sealed class DocumentExpiryScanner : IDocumentExpiryScanner
{
    private readonly ApplicationDbContext _db;
    public DocumentExpiryScanner(ApplicationDbContext db) { _db = db; }

    private static readonly Dictionary<string, string> TypeLabels = new()
    {
        ["Id"] = "الهوية الوطنية", ["Iqama"] = "الإقامة", ["Passport"] = "جواز السفر",
        ["Contract"] = "عقد العمل", ["Certificate"] = "شهادة", ["MedicalReport"] = "تقرير طبي",
        ["Custom"] = "مستند",
    };

    public async Task<int> RunAsync(CancellationToken ct)
    {
        var today = DateTime.UtcNow.Date;
        var rules = await _db.NotificationRules.IgnoreQueryFilters()
            .Where(r => r.IsActive && !r.IsDeleted && r.Event == "DocumentExpiry")
            .ToListAsync(ct);
        if (rules.Count == 0) return 0;

        int created = 0;
        foreach (var rule in rules)
        {
            var windowEnd = today.AddDays(rule.DaysBefore);
            var q = _db.EmployeeDocuments.IgnoreQueryFilters()
                .Where(d => !d.IsDeleted && d.TenantId == rule.TenantId
                    && d.ExpiryDate != null && d.ExpiryDate >= today && d.ExpiryDate <= windowEnd);
            if (!string.IsNullOrWhiteSpace(rule.DocumentType))
                q = q.Where(d => d.Type == rule.DocumentType);
            var docs = await q.ToListAsync(ct);

            foreach (var doc in docs)
            {
                var recipients = await ResolveRecipientsAsync(rule, doc, ct);
                foreach (var userId in recipients)
                {
                    if (userId == Guid.Empty) continue;
                    var already = await _db.NotificationDispatches.IgnoreQueryFilters()
                        .AnyAsync(x => x.RuleId == rule.Id && x.SourceEntityId == doc.Id && x.UserId == userId, ct);
                    if (already) continue;

                    var daysLeft = (doc.ExpiryDate!.Value.Date - today).Days;
                    var label = TypeLabels.GetValueOrDefault(doc.Type, "مستند");
                    var titleAr = "تنبيه: قرب انتهاء مستند";
                    var bodyAr = $"{label} ({doc.Title}) ينتهي خلال {daysLeft} يوم — بتاريخ {doc.ExpiryDate:yyyy-MM-dd}.";
                    var bodyEn = $"{doc.Title} expires in {daysLeft} day(s) on {doc.ExpiryDate:yyyy-MM-dd}.";
                    var link = $"/employees/{doc.EmployeeId}";

                    if (rule.ChannelBell)
                    {
                        _db.Notifications.Add(new Notification
                        {
                            TenantId = rule.TenantId, UserId = userId,
                            TitleAr = titleAr, TitleEn = "Document Expiry",
                            BodyAr = bodyAr, BodyEn = bodyEn,
                            Category = "DocumentExpiry", EntityId = doc.Id, Link = link, IsRead = false,
                        });
                    }
                    if (rule.ChannelEmail)
                    {
                        var to = await _db.Users.IgnoreQueryFilters().Where(u => u.Id == userId)
                            .Select(u => u.Email).FirstOrDefaultAsync(ct);
                        if (!string.IsNullOrWhiteSpace(to))
                            _db.EmailQueue.Add(new EmailNotificationQueue
                            {
                                TenantId = rule.TenantId, UserId = userId, ToEmail = to!,
                                Subject = titleAr, Body = bodyAr + "\n" + link,
                                Category = "DocumentExpiry", EntityId = doc.Id, Link = link,
                                Status = HR.Domain.Enums.EmailQueueStatus.Pending,
                            });
                    }
                    // SMS is captured on the rule for the future; no provider wired yet.

                    _db.NotificationDispatches.Add(new NotificationDispatch
                    {
                        TenantId = rule.TenantId, RuleId = rule.Id, SourceEntityId = doc.Id, UserId = userId,
                    });
                    created++;
                }
            }
        }
        if (created > 0) await _db.SaveChangesAsync(ct);
        return created;
    }

    private async Task<HashSet<Guid>> ResolveRecipientsAsync(NotificationRule rule, HR.Domain.Engines.Documents.EmployeeDocument doc, CancellationToken ct)
    {
        var set = new HashSet<Guid>();
        var emp = await _db.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == doc.EmployeeId, ct);
        if (emp is null) return set;

        async Task AddEmployeeUser(Guid? employeeId)
        {
            if (employeeId is not { } id) return;
            var uid = await _db.Employees.IgnoreQueryFilters().Where(e => e.Id == id)
                .Select(e => e.UserId).FirstOrDefaultAsync(ct);
            if (uid is { } u && u != Guid.Empty) set.Add(u);
        }

        if (rule.NotifyEmployee && emp.UserId is { } eu && eu != Guid.Empty) set.Add(eu);
        if (rule.NotifyDirectManager) await AddEmployeeUser(emp.ManagerId);
        if (rule.NotifyDepartmentManager && emp.DepartmentId is { } did)
        {
            var mgrId = await _db.Departments.IgnoreQueryFilters().Where(d => d.Id == did)
                .Select(d => d.ManagerId).FirstOrDefaultAsync(ct);
            await AddEmployeeUser(mgrId);
        }
        if (rule.ExtraEmployeeId is { } extra) await AddEmployeeUser(extra);
        if (rule.RoleId is { } roleId)
        {
            var users = await _db.UserRoles.IgnoreQueryFilters().Where(ur => ur.RoleId == roleId)
                .Select(ur => ur.UserId).ToListAsync(ct);
            foreach (var u in users) if (u != Guid.Empty) set.Add(u);
        }
        return set;
    }
}
