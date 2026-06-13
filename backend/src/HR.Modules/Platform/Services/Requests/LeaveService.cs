using System.Text.Json;
using HR.Application.Common.Interfaces;
using HR.Domain.Engines.MasterData;
using HR.Domain.Engines.Requests;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Modules.Platform.Services.Requests;

/// <summary>
/// Leave rules, balances, day calculation, approver resolution and validation.
/// Rules live on each LeaveType master-data item's MetadataJson — never hardcoded —
/// so adding/changing a leave type in settings changes behavior everywhere.
/// </summary>
public interface ILeaveService
{
    LeaveRules GetRules(string? metadataJson);
    decimal ComputeDays(DateTime start, DateTime end, LeaveRules rules);
    Task<List<LeaveTypeInfo>> GetLeaveTypesAsync(Guid employeeId, CancellationToken ct);
    Task<LeavePreview> PreviewAsync(Guid employeeId, Guid leaveTypeId, DateTime? start, DateTime? end, bool hasAttachment, CancellationToken ct);
}

public sealed class LeaveRules
{
    public bool Paid { get; set; } = true;
    public double PaidPercentage { get; set; } = 100;
    public double MaxDays { get; set; } = 30;
    public double AnnualBalance { get; set; } = 30;
    public bool RequiresAttachment { get; set; }
    public bool AffectsPayroll { get; set; }
    public bool AffectsAttendance { get; set; } = true;
    public bool CountWeekends { get; set; }
    public bool CountHolidays { get; set; }
}

public sealed class LeaveTypeInfo
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public LeaveRules Rules { get; set; } = new();
    public decimal EntitledDays { get; set; }
    public decimal UsedDays { get; set; }
    public decimal RemainingDays { get; set; }
}

public sealed class LeavePreview
{
    public Guid LeaveTypeId { get; set; }
    public decimal Days { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public bool RequiresAttachment { get; set; }
    public bool AffectsPayroll { get; set; }
    public bool AffectsAttendance { get; set; }
    public double PaidPercentage { get; set; }
    public bool Paid { get; set; }
    public string? NextApproverAr { get; set; }
    public string? NextApproverEn { get; set; }
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
}

public sealed class LeaveService : ILeaveService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _user;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public LeaveService(ApplicationDbContext db, ICurrentUserService user) { _db = db; _user = user; }

    public LeaveRules GetRules(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return new LeaveRules();
        try { return JsonSerializer.Deserialize<LeaveRules>(metadataJson, Json) ?? new LeaveRules(); }
        catch { return new LeaveRules(); }
    }

    public decimal ComputeDays(DateTime start, DateTime end, LeaveRules rules)
    {
        if (end < start) return 0;
        decimal days = 0;
        for (var d = start.Date; d <= end.Date; d = d.AddDays(1))
        {
            var isWeekend = d.DayOfWeek is DayOfWeek.Friday or DayOfWeek.Saturday;
            if (rules.CountWeekends || !isWeekend) days++;
        }
        return days;
    }

    public async Task<List<LeaveTypeInfo>> GetLeaveTypesAsync(Guid employeeId, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var types = await _db.MasterDataItems
            .Where(m => m.ObjectType == MasterDataObjectType.LeaveType && m.IsActive)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.NameAr).ToListAsync(ct);
        var balances = await _db.LeaveBalances
            .Where(b => b.EmployeeId == employeeId && b.Year == year).ToListAsync(ct);

