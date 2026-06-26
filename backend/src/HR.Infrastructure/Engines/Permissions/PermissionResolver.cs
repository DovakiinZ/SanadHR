using HR.Application.Engines.Permissions;
using HR.Domain.Engines.Permissions;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Permissions;

/// <summary>DB-backed unified permission resolver. Gathers all sources for a user and merges them via the
/// pure <see cref="PermissionMerge"/> (deny wins). Uses IgnoreQueryFilters so it works during login (before
/// a tenant principal exists) and in background contexts.</summary>
public sealed class PermissionResolver : IPermissionResolver
{
    private readonly ApplicationDbContext _db;

    public PermissionResolver(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<string>> ResolveAsync(Guid userId, CancellationToken ct = default)
    {
        // Role-derived permissions ("Module.Name").
        var rolePerms = await _db.UserRoles.IgnoreQueryFilters()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Module + "." + rp.Permission.Name)
            .ToListAsync(ct);

        // Direct user permissions.
        var directPerms = await _db.UserPermissions.IgnoreQueryFilters()
            .Where(up => up.UserId == userId)
            .Select(up => up.Permission.Module + "." + up.Permission.Name)
            .ToListAsync(ct);

        // Permission-template items assigned to the user.
        var templateIds = await _db.UserPermissionTemplates.IgnoreQueryFilters()
            .Where(t => t.UserId == userId)
            .Select(t => t.PermissionTemplateId)
            .ToListAsync(ct);
        var templatePerms = templateIds.Count == 0
            ? new List<string>()
            : await _db.PermissionTemplateItems.IgnoreQueryFilters()
                .Where(i => templateIds.Contains(i.PermissionTemplateId))
                .Select(i => i.PermissionCode)
                .ToListAsync(ct);

        // Per-user overrides (allow / deny).
        var overrides = await _db.UserPermissionOverrides.IgnoreQueryFilters()
            .Where(o => o.UserId == userId)
            .Select(o => new { o.PermissionCode, o.IsGranted })
            .ToListAsync(ct);
        var allow = overrides.Where(o => o.IsGranted).Select(o => o.PermissionCode);
        var deny = overrides.Where(o => !o.IsGranted).Select(o => o.PermissionCode);

        return PermissionMerge.Resolve(rolePerms, directPerms, templatePerms, allow, deny);
    }
}
