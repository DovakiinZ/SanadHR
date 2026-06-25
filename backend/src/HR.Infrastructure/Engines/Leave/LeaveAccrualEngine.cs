using System.Text.Json;
using HR.Application.Engines.Leave;
using HR.Domain.Engines.Leave;
using HR.Domain.Engines.MasterData;
using HR.Domain.Enums;
using HR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HR.Infrastructure.Engines.Leave;

/// <summary>Ledger-based leave accrual. Posts periodic (monthly) Accrual transactions onto the existing
/// LeaveBalanceTransaction ledger and keeps LeaveBalance.EntitledDays equal to the accrued total per
/// year. The daily accrual rate follows Saudi Labor Law Art. 109: 21 days/year for the first five
/// years of service, 30 days/year thereafter — the threshold measured against *effective* service
/// (calendar days minus unpaid-leave days). Recalculate owns only the Accrual rows; usage/restoration
/// rows written by the leave lifecycle are never touched.</summary>
public class LeaveAccrualEngine : ILeaveAccrualEngine
{
    private const decimal DaysPerYear = 365.25m;
    private const decimal RateBelow5 = 21m / DaysPerYear;   // ≈ 0.05749 days/day
    private const decimal RateFrom5 = 30m / DaysPerYear;    // ≈ 0.08213 days/day

    private readonly ApplicationDbContext _db;

    public LeaveAccrualEngine(ApplicationDbContext db) => _db = db;

    public async Task<int> RecalculateAsync(Guid employeeId, Guid leaveTypeId, CancellationToken ct = default)
    {
        var employee = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == employeeId, ct);
        if (employee is null || employee.HireDate == default) return 0;

        var start = DateTime.SpecifyKind(employee.HireDate.Date, DateTimeKind.Utc);
        var windowEnd = employee.TerminationDate is { } term && term.Date < DateTime.UtcNow.Date
            ? DateTime.SpecifyKind(term.Date, DateTimeKind.Utc)
            : DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
        if (windowEnd < start) return 0;

        var unpaid = await LoadUnpaidRangesAsync(employeeId, ct);

        // Drop the existing accrual rows — this method fully owns them.
        var existing = await _db.LeaveBalanceTransactions
            .Where(t => t.EmployeeId == employeeId && t.LeaveTypeId == leaveTypeId && t.Type == LeaveTransactionType.Accrual)
            .ToListAsync(ct);
        if (existing.Count > 0) _db.LeaveBalanceTransactions.RemoveRange(existing);

        var posted = 0;
        decimal cumulativeAccrued = 0m;
        var yearAccrued = new Dictionary<int, decimal>();

        decimal monthSum = 0m;
        int effectivePaidDays = 0;            // effective service used for the seniority rate
        int curYear = start.Year, curMonth = start.Month;

        void Flush(int year, int month)
        {
            var delta = Math.Round(monthSum, 2);
            monthSum = 0m;
            if (delta <= 0m) return;
            cumulativeAccrued += delta;
            _db.LeaveBalanceTransactions.Add(new LeaveBalanceTransaction
            {
                EmployeeId = employeeId, LeaveTypeId = leaveTypeId, Year = year,
                Type = LeaveTransactionType.Accrual, LeaveRecordId = null,
                Delta = delta, BalanceAfter = cumulativeAccrued,
                Reason = "استحقاق إجازة شهري", // monthly leave accrual
                ActorUserId = null,
                At = EndOfMonthUtc(year, month),
            });
            yearAccrued[year] = yearAccrued.GetValueOrDefault(year) + delta;
            posted++;
        }

        for (var d = start; d <= windowEnd; d = d.AddDays(1))
        {
            if (d.Year != curYear || d.Month != curMonth)
            {
                Flush(curYear, curMonth);
                curYear = d.Year; curMonth = d.Month;
            }
            if (IsUnpaid(d, unpaid)) continue;   // no accrual, no seniority progress during unpaid leave

            var years = effectivePaidDays / DaysPerYear;
            monthSum += years >= 5m ? RateFrom5 : RateBelow5;
            effectivePaidDays++;
        }
        Flush(curYear, curMonth);

