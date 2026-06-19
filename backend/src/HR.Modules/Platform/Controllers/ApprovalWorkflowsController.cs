using System.Text.Json;
using HR.Api.Controllers;
using HR.Api.Filters;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Engines.Workflows;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.Services.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Controllers;

/// <summary>
/// Business-user approval workflows. A workflow is a flat, ordered chain of approval steps stored as
/// <see cref="WorkflowChainConfig"/> JSON on a published <see cref="WorkflowVersion"/> — exactly what
/// <c>RequestEngine.BuildApprovalChainAsync</c> consumes at submit time. This controller upserts the
/// whole chain in one call (no granular node/edge API) and assigns the workflow to request types.
/// </summary>
[Authorize]
[Route("api/approval-workflows")]
public class ApprovalWorkflowsController : BaseApiController
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public ApprovalWorkflowsController(ApplicationDbContext db, ICurrentUserService user)
    {
        _db = db; _user = user;
    }

    // ── List ────────────────────────────────────────────────────────────────────

    [HttpGet]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<List<ApprovalWorkflowListItemDto>>>> List(CancellationToken ct)
    {
        var defs = await _db.WorkflowDefinitions
            .OrderBy(d => d.NameAr)
            .Select(d => new { d.Id, d.Code, d.NameAr, d.NameEn, d.IsActive })
            .ToListAsync(ct);

        var ids = defs.Select(d => d.Id).ToList();
        var versions = await _db.WorkflowVersions
            .Where(v => ids.Contains(v.WorkflowDefinitionId) && v.IsPublished)
            .Select(v => new { v.WorkflowDefinitionId, v.VersionNumber, v.Configuration })
            .ToListAsync(ct);
        var cfgByDef = versions
            .GroupBy(v => v.WorkflowDefinitionId)
            .ToDictionary(g => g.Key, g => ParseChain(g.OrderByDescending(v => v.VersionNumber).First().Configuration));

        var assigned = await _db.RequestTypes.Where(t => t.WorkflowDefinitionId != null)
            .Select(t => new { t.WorkflowDefinitionId, t.NameAr }).ToListAsync(ct);
        var typesByDef = assigned.GroupBy(a => a.WorkflowDefinitionId!.Value)
            .ToDictionary(g => g.Key, g => g.Select(x => x.NameAr).ToList());

        var result = defs.Select(d => new ApprovalWorkflowListItemDto
        {
            Id = d.Id, Code = d.Code, Name = d.NameAr, IsActive = d.IsActive,
            StepCount = cfgByDef.TryGetValue(d.Id, out var c) ? c.Steps.Count : 0,
            AssignedRequestTypes = typesByDef.TryGetValue(d.Id, out var ts) ? ts : new(),
        }).ToList();
        return OkResponse(result);
    }

    // ── Get one (with parsed steps) ──────────────────────────────────────────────

    [HttpGet("{id:guid}")]
    [RequirePermission("Platform.Workflows.View")]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowDetailDto>>> Get(Guid id, CancellationToken ct)
    {
        var dto = await BuildDetailAsync(id, ct);
        return dto is null ? NotFound(ApiResponse.Fail("Workflow not found")) : OkResponse(dto);
    }

    private async Task<ApprovalWorkflowDetailDto?> BuildDetailAsync(Guid id, CancellationToken ct)
    {
        var def = await _db.WorkflowDefinitions.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (def is null) return null;

        var version = await _db.WorkflowVersions.Where(v => v.WorkflowDefinitionId == id && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber).FirstOrDefaultAsync(ct);
        var cfg = ParseChain(version?.Configuration);

        var assignedTypeIds = await _db.RequestTypes.Where(t => t.WorkflowDefinitionId == id)
            .Select(t => t.Id).ToListAsync(ct);

        return new ApprovalWorkflowDetailDto
        {
            Id = def.Id, Code = def.Code, Name = def.NameAr, Description = def.NameEn == def.NameAr ? null : def.NameEn,
            IsActive = def.IsActive,
            Steps = cfg.Steps.Select(ToStepDto).ToList(),
            RequestTypeIds = assignedTypeIds,
        };
    }

    // ── Create ───────────────────────────────────────────────────────────────────

    [HttpPost]
    [RequirePermission("Platform.Workflows.Create")]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowDetailDto>>> Create([FromBody] UpsertApprovalWorkflowBody body, CancellationToken ct)
    {
        var name = (body.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(ApiResponse.Fail("اسم المسار مطلوب"));

        var code = await UniqueCodeAsync(body.Code, name, ct);
        var def = new WorkflowDefinition
        {
            TenantId = _user.TenantId,
            Code = code,
            NameAr = name,
            NameEn = string.IsNullOrWhiteSpace(body.Description) ? name : body.Description!.Trim(),
            TriggerEntityType = "RequestInstance",
            IsActive = body.IsActive,
        };
        _db.WorkflowDefinitions.Add(def);

        _db.WorkflowVersions.Add(new WorkflowVersion
        {
            WorkflowDefinitionId = def.Id,
            VersionNumber = 1,
            IsPublished = true,
            PublishedAt = DateTime.UtcNow,
            Configuration = SerializeChain(body.Steps),
        });

        await ReassignRequestTypesAsync(def.Id, body.RequestTypeIds, ct);
        await _db.SaveChangesAsync(ct);
        return CreatedResponse((await BuildDetailAsync(def.Id, ct))!);
    }

    // ── Update (upsert published version + reconcile assignments) ─────────────────

    [HttpPut("{id:guid}")]
    [RequirePermission("Platform.Workflows.Edit")]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowDetailDto>>> Update(Guid id, [FromBody] UpsertApprovalWorkflowBody body, CancellationToken ct)
    {
        var def = await _db.WorkflowDefinitions.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (def is null) return NotFound(ApiResponse.Fail("Workflow not found"));

        var name = (body.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name)) return BadRequest(ApiResponse.Fail("اسم المسار مطلوب"));
        def.NameAr = name;
        def.NameEn = string.IsNullOrWhiteSpace(body.Description) ? name : body.Description!.Trim();
        def.IsActive = body.IsActive;

        var version = await _db.WorkflowVersions.Where(v => v.WorkflowDefinitionId == id && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber).FirstOrDefaultAsync(ct);
        if (version is null)
        {
            _db.WorkflowVersions.Add(new WorkflowVersion
            {
                WorkflowDefinitionId = id, VersionNumber = 1, IsPublished = true,
                PublishedAt = DateTime.UtcNow, Configuration = SerializeChain(body.Steps),
            });
        }
        else
        {
            version.Configuration = SerializeChain(body.Steps);
            version.PublishedAt = DateTime.UtcNow;
        }

        await ReassignRequestTypesAsync(id, body.RequestTypeIds, ct);
        await _db.SaveChangesAsync(ct);
        return OkResponse((await BuildDetailAsync(id, ct))!);
    }

    // ── Duplicate ─────────────────────────────────────────────────────────────────

    [HttpPost("{id:guid}/duplicate")]
    [RequirePermission("Platform.Workflows.Create")]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowDetailDto>>> Duplicate(Guid id, CancellationToken ct)
    {
        var def = await _db.WorkflowDefinitions.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (def is null) return NotFound(ApiResponse.Fail("Workflow not found"));
        var version = await _db.WorkflowVersions.Where(v => v.WorkflowDefinitionId == id && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber).FirstOrDefaultAsync(ct);

        var copyName = $"{def.NameAr} (نسخة)";
        var copy = new WorkflowDefinition
        {
            TenantId = _user.TenantId,
            Code = await UniqueCodeAsync(null, copyName, ct),
            NameAr = copyName, NameEn = copyName,
            TriggerEntityType = "RequestInstance", IsActive = false, // a copy starts disabled until reviewed
        };
        _db.WorkflowDefinitions.Add(copy);
        _db.WorkflowVersions.Add(new WorkflowVersion
        {
            WorkflowDefinitionId = copy.Id, VersionNumber = 1, IsPublished = true,
            PublishedAt = DateTime.UtcNow, Configuration = version?.Configuration ?? SerializeChain(new()),
        });
        await _db.SaveChangesAsync(ct);
        return CreatedResponse((await BuildDetailAsync(copy.Id, ct))!);
    }

    // ── Delete (soft) ─────────────────────────────────────────────────────────────

    [HttpDelete("{id:guid}")]
    [RequirePermission("Platform.Workflows.Delete")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var def = await _db.WorkflowDefinitions.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (def is null) return NotFound(ApiResponse.Fail("Workflow not found"));
        // Clear assignments so no request type points at a removed workflow.
        await ReassignRequestTypesAsync(id, new(), ct);
        def.IsActive = false;
        def.IsDeleted = true;
        def.DeletedAt = DateTime.UtcNow;
        def.DeletedBy = _user.Email;
        await _db.SaveChangesAsync(ct);
        return OkResponse("Workflow deleted");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private async Task ReassignRequestTypesAsync(Guid workflowId, List<Guid>? requestTypeIds, CancellationToken ct)
    {
        var want = (requestTypeIds ?? new()).ToHashSet();
        // Clear types currently pointing here but no longer selected.
        var current = await _db.RequestTypes.Where(t => t.WorkflowDefinitionId == workflowId).ToListAsync(ct);
        foreach (var t in current.Where(t => !want.Contains(t.Id))) t.WorkflowDefinitionId = null;
        // Assign newly-selected types.
        if (want.Count > 0)
        {
            var toAssign = await _db.RequestTypes.Where(t => want.Contains(t.Id)).ToListAsync(ct);
            foreach (var t in toAssign) t.WorkflowDefinitionId = workflowId;
        }
    }

    private async Task<string> UniqueCodeAsync(string? requested, string name, CancellationToken ct)
    {
        var baseCode = !string.IsNullOrWhiteSpace(requested)
            ? requested!.Trim().ToUpperInvariant().Replace(' ', '-')
            : "AWF-" + new string((name.Where(char.IsLetterOrDigit).Take(8).ToArray())).ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(baseCode)) baseCode = "AWF";
        var code = baseCode; var n = 1;
        while (await _db.WorkflowDefinitions.IgnoreQueryFilters().AnyAsync(d => d.TenantId == _user.TenantId && d.Code == code, ct))
            code = $"{baseCode}-{++n}";
        return code;
    }

    private static WorkflowChainConfig ParseChain(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return new();
        try { return JsonSerializer.Deserialize<WorkflowChainConfig>(json, Json) ?? new(); }
        catch { return new(); }
    }

    private static string SerializeChain(List<ApprovalWorkflowStepDto>? steps)
    {
        var cfg = new WorkflowChainConfig
        {
            Steps = (steps ?? new()).Select((s, i) => new WorkflowStepConfig
            {
                ApproverType = s.ApproverType,
                NameAr = string.IsNullOrWhiteSpace(s.NameAr) ? $"خطوة {i + 1}" : s.NameAr,
                NameEn = string.IsNullOrWhiteSpace(s.NameEn) ? $"Step {i + 1}" : s.NameEn,
                SpecificEntityId = s.SpecificEntityId,
                ChainLevel = s.ChainLevel <= 0 ? 1 : s.ChainLevel,
                Required = s.Required,
                CanReject = s.CanReject,
                CanReturn = s.CanReturn,
                CanDelegate = s.CanDelegate,
                Conditions = (s.Conditions ?? new()).Select(c => new StepConditionConfig
                {
                    Field = c.Field, Operator = c.Operator, Value = c.Value,
                }).ToList(),
            }).ToList(),
        };
        return JsonSerializer.Serialize(cfg, Json);
    }

    private static ApprovalWorkflowStepDto ToStepDto(WorkflowStepConfig s) => new()
    {
        ApproverType = s.ApproverType, NameAr = s.NameAr, NameEn = s.NameEn,
        SpecificEntityId = s.SpecificEntityId ?? s.SpecificUserId,
        ChainLevel = s.ChainLevel, Required = s.Required,
        CanReject = s.CanReject, CanReturn = s.CanReturn, CanDelegate = s.CanDelegate,
        Conditions = (s.Conditions ?? new()).Select(c => new ApprovalWorkflowConditionDto { Field = c.Field, Operator = c.Operator, Value = c.Value }).ToList(),
    };
}

