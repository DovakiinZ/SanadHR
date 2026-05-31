using System.Text.Json;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Audit;
using HR.Domain.Engines.Audit;
using HR.Infrastructure.Persistence;

namespace HR.Infrastructure.Engines.Audit;

public class AuditEngine : IAuditEngine
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditEngine(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task LogChange(string entityType, Guid entityId, string action, object? oldValues = null, object? newValues = null, CancellationToken ct = default)
    {
        var entry = new AuditEntry
        {
            UserId = _currentUser.UserId,
            UserEmail = _currentUser.Email,
            Action = action,
            Module = entityType.Split('.').FirstOrDefault() ?? "Unknown",
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues != null ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues != null ? JsonSerializer.Serialize(newValues) : null,
            Timestamp = DateTime.UtcNow
        };

        _context.AuditEntries.Add(entry);
        await _context.SaveChangesAsync(ct);
    }
}
