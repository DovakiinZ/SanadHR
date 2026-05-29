namespace HR.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string? Email { get; }
    IReadOnlyList<string> Permissions { get; }
    bool IsAuthenticated { get; }
}
