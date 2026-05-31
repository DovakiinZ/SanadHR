using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Models;
using HR.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Controllers;

[Authorize]
[Route("api/platform/admin")]
public class AdminController : BaseApiController
{
    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("overview")]
    [RequirePermission("Platform.Admin.View")]
    public async Task<ActionResult<ApiResponse<PlatformOverviewDto>>> GetOverview(CancellationToken ct)
    {
        var dto = new PlatformOverviewDto
        {
            MetadataDefinitionsCount = await _context.MetadataDefinitions.CountAsync(ct),
            ObjectDefinitionsCount = await _context.ObjectDefinitions.CountAsync(ct),
            PermissionTemplatesCount = await _context.PermissionTemplates.CountAsync(ct),
            FormDefinitionsCount = await _context.FormDefinitions.CountAsync(ct),
            WorkflowDefinitionsCount = await _context.WorkflowDefinitions.CountAsync(ct),
            AutomationRulesCount = await _context.AutomationRules.CountAsync(ct),
            DashboardDefinitionsCount = await _context.DashboardDefinitions.CountAsync(ct)
        };

        return OkResponse(dto);
    }
}

public class PlatformOverviewDto
{
    public int MetadataDefinitionsCount { get; set; }
    public int ObjectDefinitionsCount { get; set; }
    public int PermissionTemplatesCount { get; set; }
    public int FormDefinitionsCount { get; set; }
    public int WorkflowDefinitionsCount { get; set; }
    public int AutomationRulesCount { get; set; }
    public int DashboardDefinitionsCount { get; set; }
}
