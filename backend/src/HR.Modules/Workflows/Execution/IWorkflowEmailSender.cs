namespace HR.Modules.Workflows.Execution;

/// <summary>
/// Abstraction over "send an e-mail" so the <see cref="EmailActionHandler"/> stays decoupled from
/// the delivery mechanism (SMTP, queue, test double). The default implementation enqueues onto the
/// existing notification e-mail queue.
/// </summary>
public interface IWorkflowEmailSender
{
    Task SendAsync(string toEmail, string subject, string body, Guid? relatedRequestId, CancellationToken ct);
}
