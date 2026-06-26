using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Exceptions;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Application.Engines.Permissions;
using HR.Domain.Engines.Permissions;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Identity.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Identity.Controllers;

[Authorize]
[Route("api/access")]
public class AccessController : BaseApiController
{
    private const string ManageTemplates = "Settings.ManageTemplates";

    private readonly ApplicationDbContext _db;
    private readonly IAuditLogService _audit;
    private readonly IPermissionResolver _resolver;
    private readonly IAccessTemplateSeeder _seeder;
    private readonly ICurrentUserService _currentUser;

    public AccessController(ApplicationDbContext db, IAuditLogService audit, IPermissionResolver resolver,
        IAccessTemplateSeeder seeder, ICurrentUserService currentUser)
    {
        _db = db;
        _audit = audit;
        _resolver = resolver;
        _seeder = seeder;
        _currentUser = currentUser;
    }

    // ---- Permission catalog (for the matrix) ----

    [HttpGet("permissions")]
    [RequirePermission("Identity.ViewRoles")]
    public async Task<ActionResult<ApiResponse<List<PermissionCatalogModule>>>> Catalog(CancellationToken ct)
    {
        var perms = await _db.Permissions.AsNoTracking()
            .Select(p => new { p.Module, p.Name })
            .ToListAsync(ct);

        var grouped = perms
            .GroupBy(p => p.Module)
            .OrderBy(g => g.Key)
            .Select(g => new PermissionCatalogModule
            {
                Module = g.Key,
                Permissions = g.OrderBy(x => x.Name)
                    .Select(x => new PermissionCatalogItem { Code = $"{g.Key}.{x.Name}", Name = x.Name })
                    .ToList(),
            })
            .ToList();

        return OkResponse(grouped);
    }

    // ---- Templates ----

    [HttpGet("templates")]
    [RequirePermission("Identity.ViewRoles")]
    public async Task<ActionResult<ApiResponse<List<TemplateDto>>>> Templates(CancellationToken ct)
    {
        var templates = await _db.PermissionTemplates.AsNoTracking().Include(t => t.Items).OrderBy(t => t.NameEn).ToListAsync(ct);
        return OkResponse(templates.Select(ToDto).ToList());
    }

    [HttpGet("templates/{id:guid}")]
    [RequirePermission("Identity.ViewRoles")]
    public async Task<ActionResult<ApiResponse<TemplateDto>>> Template(Guid id, CancellationToken ct)
        => OkResponse(ToDto(await LoadTemplate(id, ct)));

    [HttpPost("templates/seed-defaults")]
    [RequirePermission(ManageTemplates)]
    public async Task<ActionResult<ApiResponse>> SeedDefaults(CancellationToken ct)
    {
        var created = await _seeder.EnsureDefaultsAsync(ct);
        await _audit.LogAsync("TemplatesSeeded", "Access.Template", Guid.Empty, null, new { created }, ct);
        return OkResponse($"{created} template(s) created.");
    }

