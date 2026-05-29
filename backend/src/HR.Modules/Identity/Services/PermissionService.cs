using HR.Infrastructure.Persistence;
using HR.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Identity.Services;

public class PermissionService
{
    private readonly ApplicationDbContext _context;

    public PermissionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var rolePermissions = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Module}.{rp.Permission.Name}")
            .ToListAsync(ct);

        var directPermissions = await _context.UserPermissions
            .Where(up => up.UserId == userId)
            .Select(up => $"{up.Permission.Module}.{up.Permission.Name}")
            .ToListAsync(ct);

        return rolePermissions.Union(directPermissions).Distinct().ToList();
    }
}
