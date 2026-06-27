using System.Text.Json;
using HR.Application.Common.Interfaces;
using HR.Application.Engines.Audit;
using HR.Application.Engines.Settlement;
using HR.Domain.Engines.MasterData;
using HR.Domain.Engines.Settlement;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using HR.Modules.Employees.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Settlement;

/// <summary>Loads the employee, resolves the monthly wage (basic + active allowances) and the total
/// unpaid-leave days over the service period, then defers the statutory math to
/// <see cref="EndOfServiceCalculator"/>. <see cref="SettleAsync"/> additionally persists the
/// settlement and transitions the employee's status.</summary>
public class EndOfServiceEngine : IEndOfServiceEngine
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditEngine _audit;

    public EndOfServiceEngine(ApplicationDbContext db, ICurrentUserService currentUser, IAuditEngine audit)
    {
        _db = db; _currentUser = currentUser; _audit = audit;
    }

    public async Task<SettlementResult> PreviewAsync(SettlementRequest request, CancellationToken ct = default)
    {
        var (employee, input) = await BuildInputAsync(request, ct);
        _ = employee;
        return EndOfServiceCalculator.Calculate(input);
    }

    public async Task<TerminationSettlement> SettleAsync(SettlementRequest request, CancellationToken ct = default)
    {
        var (employee, input) = await BuildInputAsync(request, ct);
        var result = EndOfServiceCalculator.Calculate(input);

        var settlement = new TerminationSettlement
        {
            EmployeeId = employee.Id,
            HireDate = input.HireDate,
            TerminationDate = input.TerminationDate,
            Scenario = request.Scenario,
            ContractTermType = request.ContractTermType,
            MonthlyWage = result.MonthlyWage,
            DailyWage = result.DailyWage,
            ServiceYears = result.ServiceYears,
            EffectiveServiceDays = result.EffectiveServiceDays,
            UnpaidLeaveDays = result.UnpaidLeaveDays,
            GratuityAmount = result.GratuityAmount,
            Article77Award = result.Article77Award,
            NoticeCompensation = result.NoticeCompensation,
            TotalAward = result.TotalAward,
            Currency = result.Currency,
            ComputedByUserId = _currentUser.UserId,
            ComputedAt = DateTime.UtcNow,
            Notes = request.Notes,
        };
        foreach (var line in result.Lines)
            settlement.Items.Add(new TerminationSettlementItem
            {
                LabelEn = line.LabelEn, LabelAr = line.LabelAr, ArticleRef = line.ArticleRef, Amount = line.Amount,
            });

        _db.TerminationSettlements.Add(settlement);

        // Transition the employee: resignation scenarios → Resigned, otherwise Terminated.
        employee.Status = request.Scenario is TerminationScenario.NormalResignation
            or TerminationScenario.Article81EmployerBreachResignation
            ? EmployeeStatus.Resigned
            : EmployeeStatus.Terminated;
        employee.TerminationDate = input.TerminationDate;
        employee.ContractTermType = request.ContractTermType;
        if (input.ContractEndDate is { } end) employee.ContractEndDate = end;

        await _audit.LogChange("Employee", employee.Id, "Terminated",
            new { employee.Status, employee.TerminationDate },
            new { request.Scenario, result.TotalAward, result.GratuityAmount, result.Article77Award }, ct);

        await _db.SaveChangesAsync(ct);
        return settlement;
    }

    private async Task<(Employee employee, SettlementInput input)> BuildInputAsync(SettlementRequest request, CancellationToken ct)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, ct)
            ?? throw new KeyNotFoundException("Employee not found");

        // Incoming dates arrive as Kind=Unspecified (JSON "yyyy-MM-dd"); PostgreSQL timestamptz columns
        // require Kind=Utc, so normalize before any query/persist (matches the rest of the codebase).
        var hireDate = AsUtc(employee.HireDate);
        var terminationDate = AsUtc(request.TerminationDate);
        var contractEnd = request.ContractEndDate is { } ce ? AsUtc(ce)
            : employee.ContractEndDate is { } ece ? AsUtc(ece) : (DateTime?)null;

        var monthlyWage = await ResolveMonthlyWageAsync(employee, ct);
        var unpaidDays = await SumUnpaidLeaveDaysAsync(employee.Id, hireDate, terminationDate, ct);

        var input = new SettlementInput
        {
            MonthlyWage = monthlyWage,
            HireDate = hireDate,
            TerminationDate = terminationDate,
            ContractTermType = request.ContractTermType,
            ContractEndDate = contractEnd,
            Scenario = request.Scenario,
            UnpaidLeaveDays = unpaidDays,
        };
        return (employee, input);
    }

    private static DateTime AsUtc(DateTime d) => DateTime.SpecifyKind(d, DateTimeKind.Utc);

    /// <summary>Wage base = basic salary + active allowances. Matches the gross-salary basis used in
    /// EmployeeProjection (the EOSB award is computed on the full wage, not just the basic).</summary>
    private async Task<decimal> ResolveMonthlyWageAsync(Employee employee, CancellationToken ct)
    {
        var allowances = await _db.EmployeeAllowances.AsNoTracking()
            .Where(a => a.EmployeeId == employee.Id && a.IsActive)
            .SumAsync(a => (decimal?)a.Amount, ct) ?? 0m;
        return employee.BasicSalary + allowances;
    }

    /// <summary>Total days of unpaid (Paid=false) leave the employee took within the service window —
    /// shifts the seniority service end-date for the gratuity calculation.</summary>
    private async Task<decimal> SumUnpaidLeaveDaysAsync(Guid employeeId, DateTime hireDate, DateTime terminationDate, CancellationToken ct)
    {
        var unpaidTypeIds = await UnpaidLeaveTypeIdsAsync(ct);
        if (unpaidTypeIds.Count == 0) return 0m;

        return await _db.LeaveRecords.AsNoTracking()
            .Where(r => r.EmployeeId == employeeId
                        && r.Status != LeaveRecordStatus.Canceled
                        && unpaidTypeIds.Contains(r.LeaveTypeId)
                        && r.StartDate >= hireDate && r.StartDate <= terminationDate)
            .SumAsync(r => (decimal?)r.DaysCount, ct) ?? 0m;
    }

    /// <summary>LeaveType master-data items whose rules mark them unpaid ("paid": false).</summary>
    private async Task<List<Guid>> UnpaidLeaveTypeIdsAsync(CancellationToken ct)
    {
        var types = await _db.MasterDataItems.AsNoTracking()
            .Where(m => m.ObjectType == MasterDataObjectType.LeaveType)
            .Select(m => new { m.Id, m.MetadataJson }).ToListAsync(ct);
        return types.Where(t => !IsPaid(t.MetadataJson)).Select(t => t.Id).ToList();
    }

    /// <summary>Reads the "paid" flag from a LeaveType's rules JSON (case-insensitive). Defaults to paid.</summary>
    private static bool IsPaid(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson)) return true;
        try
        {
            using var doc = JsonDocument.Parse(metadataJson);
            if (doc.RootElement.ValueKind != JsonValueKind.Object) return true;
            foreach (var p in doc.RootElement.EnumerateObject())
                if (p.NameEquals("paid") || p.NameEquals("Paid"))
                    return p.Value.ValueKind != JsonValueKind.False;
            return true;
        }
        catch { return true; }
    }
}
