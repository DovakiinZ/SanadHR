namespace HR.Application.Common.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(string action, string entityType, Guid entityId, object? oldValues = null, object? newValues = null, CancellationToken ct = default);
}
