using HR.Api.Filters;
using HR.Application.Common.Interfaces;
using HR.Application.Common.Models;
using HR.Domain.Engines.Notifications;
using HR.Infrastructure.Persistence;
using HR.Modules.Platform.Services.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Controllers;

/// <summary>
/// Admin configuration for notification rules (Settings → Notifications). The first event is
/// document expiry: notify the configured recipients N days before an employee document expires.
/// </summary>
[Authorize]
[ApiController]
[Route("api/notifications/rules")]
public class NotificationRulesController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IDocumentExpiryScanner _scanner;
    private readonly ICurrentUserService _user;
    public NotificationRulesController(ApplicationDbContext db, IDocumentExpiryScanner scanner, ICurrentUserService user)
    { _db = db; _scanner = scanner; _user = user; }

    public sealed class RuleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Event { get; set; } = "DocumentExpiry";
        public int DaysBefore { get; set; }
        public string? DocumentType { get; set; }
        public bool NotifyEmployee { get; set; }
        public bool NotifyDirectManager { get; set; }
        public bool NotifyDepartmentManager { get; set; }
        public Guid? ExtraEmployeeId { get; set; }
        public Guid? RoleId { get; set; }
        public bool ChannelBell { get; set; }
        public bool ChannelEmail { get; set; }
        public bool ChannelSms { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class RuleInput
    {
        public string Name { get; set; } = null!;
        public string Event { get; set; } = "DocumentExpiry";
        public int DaysBefore { get; set; } = 30;
        public string? DocumentType { get; set; }
        public bool NotifyEmployee { get; set; }
        public bool NotifyDirectManager { get; set; }
        public bool NotifyDepartmentManager { get; set; }
        public Guid? ExtraEmployeeId { get; set; }
        public Guid? RoleId { get; set; }
        public bool ChannelBell { get; set; } = true;
        public bool ChannelEmail { get; set; }
        public bool ChannelSms { get; set; }
        public bool IsActive { get; set; } = true;
    }

    private static RuleDto ToDto(NotificationRule r) => new()
    {
        Id = r.Id, Name = r.Name, Event = r.Event, DaysBefore = r.DaysBefore, DocumentType = r.DocumentType,
        NotifyEmployee = r.NotifyEmployee, NotifyDirectManager = r.NotifyDirectManager,
        NotifyDepartmentManager = r.NotifyDepartmentManager, ExtraEmployeeId = r.ExtraEmployeeId, RoleId = r.RoleId,
        ChannelBell = r.ChannelBell, ChannelEmail = r.ChannelEmail, ChannelSms = r.ChannelSms,
        IsActive = r.IsActive, CreatedAt = r.CreatedAt,
    };

    private static void Apply(NotificationRule r, RuleInput i)
    {
        r.Name = i.Name.Trim();
        r.Event = string.IsNullOrWhiteSpace(i.Event) ? "DocumentExpiry" : i.Event.Trim();
        r.DaysBefore = i.DaysBefore < 0 ? 0 : i.DaysBefore;
        r.DocumentType = string.IsNullOrWhiteSpace(i.DocumentType) ? null : i.DocumentType.Trim();
        r.NotifyEmployee = i.NotifyEmployee;
        r.NotifyDirectManager = i.NotifyDirectManager;
        r.NotifyDepartmentManager = i.NotifyDepartmentManager;
        r.ExtraEmployeeId = i.ExtraEmployeeId;
        r.RoleId = i.RoleId;
        r.ChannelBell = i.ChannelBell;
        r.ChannelEmail = i.ChannelEmail;
        r.ChannelSms = i.ChannelSms;
        r.IsActive = i.IsActive;
    }

    [HttpGet]
    [RequirePermission("Settings.View")]
    public async Task<ActionResult<ApiResponse<List<RuleDto>>>> List(CancellationToken ct)
    {
        var rows = await _db.NotificationRules.AsNoTracking().OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
        return Ok(ApiResponse<List<RuleDto>>.Ok(rows.Select(ToDto).ToList()));
    }

    [HttpPost]
    [RequirePermission("Settings.Edit")]
    public async Task<ActionResult<ApiResponse<RuleDto>>> Create([FromBody] RuleInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) return BadRequest(ApiResponse<RuleDto>.Fail("اسم القاعدة مطلوب"));
        var rule = new NotificationRule();
        Apply(rule, input);
        _db.NotificationRules.Add(rule);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<RuleDto>.Ok(ToDto(rule)));
    }

    [HttpPut("{id:guid}")]
    [RequirePermission("Settings.Edit")]
    public async Task<ActionResult<ApiResponse<RuleDto>>> Update(Guid id, [FromBody] RuleInput input, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) return BadRequest(ApiResponse<RuleDto>.Fail("اسم القاعدة مطلوب"));
        var rule = await _db.NotificationRules.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rule is null) return NotFound(ApiResponse<RuleDto>.Fail("القاعدة غير موجودة"));
        Apply(rule, input);
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse<RuleDto>.Ok(ToDto(rule)));
    }

    [HttpDelete("{id:guid}")]
    [RequirePermission("Settings.Edit")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id, CancellationToken ct)
    {
        var rule = await _db.NotificationRules.FirstOrDefaultAsync(r => r.Id == id, ct);
        if (rule is null) return NotFound(ApiResponse.Fail("القاعدة غير موجودة"));
        rule.IsDeleted = true;
        rule.DeletedAt = DateTime.UtcNow;
        rule.DeletedBy = _user.Email;
        await _db.SaveChangesAsync(ct);
        return Ok(ApiResponse.Ok("تم حذف القاعدة"));
    }

    /// <summary>Run the document-expiry scan now (also runs automatically in the background).</summary>
    [HttpPost("run")]
    [RequirePermission("Settings.Edit")]
    public async Task<ActionResult<ApiResponse<int>>> Run(CancellationToken ct)
    {
        var count = await _scanner.RunAsync(ct);
        return Ok(ApiResponse<int>.Ok(count, $"تم إنشاء {count} تنبيه"));
    }
}