        // Keep each year's EntitledDays equal to that year's accrued total (accrual owns entitlement).
        foreach (var (year, accrued) in yearAccrued)
        {
            var bal = await _db.LeaveBalances
                .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.LeaveTypeId == leaveTypeId && b.Year == year, ct);
            if (bal is null)
            {
                bal = new LeaveBalance { EmployeeId = employeeId, LeaveTypeId = leaveTypeId, Year = year, UsedDays = 0m };
                _db.LeaveBalances.Add(bal);
            }
            bal.EntitledDays = Math.Round(accrued, 2);
        }

        await _db.SaveChangesAsync(ct);
        return posted;
    }

    public async Task<LeaveLedgerView> GetLedgerAsync(Guid employeeId, Guid leaveTypeId, CancellationToken ct = default)
    {
        var txns = await _db.LeaveBalanceTransactions.AsNoTracking()
            .Where(t => t.EmployeeId == employeeId && t.LeaveTypeId == leaveTypeId)
            .OrderBy(t => t.At).ThenBy(t => t.Type == LeaveTransactionType.Accrual ? 0 : 1)
            .ToListAsync(ct);

        var unpaid = await LoadUnpaidRangesAsync(employeeId, ct);

        decimal running = 0m, accrued = 0m, used = 0m;
        var entries = new List<LeaveLedgerEntryDto>(txns.Count);
        foreach (var t in txns)
        {
            running += t.Delta;
            if (t.Type == LeaveTransactionType.Accrual) accrued += t.Delta;
            else if (t.Delta < 0m) used += -t.Delta;
            entries.Add(new LeaveLedgerEntryDto(
                Date: t.At,
                Type: t.Type.ToString(),
                Amount: t.Delta,
                RunningBalance: Math.Round(running, 2),
                Reason: t.Reason,
                IsUnpaidPeriod: IsUnpaid(DateTime.SpecifyKind(t.At.Date, DateTimeKind.Utc), unpaid)));
        }

        return new LeaveLedgerView
        {
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            CurrentBalance = Math.Round(running, 2),
            AccruedToDate = Math.Round(accrued, 2),
            UsedToDate = Math.Round(used, 2),
            Entries = entries,
            UnpaidPeriods = unpaid
                .Select(r => new LeaveLedgerGapDto(r.Start, r.End, (decimal)((r.End - r.Start).Days + 1)))
                .ToList(),
        };
    }

    // ── helpers ──────────────────────────────────────────────────────────────────

    private async Task<List<(DateTime Start, DateTime End)>> LoadUnpaidRangesAsync(Guid employeeId, CancellationToken ct)
    {
        var unpaidTypeIds = await UnpaidLeaveTypeIdsAsync(ct);
        if (unpaidTypeIds.Count == 0) return new();

        var rows = await _db.LeaveRecords.AsNoTracking()
            .Where(r => r.EmployeeId == employeeId && r.Status != LeaveRecordStatus.Canceled && unpaidTypeIds.Contains(r.LeaveTypeId))
            .Select(r => new { r.StartDate, r.EndDate })
            .ToListAsync(ct);

        return rows
            .Select(r => (Start: DateTime.SpecifyKind(r.StartDate.Date, DateTimeKind.Utc),
                          End: DateTime.SpecifyKind(r.EndDate.Date, DateTimeKind.Utc)))
            .OrderBy(r => r.Start).ToList();
    }

    private async Task<List<Guid>> UnpaidLeaveTypeIdsAsync(CancellationToken ct)
    {
        var types = await _db.MasterDataItems.AsNoTracking()
            .Where(m => m.ObjectType == MasterDataObjectType.LeaveType)
            .Select(m => new { m.Id, m.MetadataJson }).ToListAsync(ct);
        return types.Where(t => !IsPaid(t.MetadataJson)).Select(t => t.Id).ToList();
    }

    private static bool IsUnpaid(DateTime date, List<(DateTime Start, DateTime End)> ranges)
    {
        foreach (var r in ranges)
            if (date >= r.Start && date <= r.End) return true;
        return false;
    }

    private static DateTime EndOfMonthUtc(int year, int month)
        => DateTime.SpecifyKind(new DateTime(year, month, DateTime.DaysInMonth(year, month)), DateTimeKind.Utc);

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
