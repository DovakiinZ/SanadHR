using System.Text.Json;
using HR.Application.Engines.Finance;
using HR.Domain.Engines.Finance;
using HR.Domain.Engines.Finance.Entities;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Finance;

/// <summary>Resolves the employee population for a payroll version and builds each employee's fact bag.
/// Mirrors the canonical salary breakdown (basic + capped allowances + additions − deductions − GOSI,
/// with allowance caps and GOSI-inclusion read from master-data metadata) but emits the raw inputs as
/// facts so the rule set — not this provider — decides how they combine.</summary>
public sealed class PayrollFactProvider : IPayrollFactProvider
{
    private readonly ApplicationDbContext _db;

    public PayrollFactProvider(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<EmployeePayrollInput>> BuildInputsAsync(
        PayrollDefinitionVersion version, PayrollPeriod period, CancellationToken ct = default)
    {
        var filter = ParseFilter(version.EmployeeFilterJson);

        var query = _db.Employees.AsNoTracking().AsQueryable();
        if (!filter.IncludeInactive)
            query = query.Where(e => e.Status != EmployeeStatus.Terminated && e.Status != EmployeeStatus.Resigned);
        if (filter.DepartmentIds.Count > 0)
            query = query.Where(e => e.DepartmentId != null && filter.DepartmentIds.Contains(e.DepartmentId.Value));
        if (filter.EmployeeIds.Count > 0)
            query = query.Where(e => filter.EmployeeIds.Contains(e.Id));

        var employees = await query.ToListAsync(ct);
        if (employees.Count == 0) return Array.Empty<EmployeePayrollInput>();

        var empIds = employees.Select(e => e.Id).ToList();

        var allowances = await _db.EmployeeAllowances.AsNoTracking()
            .Where(a => empIds.Contains(a.EmployeeId) && a.IsActive)
            .Select(a => new { a.EmployeeId, a.AllowanceTypeId, a.Amount }).ToListAsync(ct);
        var additions = await _db.EmployeeAdditions.AsNoTracking()
            .Where(a => empIds.Contains(a.EmployeeId) && a.IsActive)
            .Select(a => new { a.EmployeeId, a.Amount }).ToListAsync(ct);
        var deductions = await _db.EmployeeDeductions.AsNoTracking()
            .Where(a => empIds.Contains(a.EmployeeId) && a.IsActive)
            .Select(a => new { a.EmployeeId, a.Amount }).ToListAsync(ct);

        var gosiRate = await _db.CompanyProfiles.Select(c => (decimal?)c.GosiRate).FirstOrDefaultAsync(ct) ?? 9.75m;

        // Allowance-type metadata (cap + GOSI inclusion).
        var allowTypeIds = allowances.Select(a => a.AllowanceTypeId).Distinct().ToList();
        var allowMeta = new Dictionary<Guid, (decimal? Max, bool Gosi)>();
        var md = await _db.MasterDataItems.AsNoTracking()
            .Where(m => allowTypeIds.Contains(m.Id))
            .Select(m => new { m.Id, m.MetadataJson }).ToListAsync(ct);
        foreach (var m in md) allowMeta[m.Id] = ParseAllowanceRules(m.MetadataJson);

        // Department names (for categorical rules like Department == "Sales").
        var depIds = employees.Where(e => e.DepartmentId.HasValue).Select(e => e.DepartmentId!.Value).ToHashSet();
        var deps = await _db.Departments.AsNoTracking().Where(d => depIds.Contains(d.Id))
            .Select(d => new { d.Id, Name = d.NameAr ?? d.Name }).ToDictionaryAsync(d => d.Id, d => d.Name, ct);

        // Attendance aggregates for the period. Shortage on Absent days is excluded here because the
        // whole-day absence is already deducted at the daily-wage rate (avoids double-counting).
        var attendance = await _db.AttendanceRecords.AsNoTracking()
            .Where(a => empIds.Contains(a.EmployeeId) && a.Date >= period.Start && a.Date <= period.End)
            .GroupBy(a => a.EmployeeId)
            .Select(g => new
            {
                EmployeeId = g.Key,
                Days = g.Count(),
                OvertimeMinutes = g.Sum(x => x.OvertimeMinutes),
                LateMinutes = g.Sum(x => x.LateMinutes),
                AbsentDays = g.Count(x => x.Status == AttendanceStatus.Absent),
                ShortageMinutes = g.Sum(x => x.Status == AttendanceStatus.Absent ? 0 : x.ShortageMinutes),
            })
            .ToDictionaryAsync(x => x.EmployeeId, ct);

        var allowByEmp = allowances.GroupBy(a => a.EmployeeId).ToDictionary(g => g.Key, g => g.ToList());
        var addByEmp = additions.GroupBy(a => a.EmployeeId).ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));
        var dedByEmp = deductions.GroupBy(a => a.EmployeeId).ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var inputs = new List<EmployeePayrollInput>(employees.Count);
        foreach (var e in employees)
        {
            decimal totalAllowances = 0m, gosiAllowanceBase = 0m;
            if (allowByEmp.TryGetValue(e.Id, out var al))
            {
                foreach (var a in al)
                {
                    var amount = a.Amount;
                    var rule = allowMeta.TryGetValue(a.AllowanceTypeId, out var r) ? r : (Max: (decimal?)null, Gosi: false);
                    if (rule.Max is { } max && max > 0 && amount > max) amount = max;
                    totalAllowances += amount;
                    if (rule.Gosi) gosiAllowanceBase += amount;
                }
            }

            var totalAdditions = addByEmp.TryGetValue(e.Id, out var ad) ? ad : 0m;
            var totalDeductions = dedByEmp.TryGetValue(e.Id, out var de) ? de : 0m;
            attendance.TryGetValue(e.Id, out var att);
            var hasAttendance = att is not null && att.Days > 0;

            // Wage-rate basis for attendance deductions: full monthly wage / 30 days, / 8 hours.
            var monthlyWage = e.BasicSalary + totalAllowances;
            var dailyWage = Math.Round(monthlyWage / 30m, 4);
            var hourlyWage = Math.Round(dailyWage / 8m, 4);
            var absentDays = att?.AbsentDays ?? 0;
            var lateHours = att is null ? 0m : Math.Round(att.LateMinutes / 60m, 2);
            var shortageHours = att is null ? 0m : Math.Round(att.ShortageMinutes / 60m, 2);

            var facts = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
            {
                ["BasicSalary"] = e.BasicSalary,
                ["TotalAllowances"] = totalAllowances,
                ["GosiAllowanceBase"] = gosiAllowanceBase,
                ["GosiBase"] = e.BasicSalary + gosiAllowanceBase,
                ["TotalAdditions"] = totalAdditions,
                ["TotalDeductions"] = totalDeductions,
                ["GosiRate"] = gosiRate,
                ["WorkedDays"] = att?.Days ?? 0,
                ["OvertimeHours"] = att is null ? 0m : Math.Round(att.OvertimeMinutes / 60m, 2),
                // Attendance-driven deduction inputs (auto-computed from the attendance engine).
                ["DailyWage"] = dailyWage,
                ["HourlyWage"] = hourlyWage,
                ["AbsentDays"] = absentDays,
                ["LateHours"] = lateHours,
                ["ShortageHours"] = shortageHours,
                ["Department"] = e.DepartmentId.HasValue && deps.TryGetValue(e.DepartmentId.Value, out var dn) ? dn : "",
                ["Currency"] = string.IsNullOrWhiteSpace(e.Currency) ? version.Currency : e.Currency,
            };

            inputs.Add(new EmployeePayrollInput
            {
                EmployeeId = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                EmployeeName = $"{e.FirstNameAr ?? e.FirstName} {e.LastNameAr ?? e.LastName}".Trim(),
                Currency = (string.IsNullOrWhiteSpace(e.Currency) ? version.Currency : e.Currency!).ToUpperInvariant(),
                Facts = facts,
                HasAttendanceData = hasAttendance,
            });
        }