    [HttpPost("templates")]
    [RequirePermission(ManageTemplates)]
    public async Task<ActionResult<ApiResponse<TemplateDto>>> CreateTemplate([FromBody] CreateTemplateRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.NameEn) && string.IsNullOrWhiteSpace(req.NameAr))
            throw new ConflictException("Template name is required.");
        var template = new PermissionTemplate
        {
            NameEn = (req.NameEn?.Trim() is { Length: > 0 } en) ? en : req.NameAr!.Trim(),
            NameAr = (req.NameAr?.Trim() is { Length: > 0 } ar) ? ar : req.NameEn!.Trim(),
            Description = req.Description?.Trim(),
            IsSystem = false,
        };
        _db.PermissionTemplates.Add(template);
        await _db.SaveChangesAsync(ct);
        if (req.PermissionCodes is { Count: > 0 }) await ApplyTemplateItemsAsync(template.Id, req.PermissionCodes, ct);
        await _audit.LogAsync("TemplateCreated", "Access.Template", template.Id, null, new { template.NameEn }, ct);
        return CreatedResponse(ToDto(await LoadTemplate(template.Id, ct)));
    }

    [HttpPut("templates/{id:guid}")]
    [RequirePermission(ManageTemplates)]
    public async Task<ActionResult<ApiResponse<TemplateDto>>> UpdateTemplate(Guid id, [FromBody] CreateTemplateRequest req, CancellationToken ct)
    {
        var template = await _db.PermissionTemplates.FirstOrDefaultAsync(t => t.Id == id, ct) ?? throw new NotFoundException("Template", id);
        if (!string.IsNullOrWhiteSpace(req.NameEn)) template.NameEn = req.NameEn.Trim();
        if (!string.IsNullOrWhiteSpace(req.NameAr)) template.NameAr = req.NameAr.Trim();
        template.Description = req.Description?.Trim();
        await _db.SaveChangesAsync(ct);
        if (req.PermissionCodes is not null) await ApplyTemplateItemsAsync(id, req.PermissionCodes, ct);
        await _audit.LogAsync("TemplateUpdated", "Access.Template", id, null, new { template.NameEn }, ct);
        return OkResponse(ToDto(await LoadTemplate(id, ct)));
    }

    [HttpPut("templates/{id:guid}/permissions")]
    [RequirePermission(ManageTemplates)]
    public async Task<ActionResult<ApiResponse<TemplateDto>>> SetTemplatePermissions(Guid id, [FromBody] SetPermissionsRequest req, CancellationToken ct)
    {
        _ = await _db.PermissionTemplates.FirstOrDefaultAsync(t => t.Id == id, ct) ?? throw new NotFoundException("Template", id);
        await ApplyTemplateItemsAsync(id, req.PermissionCodes, ct);
        await _audit.LogAsync("TemplatePermissionsChanged", "Access.Template", id, null, new { count = req.PermissionCodes.Count }, ct);
        return OkResponse(ToDto(await LoadTemplate(id, ct)));
    }

    [HttpPost("templates/{id:guid}/duplicate")]
    [RequirePermission(ManageTemplates)]
    public async Task<ActionResult<ApiResponse<TemplateDto>>> Duplicate(Guid id, CancellationToken ct)
    {
        var source = await LoadTemplate(id, ct);
        var copy = new PermissionTemplate
        {
            NameEn = source.NameEn + " (Copy)",
            NameAr = source.NameAr + " (نسخة)",
            Description = source.Description,
            IsSystem = false,
        };
        _db.PermissionTemplates.Add(copy);
        await _db.SaveChangesAsync(ct);
        foreach (var item in source.Items)
            _db.PermissionTemplateItems.Add(new PermissionTemplateItem { PermissionTemplateId = copy.Id, PermissionCode = item.PermissionCode, Scope = item.Scope });
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("TemplateDuplicated", "Access.Template", copy.Id, new { SourceId = id }, new { copy.NameEn }, ct);
        return CreatedResponse(ToDto(await LoadTemplate(copy.Id, ct)));
    }

    [HttpDelete("templates/{id:guid}")]
    [RequirePermission(ManageTemplates)]
    public async Task<ActionResult<ApiResponse>> DeleteTemplate(Guid id, CancellationToken ct)
    {
        var template = await _db.PermissionTemplates.FirstOrDefaultAsync(t => t.Id == id, ct) ?? throw new NotFoundException("Template", id);
        if (template.IsSystem) throw new ConflictException("System templates cannot be deleted.");
        var assigned = await _db.UserPermissionTemplates.AnyAsync(t => t.PermissionTemplateId == id, ct);
        if (assigned) throw new ConflictException("Unassign this template from all users before deleting it.");
        _db.PermissionTemplates.Remove(template);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("TemplateDeleted", "Access.Template", id, new { template.NameEn }, null, ct);
        return OkResponse("Template deleted.");
    }

    // ---- Per-user assignment & overrides ----

    [HttpPost("users/{userId:guid}/assign-template")]
    [RequirePermission("Settings.ManageUsers")]
    public async Task<ActionResult<ApiResponse>> AssignTemplate(Guid userId, [FromBody] AssignTemplateRequest req, CancellationToken ct)
    {
        var exists = await _db.UserPermissionTemplates.AnyAsync(t => t.UserId == userId && t.PermissionTemplateId == req.TemplateId, ct);
        if (!exists)
        {
            _db.UserPermissionTemplates.Add(new UserPermissionTemplate
            {
                UserId = userId,
                PermissionTemplateId = req.TemplateId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = _currentUser.Email,
            });
            await _db.SaveChangesAsync(ct);
        }
        await _audit.LogAsync("TemplateAssigned", "Access.User", userId, null, new { req.TemplateId }, ct);
        return OkResponse("Template assigned.");
    }

    [HttpDelete("users/{userId:guid}/templates/{templateId:guid}")]
    [RequirePermission("Settings.ManageUsers")]
    public async Task<ActionResult<ApiResponse>> RevokeTemplate(Guid userId, Guid templateId, CancellationToken ct)
    {
        var rows = await _db.UserPermissionTemplates.Where(t => t.UserId == userId && t.PermissionTemplateId == templateId).ToListAsync(ct);
        _db.UserPermissionTemplates.RemoveRange(rows);
        await _db.SaveChangesAsync(ct);
        await _audit.LogAsync("TemplateRevoked", "Access.User", userId, new { templateId }, null, ct);
        return OkResponse("Template revoked.");
    }

    [HttpPut("users/{userId:guid}/overrides")]
    [RequirePermission("Settings.ManageUsers")]
    public async Task<ActionResult<ApiResponse>> SetOverrides(Guid userId, [FromBody] SetOverridesRequest req, CancellationToken ct)
    {
        var existing = await _db.UserPermissionOverrides.Where(o => o.UserId == userId).ToListAsync(ct);
        _db.UserPermissionOverrides.RemoveRange(existing);
        foreach (var o in req.Overrides.Where(o => !string.IsNullOrWhiteSpace(o.PermissionCode)))
            _db.UserPermissionOverrides.Add(new UserPermissionOverride
            {
                UserId = userId,
                PermissionCode = o.PermissionCode.Trim(),
                IsGranted = o.IsGranted,
                Scope = ScopeType.Company,
            });
        await _db.SaveChangesAsync(ct);

        // Self-protection: don't let an admin deny their own user-management access.
        if (userId == _currentUser.UserId)
        {
            var effective = await _resolver.ResolveAsync(userId, ct);
            if (!effective.Contains("Settings.ManageUsers"))
                throw new ConflictException("You cannot deny your own user-management access.");
        }

        await _audit.LogAsync("OverridesChanged", "Access.User", userId, null, new { count = req.Overrides.Count }, ct);
        return OkResponse("Overrides updated.");
    }

    [HttpGet("users/{userId:guid}/effective")]
    [RequirePermission("Identity.ViewUsers")]
    public async Task<ActionResult<ApiResponse<List<string>>>> Effective(Guid userId, CancellationToken ct)
        => OkResponse((await _resolver.ResolveAsync(userId, ct)).ToList());

    // ---- Audit log ----

    [HttpGet("audit")]
    [RequirePermission("Settings.ViewAudit")]
    public async Task<ActionResult<ApiResponse<List<AccessAuditDto>>>> Audit([FromQuery] int take, CancellationToken ct)
    {
        var tenantId = _currentUser.TenantId;
        var limit = take is > 0 and <= 500 ? take : 200;
        var raw = await _db.AuditLogs.AsNoTracking()
            .Where(a => a.TenantId == tenantId && a.EntityType.StartsWith("Access."))
            .OrderByDescending(a => a.Timestamp)
            .Take(limit)
            .Select(a => new { a.Id, a.Action, a.EntityType, a.EntityId, a.UserId, a.Timestamp, a.OldValues, a.NewValues })
            .ToListAsync(ct);

        var actorIds = raw.Where(r => r.UserId.HasValue).Select(r => r.UserId!.Value).Distinct().ToList();
        var emails = await _db.Users.IgnoreQueryFilters()
            .Where(u => actorIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToDictionaryAsync(u => u.Id, u => u.Email, ct);

        var rows = raw.Select(a => new AccessAuditDto
        {
            Id = a.Id, Action = a.Action, EntityType = a.EntityType, EntityId = a.EntityId,
            UserEmail = a.UserId.HasValue && emails.TryGetValue(a.UserId.Value, out var e) ? e : null,
            Timestamp = a.Timestamp, OldValues = a.OldValues, NewValues = a.NewValues,
        }).ToList();

        return OkResponse(rows);
    }

    // ---- helpers ----

    private async Task ApplyTemplateItemsAsync(Guid templateId, List<string> codes, CancellationToken ct)
    {
        var existing = await _db.PermissionTemplateItems.Where(i => i.PermissionTemplateId == templateId).ToListAsync(ct);
        _db.PermissionTemplateItems.RemoveRange(existing);
        foreach (var code in codes.Distinct(StringComparer.OrdinalIgnoreCase))
            _db.PermissionTemplateItems.Add(new PermissionTemplateItem
            {
                PermissionTemplateId = templateId,
                PermissionCode = code.Trim(),
                Scope = ScopeType.Company,
            });
        await _db.SaveChangesAsync(ct);
    }

    private async Task<PermissionTemplate> LoadTemplate(Guid id, CancellationToken ct)
        => await _db.PermissionTemplates.AsNoTracking().Include(t => t.Items).FirstOrDefaultAsync(t => t.Id == id, ct)
           ?? throw new NotFoundException("Template", id);

    private static TemplateDto ToDto(PermissionTemplate t) => new()
    {
        Id = t.Id, NameEn = t.NameEn, NameAr = t.NameAr, Description = t.Description, IsSystem = t.IsSystem,
        PermissionCodes = t.Items.Select(i => i.PermissionCode).ToList(),
    };
}
