namespace HR.Application.Engines.Permissions;

/// <summary>Resolves a user's effective permission set by unifying every source — role permissions,
/// direct user permissions, assigned permission-template items, and per-user allow/deny overrides — with
/// explicit deny winning. This is the single authority used to stamp the JWT at login/refresh and to
/// answer "what can this user do right now".</summary>
public interface IPermissionResolver
{
    Task<IReadOnlyList<string>> ResolveAsync(Guid userId, CancellationToken ct = default);
}
