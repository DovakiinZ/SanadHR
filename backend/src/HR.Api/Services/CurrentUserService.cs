using System.Security.Claims;
using HR.Application.Common.Interfaces;

namespace HR.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId => IsAuthenticated
        ? Guid.Parse(User!.FindFirstValue(ClaimTypes.NameIdentifier)!)
        : Guid.Empty;

    public Guid TenantId => IsAuthenticated
        ? Guid.Parse(User!.FindFirstValue("tenant_id")!)
        : Guid.Empty;

    public string? Email => User?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyList<string> Permissions => User?.FindAll("permission")
        .Select(c => c.Value).ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
}
