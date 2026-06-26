using HR.Application.Engines.Finance.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>A sample subscriber demonstrating the event-driven architecture: it reacts to payroll
/// milestones without the payroll engine knowing it exists. Real subscribers (notifications, GL export,
/// analytics) follow the same shape — implement INotificationHandler&lt;TEvent&gt; and register it.</summary>
public sealed class PayrollEventLogHandler :
    INotificationHandler<PayrollApprovedEvent>,
    INotificationHandler<PayrollExecutionStartedEvent>,
    INotificationHandler<PayrollCompletedEvent>,
    INotificationHandler<PayrollExecutionFailedEvent>
{
    private readonly ILogger<PayrollEventLogHandler> _logger;

    public PayrollEventLogHandler(ILogger<PayrollEventLogHandler> logger) => _logger = logger;

    public Task Handle(PayrollApprovedEvent e, CancellationToken ct)
    {
        _logger.LogInformation("Payroll run {RunNumber} ({RunId}) approved.", e.RunNumber, e.RunId);
        return Task.CompletedTask;
    }

    public Task Handle(PayrollExecutionStartedEvent e, CancellationToken ct)
    {
        _logger.LogInformation("Payroll run {RunId} execution started over {Count} item(s).", e.RunId, e.ItemCount);
        return Task.CompletedTask;
    }

    public Task Handle(PayrollCompletedEvent e, CancellationToken ct)
    {
        _logger.LogInformation(
            "Payroll run {RunNumber} completed: {Completed} posted, {Failed} failed, net {Net}.",
            e.RunNumber, e.Completed, e.Failed, e.NetTotal);
        return Task.CompletedTask;
    }

    public Task Handle(PayrollExecutionFailedEvent e, CancellationToken ct)
    {
        _logger.LogWarning("Payroll run {RunId} execution finished with failures: {Failed} failed, {Completed} ok.",
            e.RunId, e.Failed, e.Completed);
        return Task.CompletedTask;
    }
}
