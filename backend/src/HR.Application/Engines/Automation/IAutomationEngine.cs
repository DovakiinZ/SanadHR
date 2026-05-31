namespace HR.Application.Engines.Automation;

public interface IAutomationEngine
{
    Task PublishEvent(string eventType, string entityType, Guid entityId, object? payload = null, CancellationToken ct = default);
}
