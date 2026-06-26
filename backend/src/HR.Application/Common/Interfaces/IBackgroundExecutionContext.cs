namespace HR.Application.Common.Interfaces;

/// <summary>An ambient tenant/user context for work that runs outside an HTTP request (Hangfire jobs,
/// batch payroll execution). A background worker opens a scope with <see cref="Begin"/> for the tenant it
/// is processing; <see cref="ICurrentUserService"/> falls back to this when no HTTP principal is present,
/// so tenant isolation and audit stamping keep working in the background exactly as they do in-request.</summary>
public interface IBackgroundExecutionContext
{
    bool IsActive { get; }
    Guid TenantId { get; }
    Guid? UserId { get; }
    string? Email { get; }

    /// <summary>Begin an ambient scope; dispose the returned handle to restore the previous scope.</summary>
    IDisposable Begin(Guid tenantId, Guid? userId = null, string? email = null);
}