        return inputs;
    }

    private sealed record EmployeeFilter(IReadOnlyList<Guid> DepartmentIds, IReadOnlyList<Guid> EmployeeIds, bool IncludeInactive);

    private static EmployeeFilter ParseFilter(string? json)
    {
        var deps = new List<Guid>();
        var emps = new List<Guid>();
        var includeInactive = false;
        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("departmentIds", out var d) && d.ValueKind == JsonValueKind.Array)
                        foreach (var el in d.EnumerateArray()) if (Guid.TryParse(el.GetString(), out var g)) deps.Add(g);
                    if (root.TryGetProperty("employeeIds", out var ei) && ei.ValueKind == JsonValueKind.Array)
                        foreach (var el in ei.EnumerateArray()) if (Guid.TryParse(el.GetString(), out var g)) emps.Add(g);
                    if (root.TryGetProperty("includeInactive", out var inc) &&
                        (inc.ValueKind == JsonValueKind.True || inc.ValueKind == JsonValueKind.False))
                        includeInactive = inc.GetBoolean();
                }
            }
            catch (JsonException) { /* malformed filter → treat as "all active" */ }
        }
        return new EmployeeFilter(deps, emps, includeInactive);
    }

    private static (decimal? Max, bool Gosi) ParseAllowanceRules(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return (null, false);
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return (null, false);
            decimal? max = null; bool gosi = false;
            foreach (var p in doc.RootElement.EnumerateObject())
            {
                if (p.NameEquals("maxAmount") && p.Value.ValueKind == JsonValueKind.Number && p.Value.TryGetDecimal(out var mx))
                    max = mx;
                else if (p.NameEquals("gosiApplicable") && (p.Value.ValueKind == JsonValueKind.True || p.Value.ValueKind == JsonValueKind.False))
                    gosi = p.Value.GetBoolean();
            }
            return (max, gosi);
        }
        catch (JsonException) { return (null, false); }
    }
}
