namespace HR.Application.Engines.Audit;

public interface IAuditEngine
{
    Task LogChange(string entityType, Guid entityId, string action, object? oldValues = null, object? newValues = null, CancellationToken ct = default);
}
