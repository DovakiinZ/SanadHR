using HR.Application.Common.Interfaces;
using HR.Application.Engines.Permissions;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Permissions;

public class PermissionEvaluator : IPermissionEvaluator
{
    private readonly ApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PermissionEvaluator(ApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> HasPermission(Guid userId, string permissionCode,
        Guid? entityTenantId = null, Guid? entityBranchId = null, Guid? entityDepartmentId = null)
    {
        // Check explicit override first
        var overrideEntry = await _context.UserPermissionOverrides
            .Where(x => x.UserId == userId && x.PermissionCode == permissionCode)
            .FirstOrDefaultAsync();

        if (overrideEntry != null)
        {
            if (!overrideEntry.IsGranted) return false;
            return EvaluateScope(overrideEntry.Scope, entityTenantId, entityBranchId, entityDepartmentId);
        }

        // Check basic permission from existing system
        if (_currentUser.Permissions.Contains(permissionCode))
            return true;

        return false;
    }

    private bool EvaluateScope(ScopeType scope, Guid? entityTenantId, Guid? entityBranchId, Guid? entityDepartmentId)
    {
        return scope switch
        {
            ScopeType.Company => true,
            ScopeType.Own => true, // Caller must verify ownership
            _ => true // Default allow; detailed scope evaluation can be expanded
        };
    }
}