// ── DTOs ───────────────────────────────────────────────────────────────────────

public sealed class ApprovalWorkflowListItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public int StepCount { get; set; }
    public List<string> AssignedRequestTypes { get; set; } = new();
}

public sealed class ApprovalWorkflowDetailDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<ApprovalWorkflowStepDto> Steps { get; set; } = new();
    public List<Guid> RequestTypeIds { get; set; } = new();
}

public sealed class ApprovalWorkflowStepDto
{
    public int ApproverType { get; set; }
    public string NameAr { get; set; } = "";
    public string NameEn { get; set; } = "";
    public Guid? SpecificEntityId { get; set; }
    public int ChainLevel { get; set; } = 1;
    public bool Required { get; set; } = true;
    public bool CanReject { get; set; } = true;
    public bool CanReturn { get; set; } = true;
    public bool CanDelegate { get; set; }
    public List<ApprovalWorkflowConditionDto> Conditions { get; set; } = new();
}

public sealed class ApprovalWorkflowConditionDto
{
    public string Field { get; set; } = "";
    public string Operator { get; set; } = "eq";
    public string Value { get; set; } = "";
}

public sealed class UpsertApprovalWorkflowBody
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ApprovalWorkflowStepDto> Steps { get; set; } = new();
    public List<Guid> RequestTypeIds { get; set; } = new();
}
