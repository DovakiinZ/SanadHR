using HR.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace HR.Api.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string[] _permissions;

    /// <summary>The user must hold AT LEAST ONE of the supplied permissions (OR semantics). A single
    /// permission keeps the original behaviour.</summary>
    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

        if (!currentUser.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (_permissions.Length > 0 && !_permissions.Any(p => currentUser.Permissions.Contains(p)))
        {
            context.Result = new ForbidResult();
            return;
        }

        await Task.CompletedTask;
    }
}
