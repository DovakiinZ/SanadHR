using HR.Modules.Platform.DTOs.Permissions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Queries.Permissions;

public record GetUserEffectivePermissionsQuery(Guid UserId) : IRequest<List<UserEffectivePermissionDto>>;

public record GetAvailablePermissionsQuery : IRequest<List<string>>;

public class GetUserEffectivePermissionsQueryHandler : IRequestHandler<GetUserEffectivePermissionsQuery, List<UserEffectivePermissionDto>>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public GetUserEffectivePermissionsQueryHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserEffectivePermissionDto>> Handle(GetUserEffectivePermissionsQuery request, CancellationToken cancellationToken)
    {
        var result = new List<UserEffectivePermissionDto>();

        // Get permissions from assigned templates
        var templatePermissions = await _context.UserPermissionTemplates
            .Where(upt => upt.UserId == request.UserId)
            .Include(upt => upt.PermissionTemplate)
                .ThenInclude(pt => pt.Items)
            .SelectMany(upt => upt.PermissionTemplate.Items)
            .ToListAsync(cancellationToken);

        foreach (var item in templatePermissions)
        {
            result.Add(new UserEffectivePermissionDto
            {
                PermissionCode = item.PermissionCode,
                IsGranted = true,
                Scope = item.Scope.ToString(),
                Source = "Template"
            });
        }

        // Get overrides (these take precedence)
        var overrides = await _context.UserPermissionOverrides
            .Where(o => o.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        foreach (var over in overrides)
        {
            // Remove template-based entry if override exists
            result.RemoveAll(r => r.PermissionCode == over.PermissionCode);
            result.Add(new UserEffectivePermissionDto
            {
                PermissionCode = over.PermissionCode,
                IsGranted = over.IsGranted,
                Scope = over.Scope.ToString(),
                Source = "Override"
            });
        }

        return result;
    }
}

public class GetAvailablePermissionsQueryHandler : IRequestHandler<GetAvailablePermissionsQuery, List<string>>
{
    private readonly HR.Infrastructure.Persistence.ApplicationDbContext _context;

    public GetAvailablePermissionsQueryHandler(HR.Infrastructure.Persistence.ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> Handle(GetAvailablePermissionsQuery request, CancellationToken cancellationToken)
    {
        // Return all known permission codes from the Permission seed table
        var permissions = await _context.Set<HR.Modules.Identity.Entities.Permission>()
            .Select(p => $"{p.Module}.{p.Name}")
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
