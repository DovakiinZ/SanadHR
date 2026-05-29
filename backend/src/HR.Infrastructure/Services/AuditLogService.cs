using System.Text.Json;
using HR.Application.Common.Interfaces;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditLogService(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task LogAsync(string action, string entityType, Guid entityId,
        object? oldValues = null, object? newValues = null, CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            TenantId = _currentUser.IsAuthenticated ? _currentUser.TenantId : null,
            UserId = _currentUser.IsAuthenticated ? _currentUser.UserId : null,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(ct);
    }
}
