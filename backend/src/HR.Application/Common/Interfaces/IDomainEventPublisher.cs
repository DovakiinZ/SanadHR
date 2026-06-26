using MediatR;

namespace HR.Application.Common.Interfaces;

/// <summary>Publishes domain events to any interested subscribers. Engines depend on this abstraction
/// rather than MediatR directly, so the transport (in-process now, an outbox later) can change without
/// touching domain code. Every significant payroll milestone emits an event other modules can react to.</summary>
public interface IDomainEventPublisher
{
    Task PublishAsync(INotification domainEvent, CancellationToken ct = default);
}
