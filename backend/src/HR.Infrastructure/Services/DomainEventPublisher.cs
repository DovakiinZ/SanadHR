using HR.Application.Common.Interfaces;
using MediatR;

namespace HR.Infrastructure.Services;

/// <summary>In-process domain-event publisher backed by MediatR. Subscribers are INotificationHandler
/// implementations discovered by MediatR. A durable outbox can replace this later behind the same
/// interface without changing any engine code.</summary>
public sealed class DomainEventPublisher : IDomainEventPublisher
{
    private readonly IPublisher _mediator;

    public DomainEventPublisher(IPublisher mediator) => _mediator = mediator;

    public Task PublishAsync(INotification domainEvent, CancellationToken ct = default) =>
        _mediator.Publish(domainEvent, ct);
}
