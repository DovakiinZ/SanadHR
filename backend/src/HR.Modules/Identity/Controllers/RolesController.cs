using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using HR.Modules.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Identity.Controllers;

[Authorize]
[Route("api/roles")]
public class RolesController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public RolesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [RequirePermission("Identity.ViewRoles")]
    public async Task<ActionResult<ApiResponse<List<RoleDto>>>> GetAll(CancellationToken ct)
    {
        var roles = await _context.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                NameAr = r.NameAr,
                Description = r.Description,
                IsSystemRole = r.IsSystemRole,
                Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
            })
            .ToListAsync(ct);

        return OkResponse(roles);
    }

    [HttpPost]
    [RequirePermission("Identity.CreateRoles")]
    public async Task<ActionResult<ApiResponse<RoleDto>>> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
    {
        var role = new Role
        {
            Name = request.Name,
            NameAr = request.NameAr,
            Description = request.Description
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync(ct);

        return CreatedResponse(new RoleDto { Id = role.Id, Name = role.Name, NameAr = role.NameAr, Description = role.Description });
    }
}

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public List<string> Permissions { get; set; } = new();
}

public class CreateRoleRequest
{
    public string Name { get; set; } = null!;
    public string? NameAr { get; set; }
    public string? Description { get; set; }
}
