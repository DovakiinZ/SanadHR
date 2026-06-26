using System.Security.Claims;
using HR.Application.Common.Interfaces;

namespace HR.Api.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IBackgroundExecutionContext _background;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, IBackgroundExecutionContext background)
    {
        _httpContextAccessor = httpContextAccessor;
        _background = background;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    private bool HttpAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    // Authenticated either via the HTTP principal (in-request) or an ambient background scope
    // (Hangfire / batch execution). HTTP always wins when present.
    public bool IsAuthenticated => HttpAuthenticated || _background.IsActive;

    public Guid UserId => HttpAuthenticated
        ? Guid.Parse(User!.FindFirstValue(ClaimTypes.NameIdentifier)!)
        : _background.UserId ?? Guid.Empty;

    public Guid TenantId => HttpAuthenticated
        ? Guid.Parse(User!.FindFirstValue("tenant_id")!)
        : _background.IsActive ? _background.TenantId : Guid.Empty;

    public string? Email => HttpAuthenticated
        ? User!.FindFirstValue(ClaimTypes.Email)
        : _background.Email;

    public IReadOnlyList<string> Permissions => User?.FindAll("permission")
        .Select(c => c.Value).ToList().AsReadOnly() ?? new List<string>().AsReadOnly();
}