        var result = new List<LeaveTypeInfo>();
        foreach (var t in types)
        {
            var rules = GetRules(t.MetadataJson);
            var bal = balances.FirstOrDefault(b => b.LeaveTypeId == t.Id);
            var entitled = bal?.EntitledDays ?? (decimal)rules.AnnualBalance;
            var used = bal?.UsedDays ?? 0m;
            var carried = bal?.CarriedForwardDays ?? 0m;
            result.Add(new LeaveTypeInfo
            {
                Id = t.Id, Code = t.Code, NameAr = t.NameAr, NameEn = t.NameEn, Rules = rules,
                EntitledDays = entitled, UsedDays = used, RemainingDays = entitled + carried - used,
            });
        }
        return result;
    }

    public async Task<LeavePreview> PreviewAsync(Guid employeeId, Guid leaveTypeId, DateTime? start, DateTime? end, bool hasAttachment, CancellationToken ct)
    {
        var preview = new LeavePreview { LeaveTypeId = leaveTypeId };
        var type = await _db.MasterDataItems.FirstOrDefaultAsync(m => m.Id == leaveTypeId && m.ObjectType == MasterDataObjectType.LeaveType, ct);
        if (type is null) { preview.Errors.Add("نوع الإجازة غير موجود"); return preview; }

        var rules = GetRules(type.MetadataJson);
        preview.RequiresAttachment = rules.RequiresAttachment;
        preview.AffectsPayroll = rules.AffectsPayroll;
        preview.AffectsAttendance = rules.AffectsAttendance;
        preview.PaidPercentage = rules.PaidPercentage;
        preview.Paid = rules.Paid;

        var (entitled, used, remaining) = await BalanceAsync(employeeId, leaveTypeId, rules, ct);
        preview.BalanceBefore = remaining;

        if (start is { } s && end is { } e)
        {
            if (e < s) preview.Errors.Add("تاريخ النهاية قبل تاريخ البداية");
            preview.Days = ComputeDays(s, e, rules);
            preview.BalanceAfter = remaining - (rules.Paid ? preview.Days : 0);

            if (rules.MaxDays > 0 && (double)preview.Days > rules.MaxDays)
                preview.Errors.Add($"الحد الأقصى لهذه الإجازة {rules.MaxDays} يوم");
            if (rules.Paid && preview.Days > remaining)
                preview.Errors.Add($"الرصيد غير كافٍ (المتبقي {remaining} يوم)");
            if (await HasOverlapAsync(employeeId, s, e, ct))
                preview.Errors.Add("التواريخ تتعارض مع إجازة أخرى");
        }
        else
        {
            preview.Errors.Add("يرجى تحديد التواريخ");
        }

        if (rules.RequiresAttachment && !hasAttachment)
            preview.Errors.Add("هذه الإجازة تتطلب مرفقاً");

        // Next approver (informational): from the leave request type's workflow first step.
        var (ar, en) = await NextApproverAsync(employeeId, ct);
        preview.NextApproverAr = ar; preview.NextApproverEn = en;

        preview.IsValid = preview.Errors.Count == 0;
        return preview;
    }

    // ── internals ──

    private async Task<(decimal entitled, decimal used, decimal remaining)> BalanceAsync(Guid employeeId, Guid leaveTypeId, LeaveRules rules, CancellationToken ct)
    {
        var year = DateTime.UtcNow.Year;
        var bal = await _db.LeaveBalances.FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year, ct);
        var entitled = bal?.EntitledDays ?? (decimal)rules.AnnualBalance;
        var used = bal?.UsedDays ?? 0m;
        var carried = bal?.CarriedForwardDays ?? 0m;
        return (entitled, used, entitled + carried - used);
    }

    private async Task<bool> HasOverlapAsync(Guid employeeId, DateTime start, DateTime end, CancellationToken ct)
    {
        return await _db.RequestInstances.AnyAsync(r =>
            r.EmployeeId == employeeId &&
            r.LeaveTypeId != null &&
            (r.Status == RequestStatus.Submitted || r.Status == RequestStatus.InProgress || r.Status == RequestStatus.Approved) &&
            r.StartDate != null && r.EndDate != null &&
            r.StartDate <= end.Date && r.EndDate >= start.Date, ct);
    }

    private async Task<(string? ar, string? en)> NextApproverAsync(Guid employeeId, CancellationToken ct)
    {
        var leaveType = await _db.RequestTypes
            .Where(t => t.IsActive && t.ImpactMapping!.AffectsLeaveBalance)
            .OrderBy(t => t.SortOrder).FirstOrDefaultAsync(ct);
        if (leaveType?.WorkflowDefinitionId is not { } wfId) return ("المدير المباشر", "Direct Manager");

        var version = await _db.WorkflowVersions
            .Where(v => v.WorkflowDefinitionId == wfId && v.IsPublished)
            .OrderByDescending(v => v.VersionNumber).FirstOrDefaultAsync(ct);
        if (version?.Configuration is null) return ("المدير المباشر", "Direct Manager");
        try
        {
            var cfg = JsonSerializer.Deserialize<WorkflowChainConfig>(version.Configuration, Json);
            var first = cfg?.Steps.FirstOrDefault();
            if (first is null) return (null, null);
            // Try to resolve the actual person; fall back to the step label.
            var name = await ResolveApproverNameAsync((ApproverType)first.ApproverType, employeeId, ct);
            return (name ?? first.NameAr, name ?? first.NameEn);
        }
        catch { return ("المدير المباشر", "Direct Manager"); }
    }

    private async Task<string?> ResolveApproverNameAsync(ApproverType type, Guid employeeId, CancellationToken ct)
    {
        Guid? managerEmpId = type switch
        {
            ApproverType.DirectManager => await _db.Employees.Where(e => e.Id == employeeId).Select(e => e.ManagerId).FirstOrDefaultAsync(ct),
            _ => null,
        };
        if (managerEmpId is { } mid)
            return await _db.Employees.Where(e => e.Id == mid).Select(e => e.FirstName + " " + e.LastName).FirstOrDefaultAsync(ct);
        return null;
    }
}
