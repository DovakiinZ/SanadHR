using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Identity.DTOs;
using HR.Modules.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Identity.Controllers;

[Authorize]
[Route("api/roles")]
public class RolesController : BaseApiController
{
    private const string ManageRoles = "Settings.ManageRoles";

    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _audit;

    public RolesController(ApplicationDbContext db, IAuditLogService audit)
    {
        _db = db;
        _audit = audit;
    }

    [HttpGet]
    [RequirePermission("Identity.ViewRoles")]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAll(CancellationToken ct)
    {
        var roles = await _db.Roles.AsNoTracking()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
        return OkResponse(roles.Select(ToDto).ToList());
    }

    [HttpGet("{id:guid}")]
    [RequirePermission("Identity.ViewRoles")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> GetById(Guid id, CancellationToken ct)
        => OkResponse(await LoadDto(id, ct));

    [HttpPost]
    [RequirePermission(ManageRoles)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name)) throw new ConflictException("Role name is required.");
        var role = new Role
        {
            Name = req.Name.Trim(),
            NameAr = req.NameAr?.Trim(),
            Description = req.Description?.Trim(),
            IsSystemRole = false,
        };
        _db.Roles.Add(role);
        await _db.SaveChangesAsync(ct);

        if (req.PermissionCodes is { Count: > 0 })
            await ApplyPermissionsAsync(role.Id, req.PermissionCodes, ct);

        await _audit.LogAsync("RoleCreated", "Access.Role", role.Id, null, new { role.Name }, ct);
        return CreatedResponse(await LoadDto(role.Id, ct));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission(ManageRoles)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Update(Guid id, [FromBody] CreateRoleRequest req, CancellationToken ct)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct) ?? throw new NotFoundException("Role", id);
        if (!string.IsNullOrWhiteSpace(req.Name)) role.Name = req.Name.Trim();
        role.NameAr = req.NameAr?.Trim();
        role.Description = req.Description?.Trim();
        await _db.SaveChangesAsync(ct);
        if (req.PermissionCodes is not null) await ApplyPermissionsAsync(id, req.PermissionCodes, ct);
        await _audit.LogAsync("RoleUpdated", "Access.Role", id, null, new { role.Name }, ct);
        return OkResponse(await LoadDto(id, ct));
    }

    [HttpPut("{id:guid}/permissions")]
    [RequirePermission(ManageRoles)]
    public async Task<ActionResult<ApiResponse<RoleDto>>> SetPermissions(Guid id, [FromBody] SetPermissionsRequest req, CancellationToken ct)
    {
        _ = await _db.Roles.FirstOrDefaultAsync(r => r.Id == id, ct) ?? throw new NotFoundException("Role", id);
        await ApplyPermissionsAsync(id, req.PermissionCodes, ct);
        await _audit.LogAsync("RolePermissionsChanged", "Access.Role", id, null, new { count = req.PermissionCodes.Count }, ct);
        return OkResponse(await LoadDto(id, ct));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission(ManageRoles)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var role = await _db.Roles.Include(r => r.UserRoles).FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new NotFoundException("Role", id);
        if (role.IsSystemRole) throw new ConflictException("System roles cannot be deleted.");
        if (role.UserRoles.Count > 0) throw new ConflictException("Unassign all users before deleting this role.");
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("RoleDeleted", "Access.Role", id, new { role.Name }, null, ct);
        return OkResponse("Role deleted.");
    }

    private async Task ApplyPermissionsAsync(Guid roleId, List<string> codes, CancellationToken ct)
    {
        var catalog = await _db.Permissions.Select(p => new { p.Id, Code = p.Module + "." + p.Name }).ToListAsync(ct);
        var byCode = catalog.ToDictionary(c => c.Code, c => c.Id, StringComparer.OrdinalIgnoreCase);

        var existing = await _db.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(ct);
        _db.RolePermissions.RemoveRange(existing);
        foreach (var code in codes.Distinct(StringComparer.OrdinalIgnoreCase))
            if (byCode.TryGetValue(code, out var pid))
                _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });
        await _db.SaveChangesAsync(ct);
    }

    private async Task<RoleDto> LoadDto(Guid id, CancellationToken ct)
    {
        var role = await _db.Roles.AsNoTracking()
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id, ct) ?? throw new NotFoundException("Role", id);
        return ToDto(role);
    }

    private static RoleDto ToDto(Role r) => new()
    {
        Id = r.Id, Name = r.Name, NameAr = r.NameAr, Description = r.Description,
        IsSystemRole = r.IsSystemRole, UserCount = r.UserRoles.Count,
        PermissionCodes = r.RolePermissions.Select(rp => rp.Permission.Module + "." + rp.Permission.Name).ToList(),
    };
}
